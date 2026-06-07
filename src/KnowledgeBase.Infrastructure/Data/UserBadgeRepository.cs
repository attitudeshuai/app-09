using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Infrastructure.Data;

public class UserBadgeRepository : IUserBadgeRepository
{
    private readonly AppDbContext _context;

    public UserBadgeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserBadge>> GetByUserIdAsync(long userId)
    {
        return await _context.UserBadges
            .Include(ub => ub.Badge)
            .Where(ub => ub.UserId == userId)
            .OrderBy(ub => ub.EarnedAt)
            .ToListAsync();
    }

    public async Task<bool> HasBadgeAsync(long userId, long badgeId)
    {
        return await _context.UserBadges
            .AnyAsync(ub => ub.UserId == userId && ub.BadgeId == badgeId);
    }

    public async Task AddAsync(UserBadge userBadge)
    {
        userBadge.EarnedAt = DateTime.UtcNow;
        await _context.UserBadges.AddAsync(userBadge);
    }

    public async Task<List<Badge>> GetUserBadgesAsync(long userId)
    {
        return await _context.UserBadges
            .Include(ub => ub.Badge)
            .Where(ub => ub.UserId == userId && ub.Badge != null && ub.Badge.IsActive)
            .Select(ub => ub.Badge!)
            .OrderBy(b => b.Id)
            .ToListAsync();
    }
}
