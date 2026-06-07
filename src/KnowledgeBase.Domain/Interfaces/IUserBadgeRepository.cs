using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Domain.Interfaces;

public interface IUserBadgeRepository
{
    Task<List<UserBadge>> GetByUserIdAsync(long userId);
    Task<bool> HasBadgeAsync(long userId, long badgeId);
    Task AddAsync(UserBadge userBadge);
    Task<List<Badge>> GetUserBadgesAsync(long userId);
}
