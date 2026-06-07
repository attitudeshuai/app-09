using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetTreeAsync();
    Task<IEnumerable<Category>> GetByParentIdAsync(long? parentId);
    Task<IEnumerable<Category>> GetAllWithChildrenAsync();
    Task<bool> HasChildrenAsync(long id);
    Task<bool> IsDescendantAsync(long categoryId, long ancestorId);
}
