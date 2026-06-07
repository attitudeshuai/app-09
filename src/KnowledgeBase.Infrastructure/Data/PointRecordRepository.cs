using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Infrastructure.Data;

public class PointRecordRepository : IPointRecordRepository
{
    private readonly AppDbContext _context;

    public PointRecordRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PointRecord?> GetByIdAsync(long id)
    {
        return await _context.PointRecords
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<PointRecord>> GetAllAsync()
    {
        return await _context.PointRecords
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<PointRecord> AddAsync(PointRecord entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.PointRecords.AddAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(PointRecord entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.PointRecords.Update(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.PointRecords.FindAsync(id);
        if (entity != null)
        {
            _context.PointRecords.Remove(entity);
        }
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.PointRecords.AnyAsync(p => p.Id == id);
    }

    public async Task<List<PointRecord>> GetByUserIdAsync(long userId, int pageNumber, int pageSize)
    {
        return await _context.PointRecords
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountByUserIdAsync(long userId)
    {
        return await _context.PointRecords
            .CountAsync(p => p.UserId == userId);
    }

    public async Task<int> GetTotalPointsByUserIdAsync(long userId)
    {
        return await _context.PointRecords
            .Where(p => p.UserId == userId)
            .SumAsync(p => p.Points);
    }

    public async Task<bool> ExistsBySourceAndReferenceAsync(long userId, PointSource source, long referenceId, string referenceType)
    {
        return await _context.PointRecords
            .AnyAsync(p => p.UserId == userId &&
                           p.Source == source &&
                           p.ReferenceId == referenceId &&
                           p.ReferenceType == referenceType);
    }
}
