using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Domain.Interfaces;

public interface IUserFollowRepository
{
    Task<bool> IsFollowingAsync(long followerId, long followingId);
    Task<UserFollow?> GetByFollowerAndFollowingAsync(long followerId, long followingId);
    Task AddAsync(UserFollow follow);
    Task DeleteAsync(long followerId, long followingId);
    Task<int> GetFollowerCountAsync(long userId);
    Task<int> GetFollowingCountAsync(long userId);
    Task<List<long>> GetFollowerIdsAsync(long userId);
    Task<List<long>> GetFollowingIdsAsync(long userId);
    Task DeleteByUserIdAsync(long userId);
}
