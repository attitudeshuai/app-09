using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Infrastructure.Data;

public class DocumentViewHistoryRepository : IDocumentViewHistoryRepository
{
    private readonly AppDbContext _context;

    public DocumentViewHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddOrUpdateAsync(long userId, long documentId)
    {
        var history = await _context.DocumentViewHistories
            .FirstOrDefaultAsync(h => h.UserId == userId && h.DocumentId == documentId);

        if (history != null)
        {
            history.ViewedAt = DateTime.UtcNow;
        }
        else
        {
            history = new DocumentViewHistory
            {
                UserId = userId,
                DocumentId = documentId,
                ViewedAt = DateTime.UtcNow
            };
            await _context.DocumentViewHistories.AddAsync(history);
        }
    }

    public async Task<(IEnumerable<DocumentViewHistory> Items, int TotalCount)> GetHistoryByUserIdAsync(
        long userId,
        int pageNumber,
        int pageSize)
    {
        var query = _context.DocumentViewHistories
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.ViewedAt)
            .Include(h => h.Document)
                .ThenInclude(d => d.Category)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<int> GetHistoryCountByUserIdAsync(long userId)
    {
        return await _context.DocumentViewHistories
            .CountAsync(h => h.UserId == userId);
    }

    public async Task CleanupOldRecordsAsync(int maxRecordsPerUser, TimeSpan expirationPeriod)
    {
        var cutoffDate = DateTime.UtcNow - expirationPeriod;

        var expiredRecords = await _context.DocumentViewHistories
            .Where(h => h.ViewedAt < cutoffDate)
            .ToListAsync();

        if (expiredRecords.Any())
        {
            _context.DocumentViewHistories.RemoveRange(expiredRecords);
        }

        var userIds = await _context.DocumentViewHistories
            .Select(h => h.UserId)
            .Distinct()
            .ToListAsync();

        foreach (var userId in userIds)
        {
            var userHistoryCount = await _context.DocumentViewHistories
                .CountAsync(h => h.UserId == userId);

            if (userHistoryCount > maxRecordsPerUser)
            {
                var recordsToRemove = await _context.DocumentViewHistories
                    .Where(h => h.UserId == userId)
                    .OrderBy(h => h.ViewedAt)
                    .Take(userHistoryCount - maxRecordsPerUser)
                    .ToListAsync();

                _context.DocumentViewHistories.RemoveRange(recordsToRemove);
            }
        }
    }

    public async Task DeleteByDocumentIdAsync(long documentId)
    {
        var records = await _context.DocumentViewHistories
            .Where(h => h.DocumentId == documentId)
            .ToListAsync();

        if (records.Any())
        {
            _context.DocumentViewHistories.RemoveRange(records);
        }
    }
}
