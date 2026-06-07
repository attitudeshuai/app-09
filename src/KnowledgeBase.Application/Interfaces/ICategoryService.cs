using KnowledgeBase.Application.DTOs.Categories;

namespace KnowledgeBase.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetTreeAsync();
    Task<CategoryDto> GetByIdAsync(long id);
    Task<IEnumerable<CategoryDto>> GetByParentIdAsync(long? parentId);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, long currentUserId);
    Task UpdateAsync(long id, UpdateCategoryRequest request, long currentUserId);
    Task DeleteAsync(long id);
    Task<MigrateDocumentsResult> MigrateDocumentsAsync(long sourceCategoryId, long targetCategoryId, long currentUserId);
}
