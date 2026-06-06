using KnowledgeBase.Application.DTOs.Categories;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;

namespace KnowledgeBase.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CategoryDto>> GetTreeAsync()
    {
        var categories = await _unitOfWork.Categories.GetTreeAsync();
        return categories.Select(MapToDtoWithChildren);
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
