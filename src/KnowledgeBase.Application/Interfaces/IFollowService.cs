using KnowledgeBase.Application.DTOs.Follows;

namespace KnowledgeBase.Application.Interfaces;

public interface IFollowService
{
    Task<bool> IsFollowingAsync(long followerId, long followingId);
    Task<int> GetFollowerCountAsync(long userId);
    Task<int> GetFollowingCountAsync(long userId);
    Task<FollowStatusDto> GetFollowStatusAsync(long currentUserId, long targetUserId);
    Task<FollowStatusDto> ToggleFollowAsync(long followerId, long followingId);
    Task<List<FollowerUserDto>> GetFollowersAsync(long userId);
    Task<List<FollowingUserDto>> GetFollowingAsync(long userId);
}
