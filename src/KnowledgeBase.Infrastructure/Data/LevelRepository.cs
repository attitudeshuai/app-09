using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Infrastructure.Data;

public class LevelRepository : ILevelRepository
{
    private readonly AppDbContext _context;

    public LevelRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Level?> GetByIdAsync(long id)
    {
        return await _context.Levels.FindAsync(id);
    }

    async Task<IEnumerable<Level>> IRepository<Level>.GetAllAsync()
    {
        return await _context.Levels
            .OrderBy(l => l.LevelNumber)
            .ToListAsync();
    }

    public async Task<List<Level>> GetAllAsync()
    {
        return await _context.Levels
            .OrderBy(l => l.LevelNumber)
            .ToListAsync();
    }

    public async Task<Level> AddAsync(Level entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.Levels.AddAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(Level entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Levels.Update(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.Levels.FindAsync(id);
        if (entity != null)
        {
            _context.Levels.Remove(entity);
        }
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.Levels.AnyAsync(l => l.Id == id);
    }

    public async Task<Level?> GetByLevelNumberAsync(int levelNumber)
    {
        return await _context.Levels
            .FirstOrDefaultAsync(l => l.LevelNumber == levelNumber);
    }

    public async Task<Level?> GetLevelByPointsAsync(int points)
    {
        return await _context.Levels
            .Where(l => l.RequiredPoints <= points)
            .OrderByDescending(l => l.LevelNumber)
            .FirstOrDefaultAsync();
    }

    public async Task<Level?> GetNextLevelAsync(int currentLevelNumber)
    {
        return await _context.Levels
            .Where(l => l.LevelNumber > currentLevelNumber)
            .OrderBy(l => l.LevelNumber)
            .FirstOrDefaultAsync();
    }
}
