using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Infrastructure.Data;

public class BadgeRepository : IBadgeRepository
{
    private readonly AppDbContext _context;

    public BadgeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Badge?> GetByIdAsync(long id)
    {
        return await _context.Badges.FindAsync(id);
    }

    public async Task<IEnumerable<Badge>> GetAllAsync()
    {
        return await _context.Badges
            .OrderBy(b => b.Id)
            .ToListAsync();
    }

    public async Task<Badge> AddAsync(Badge entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.Badges.AddAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(Badge entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Badges.Update(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.Badges.FindAsync(id);
        if (entity != null)
        {
            _context.Badges.Remove(entity);
        }
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.Badges.AnyAsync(b => b.Id == id);
    }

    public async Task<List<Badge>> GetAllActiveAsync()
    {
        return await _context.Badges
            .Where(b => b.IsActive)
            .OrderBy(b => b.Type)
            .ThenBy(b => b.RequiredValue ?? 0)
            .ToListAsync();
    }

    public async Task<List<Badge>> GetByTypeAsync(BadgeType type)
    {
        return await _context.Badges
            .Where(b => b.Type == type && b.IsActive)
            .OrderBy(b => b.RequiredValue ?? 0)
            .ToListAsync();
    }
}
