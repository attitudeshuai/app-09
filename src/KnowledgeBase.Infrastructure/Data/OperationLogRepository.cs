using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Infrastructure.Data;

public class OperationLogRepository : IOperationLogRepository
{
    private readonly AppDbContext _context;

    public OperationLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OperationLog> AddAsync(OperationLog entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.OperationLogs.AddAsync(entity);
        return entity;
    }

    public async Task<(IEnumerable<OperationLog> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        long? userId,
        string? actionType,
        string? targetType,
        DateTime? startTime,
        DateTime? endTime)
    {
        var query = _context.OperationLogs.AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(actionType))
        {
            query = query.Where(o => o.ActionType == actionType);
        }

        if (!string.IsNullOrWhiteSpace(targetType))
        {
            query = query.Where(o => o.TargetType == targetType);
        }

        if (startTime.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= endTime.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
