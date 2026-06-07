using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Domain.Interfaces;

public interface IBadgeRepository : IRepository<Badge>
{
    Task<List<Badge>> GetAllActiveAsync();
    Task<List<Badge>> GetByTypeAsync(BadgeType type);
}
