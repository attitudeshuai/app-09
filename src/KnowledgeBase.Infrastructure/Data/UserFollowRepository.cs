using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Infrastructure.Data;

public class UserFollowRepository : IUserFollowRepository
{
    private readonly AppDbContext _context;

    public UserFollowRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsFollowingAsync(long followerId, long followingId)
    {
        return await _context.UserFollows
            .AnyAsync(uf => uf.FollowerId == followerId && uf.FollowingId == followingId);
    }

    public async Task<UserFollow?> GetByFollowerAndFollowingAsync(long followerId, long followingId)
    {
        return await _context.UserFollows
            .FirstOrDefaultAsync(uf => uf.FollowerId == followerId && uf.FollowingId == followingId);
    }

    public async Task AddAsync(UserFollow follow)
    {
        follow.CreatedAt = DateTime.UtcNow;
        await _context.UserFollows.AddAsync(follow);
    }

    public async Task DeleteAsync(long followerId, long followingId)
    {
        var follow = await _context.UserFollows
            .FirstOrDefaultAsync(uf => uf.FollowerId == followerId && uf.FollowingId == followingId);
        if (follow != null)
        {
            _context.UserFollows.Remove(follow);
        }
    }

    public async Task<int> GetFollowerCountAsync(long userId)
    {
        return await _context.UserFollows
            .CountAsync(uf => uf.FollowingId == userId);
    }

    public async Task<int> GetFollowingCountAsync(long userId)
    {
        return await _context.UserFollows
            .CountAsync(uf => uf.FollowerId == userId);
    }

    public async Task<List<long>> GetFollowerIdsAsync(long userId)
    {
        return await _context.UserFollows
            .Where(uf => uf.FollowingId == userId)
            .Select(uf => uf.FollowerId)
            .ToListAsync();
    }

    public async Task<List<long>> GetFollowingIdsAsync(long userId)
    {
        return await _context.UserFollows
            .Where(uf => uf.FollowerId == userId)
            .Select(uf => uf.FollowingId)
            .ToListAsync();
    }

    public async Task DeleteByUserIdAsync(long userId)
    {
        var follows = await _context.UserFollows
            .Where(uf => uf.FollowerId == userId || uf.FollowingId == userId)
            .ToListAsync();
        _context.UserFollows.RemoveRange(follows);
    }
}
