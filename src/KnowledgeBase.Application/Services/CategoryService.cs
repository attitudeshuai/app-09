using KnowledgeBase.Application.DTOs.Categories;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;

namespace KnowledgeBase.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private const string CategoryTreeCacheKey = "categories:tree";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public CategoryService(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<CategoryDto>> GetTreeAsync()
    {
        var cached = await _cacheService.GetAsync<IEnumerable<CategoryDto>>(CategoryTreeCacheKey);
        if (cached != null)
        {
            return cached;
        }

        var categories = await _unitOfWork.Categories.GetTreeAsync();
        var result = categories.Select(MapToDtoWithChildren).ToList();

        await _cacheService.SetAsync(CategoryTreeCacheKey, result, CacheDuration);
        return result;
    }

    public async Task<CategoryDto> GetByIdAsync(long id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            throw new KeyNotFoundException("分类不存在");
        }
        return MapToDtoWithChildren(category);
    }

    public async Task<IEnumerable<CategoryDto>> GetByParentIdAsync(long? parentId)
    {
        var categories = await _unitOfWork.Categories.GetByParentIdAsync(parentId);
        return categories.Select(MapToDto);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, long currentUserId)
    {
        if (request.ParentId.HasValue)
        {
            if (!await _unitOfWork.Categories.ExistsAsync(request.ParentId.Value))
            {
                throw new KeyNotFoundException("父级分类不存在");
            }
        }

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            ParentId = request.ParentId,
            SortOrder = request.SortOrder,
            CreatedBy = currentUserId
        };

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();
        await _cacheService.RemoveAsync(CategoryTreeCacheKey);

        return MapToDto(category);
    }

    public async Task UpdateAsync(long id, UpdateCategoryRequest request, long currentUserId)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            throw new KeyNotFoundException("分类不存在");
        }

        if (request.ParentId.HasValue && request.ParentId.Value == id)
        {
            throw new InvalidOperationException("不能将分类设置为自己的子分类");
        }

        if (request.ParentId.HasValue)
        {
            if (!await _unitOfWork.Categories.ExistsAsync(request.ParentId.Value))
            {
                throw new KeyNotFoundException("父级分类不存在");
            }
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.ParentId = request.ParentId;
        category.SortOrder = request.SortOrder;
        category.UpdatedBy = currentUserId;

        await _unitOfWork.Categories.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();
        await _cacheService.RemoveAsync(CategoryTreeCacheKey);
    }

    public async Task DeleteAsync(long id)
    {
        if (!await _unitOfWork.Categories.ExistsAsync(id))
        {
            throw new KeyNotFoundException("分类不存在");
        }

        if (await _unitOfWork.Categories.HasChildrenAsync(id))
        {
            throw new InvalidOperationException("该分类下有子分类，无法删除");
        }

        var documents = await _unitOfWork.Documents.GetByCategoryIdAsync(id);
        if (documents.Any())
        {
            throw new InvalidOperationException("该分类下有文档，无法删除");
        }

        await _unitOfWork.Categories.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
        await _cacheService.RemoveAsync(CategoryTreeCacheKey);
    }

    public async Task<MigrateDocumentsResult> MigrateDocumentsAsync(long sourceCategoryId, long targetCategoryId, long currentUserId)
    {
        if (sourceCategoryId == targetCategoryId)
        {
            throw new InvalidOperationException("源分类和目标分类不能相同");
        }

        if (!await _unitOfWork.Categories.ExistsAsync(sourceCategoryId))
        {
            throw new KeyNotFoundException("源分类不存在");
        }

        if (!await _unitOfWork.Categories.ExistsAsync(targetCategoryId))
        {
            throw new KeyNotFoundException("目标分类不存在");
        }

        var sourceCategory = await _unitOfWork.Categories.GetByIdAsync(sourceCategoryId);
        var targetCategory = await _unitOfWork.Categories.GetByIdAsync(targetCategoryId);

        if (sourceCategory == null || targetCategory == null)
        {
            throw new KeyNotFoundException("分类不存在");
        }

        var isDescendant = await IsDescendantAsync(targetCategoryId, sourceCategoryId);
        if (isDescendant)
        {
            throw new InvalidOperationException("不能将文档迁移到源分类的子分类下");
        }

        var documents = await _unitOfWork.Documents.GetByCategoryIdAsync(sourceCategoryId);
        var documentList = documents.ToList();

        foreach (var document in documentList)
        {
            document.CategoryId = targetCategoryId;
            document.UpdatedBy = currentUserId;
            await _unitOfWork.Documents.UpdateAsync(document);
        }

        await _unitOfWork.SaveChangesAsync();
        await _cacheService.RemoveAsync(CategoryTreeCacheKey);

        return new MigrateDocumentsResult
        {
            MigratedCount = documentList.Count,
            SourceCategoryId = sourceCategoryId,
            TargetCategoryId = targetCategoryId
        };
    }

    private async Task<bool> IsDescendantAsync(long categoryId, long ancestorId)
    {
        var currentId = categoryId;
        var visited = new HashSet<long>();

        while (currentId != 0)
        {
            if (visited.Contains(currentId))
            {
                return false;
            }
            visited.Add(currentId);

            var category = await _unitOfWork.Categories.GetByIdAsync(currentId);
            if (category == null)
            {
                return false;
            }

            if (category.ParentId == ancestorId)
            {
                return true;
            }

            if (!category.ParentId.HasValue)
            {
                return false;
            }

            currentId = category.ParentId.Value;
        }

        return false;
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentId = category.ParentId,
            SortOrder = category.SortOrder,
            CreatedAt = category.CreatedAt
        };
    }

    private static CategoryDto MapToDtoWithChildren(Category category)
    {
        var dto = MapToDto(category);
        if (category.Children != null && category.Children.Any())
        {
            dto.Children = category.Children.Select(MapToDtoWithChildren).ToList();
        }
        return dto;
    }
}
