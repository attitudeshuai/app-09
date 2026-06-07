using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Infrastructure.Data;

public class UserPasswordHistoryRepository : IUserPasswordHistoryRepository
{
    private readonly AppDbContext _context;

    public UserPasswordHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserPasswordHistory?> GetByIdAsync(long id)
    {
        return await _context.UserPasswordHistories.FindAsync(id);
    }

    public async Task<IEnumerable<UserPasswordHistory>> GetAllAsync()
    {
        return await _context.UserPasswordHistories.OrderByDescending(h => h.CreatedAt).ToListAsync();
    }

    public async Task<UserPasswordHistory> AddAsync(UserPasswordHistory entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.UserPasswordHistories.AddAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(UserPasswordHistory entity)
    {
        _context.UserPasswordHistories.Update(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.UserPasswordHistories.FindAsync(id);
        if (entity != null)
        {
            _context.UserPasswordHistories.Remove(entity);
        }
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.UserPasswordHistories.AnyAsync(h => h.Id == id);
    }

    public async Task<IEnumerable<UserPasswordHistory>> GetRecentByUserIdAsync(long userId, int count)
    {
        return await _context.UserPasswordHistories
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task AddAsync(long userId, string passwordHash)
    {
        var history = new UserPasswordHistory
        {
            UserId = userId,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };
        await _context.UserPasswordHistories.AddAsync(history);
    }
}
