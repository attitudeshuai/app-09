using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Infrastructure.Data;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> GetByIdAsync(long id)
    {
        return await _context.Documents
            .Include(d => d.Category)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<Document>> GetAllAsync()
    {
        return await _context.Documents
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .ToListAsync();
    }

    public async Task<Document> AddAsync(Document entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.Version = 1;
        await _context.Documents.AddAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(Document entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Documents.Update(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.Documents.FindAsync(id);
        if (entity != null)
        {
            _context.Documents.Remove(entity);
        }
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.Documents.AnyAsync(d => d.Id == id);
    }

    public async Task<(IEnumerable<Document> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? keyword,
        long? categoryId,
        DocumentStatus? status)
    {
        var query = _context.Documents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(d => d.Title.Contains(keyword)
                                  || d.Content.Contains(keyword)
                                  || d.Tags != null && d.Tags.Contains(keyword));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(d => d.CategoryId == categoryId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Include(d => d.Category)
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Document>> SearchAsync(string keyword, int pageNumber, int pageSize)
    {
        return await _context.Documents
            .Where(d => d.Status == DocumentStatus.Published &&
                       (d.Title.Contains(keyword) ||
                        d.Content.Contains(keyword) ||
                        (d.Tags != null && d.Tags.Contains(keyword))))
            .Include(d => d.Category)
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> SearchCountAsync(string keyword)
    {
        return await _context.Documents
            .CountAsync(d => d.Status == DocumentStatus.Published &&
                       (d.Title.Contains(keyword) ||
                        d.Content.Contains(keyword) ||
                        (d.Tags != null && d.Tags.Contains(keyword))));
    }

    public async Task IncrementViewCountAsync(long id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document != null)
        {
            document.ViewCount++;
            document.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<IEnumerable<Document>> GetByCategoryIdAsync(long categoryId)
    {
        return await _context.Documents
            .Where(d => d.CategoryId == categoryId)
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> UpdateCategoryIdAsync(long sourceCategoryId, long targetCategoryId, long updatedBy)
    {
        var now = DateTime.UtcNow;
        return await _context.Documents
            .Where(d => d.CategoryId == sourceCategoryId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(d => d.CategoryId, targetCategoryId)
                .SetProperty(d => d.UpdatedAt, now)
                .SetProperty(d => d.UpdatedBy, updatedBy));
    }

    public async Task<IEnumerable<Document>> GetScheduledDocumentsToPublishAsync(DateTime now)
    {
        return await _context.Documents
            .Where(d => d.Status == DocumentStatus.Scheduled
                     && d.PublishTime.HasValue
                     && d.PublishTime.Value <= now)
            .ToListAsync();
    }

    public async Task<int> GetDocumentCountByUserIdAsync(long userId, DocumentStatus? status = null)
    {
        var query = _context.Documents.Where(d => d.CreatedBy == userId);

        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        return await query.CountAsync();
    }
}
