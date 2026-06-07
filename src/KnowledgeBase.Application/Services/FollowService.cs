using KnowledgeBase.Application.DTOs.Follows;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;

namespace KnowledgeBase.Application.Services;

public class FollowService : IFollowService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private const string FollowerCountCachePrefix = "follows:follower-count:";
    private const string FollowingCountCachePrefix = "follows:following-count:";
    private const string UserFollowCachePrefix = "follows:user:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
    private const int CacheRemoveMaxRetries = 3;
    private static readonly TimeSpan CacheRemoveRetryDelay = TimeSpan.FromMilliseconds(100);

    public FollowService(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<bool> IsFollowingAsync(long followerId, long followingId)
    {
        if (followerId <= 0 || followingId <= 0 || followerId == followingId)
        {
            return false;
        }

        var cacheKey = GetUserFollowCacheKey(followerId, followingId);
        var cached = await _cacheService.GetAsync<bool?>(cacheKey);
        if (cached.HasValue)
        {
            return cached.Value;
        }

        var isFollowing = await _unitOfWork.UserFollows.IsFollowingAsync(followerId, followingId);
        await _cacheService.SetAsync(cacheKey, isFollowing, CacheDuration);
        return isFollowing;
    }

    public async Task<int> GetFollowerCountAsync(long userId)
    {
        if (userId <= 0)
        {
            return 0;
        }

        var cacheKey = GetFollowerCountCacheKey(userId);
        var cached = await _cacheService.GetAsync<int?>(cacheKey);
        if (cached.HasValue)
        {
            return cached.Value;
        }

        var count = await _unitOfWork.UserFollows.GetFollowerCountAsync(userId);
        await _cacheService.SetAsync(cacheKey, count, CacheDuration);
        return count;
    }

    public async Task<int> GetFollowingCountAsync(long userId)
    {
        if (userId <= 0)
        {
            return 0;
        }

        var cacheKey = GetFollowingCountCacheKey(userId);
        var cached = await _cacheService.GetAsync<int?>(cacheKey);
        if (cached.HasValue)
        {
            return cached.Value;
        }

        var count = await _unitOfWork.UserFollows.GetFollowingCountAsync(userId);
        await _cacheService.SetAsync(cacheKey, count, CacheDuration);
        return count;
    }

    public async Task<FollowStatusDto> GetFollowStatusAsync(long currentUserId, long targetUserId)
    {
        var targetUser = await _unitOfWork.Users.GetByIdAsync(targetUserId);
        if (targetUser == null)
        {
            throw new KeyNotFoundException("用户不存在");
        }

        var isFollowing = await IsFollowingAsync(currentUserId, targetUserId);
        var followerCount = await GetFollowerCountAsync(targetUserId);
        var followingCount = await GetFollowingCountAsync(targetUserId);

        return new FollowStatusDto
        {
            UserId = targetUserId,
            IsFollowing = isFollowing,
            FollowerCount = followerCount,
            FollowingCount = followingCount
        };
    }

    public async Task<FollowStatusDto> ToggleFollowAsync(long followerId, long followingId)
    {
        if (followerId == followingId)
        {
            throw new InvalidOperationException("不能关注自己");
        }

        var follower = await _unitOfWork.Users.GetByIdAsync(followerId);
        if (follower == null)
        {
            throw new KeyNotFoundException("关注者不存在");
        }

        var following = await _unitOfWork.Users.GetByIdAsync(followingId);
        if (following == null)
        {
            throw new KeyNotFoundException("被关注用户不存在");
        }

        if (!following.IsActive)
        {
            throw new InvalidOperationException("无法关注已禁用的用户");
        }

        var isFollowing = await _unitOfWork.UserFollows.IsFollowingAsync(followerId, followingId);

        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (isFollowing)
            {
                await _unitOfWork.UserFollows.DeleteAsync(followerId, followingId);
            }
            else
            {
                var follow = new UserFollow
                {
                    FollowerId = followerId,
                    FollowingId = followingId
                };
                await _unitOfWork.UserFollows.AddAsync(follow);
            }

            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            var newIsFollowing = !isFollowing;
            var followerCount = await GetFollowerCountAsync(followingId);
            var followingCount = await GetFollowingCountAsync(followerId);

            await UpdateCacheAsync(followerId, followingId, newIsFollowing, followerCount, followingCount);

            return new FollowStatusDto
            {
                UserId = followingId,
                IsFollowing = newIsFollowing,
                FollowerCount = followerCount,
                FollowingCount = followingCount
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<FollowerUserDto>> GetFollowersAsync(long userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("用户不存在");
        }

        var followerIds = await _unitOfWork.UserFollows.GetFollowerIdsAsync(userId);
        if (!followerIds.Any())
        {
            return new List<FollowerUserDto>();
        }

        var followers = new List<FollowerUserDto>();
        foreach (var followerId in followerIds)
        {
            var follower = await _unitOfWork.Users.GetByIdAsync(followerId);
            if (follower != null && follower.IsActive)
            {
                var follow = await _unitOfWork.UserFollows.GetByFollowerAndFollowingAsync(followerId, userId);
                followers.Add(new FollowerUserDto
                {
                    Id = follower.Id,
                    Username = follower.Username,
                    Nickname = follower.Nickname,
                    Avatar = follower.Avatar,
                    FollowedAt = follow?.CreatedAt ?? DateTime.UtcNow
                });
            }
        }

        return followers;
    }

    public async Task<List<FollowingUserDto>> GetFollowingAsync(long userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("用户不存在");
        }

        var followingIds = await _unitOfWork.UserFollows.GetFollowingIdsAsync(userId);
        if (!followingIds.Any())
        {
            return new List<FollowingUserDto>();
        }

        var followingList = new List<FollowingUserDto>();
        foreach (var followingId in followingIds)
        {
            var following = await _unitOfWork.Users.GetByIdAsync(followingId);
            if (following != null && following.IsActive)
            {
                var follow = await _unitOfWork.UserFollows.GetByFollowerAndFollowingAsync(userId, followingId);
                followingList.Add(new FollowingUserDto
                {
                    Id = following.Id,
                    Username = following.Username,
                    Nickname = following.Nickname,
                    Avatar = following.Avatar,
                    FollowedAt = follow?.CreatedAt ?? DateTime.UtcNow
                });
            }
        }

        return followingList;
    }

    private async Task UpdateCacheAsync(long followerId, long followingId, bool isFollowing, int followerCount, int followingCount)
    {
        var userFollowCacheKey = GetUserFollowCacheKey(followerId, followingId);
        var followerCountCacheKey = GetFollowerCountCacheKey(followingId);
        var followingCountCacheKey = GetFollowingCountCacheKey(followerId);

        await _cacheService.SetAsync(userFollowCacheKey, isFollowing, CacheDuration);
        await _cacheService.SetAsync(followerCountCacheKey, followerCount, CacheDuration);
        await _cacheService.SetAsync(followingCountCacheKey, followingCount, CacheDuration);
    }

    private static string GetFollowerCountCacheKey(long userId)
    {
        return $"{FollowerCountCachePrefix}{userId}";
    }

    private static string GetFollowingCountCacheKey(long userId)
    {
        return $"{FollowingCountCachePrefix}{userId}";
    }

    private static string GetUserFollowCacheKey(long followerId, long followingId)
    {
        return $"{UserFollowCachePrefix}{followerId}:{followingId}";
    }
}
