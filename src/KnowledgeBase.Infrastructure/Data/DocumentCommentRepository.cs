using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Infrastructure.Data;

public class DocumentCommentRepository : IDocumentCommentRepository
{
    private readonly AppDbContext _context;

    public DocumentCommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentComment?> GetByIdAsync(long id)
    {
        return await _context.DocumentComments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<DocumentComment>> GetAllAsync()
    {
        return await _context.DocumentComments
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<DocumentComment> AddAsync(DocumentComment entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.DocumentComments.AddAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(DocumentComment entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.DocumentComments.Update(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.DocumentComments.FindAsync(id);
        if (entity != null)
        {
            _context.DocumentComments.Remove(entity);
        }
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.DocumentComments.AnyAsync(c => c.Id == id);
    }

    public async Task<(IEnumerable<DocumentComment> Items, int TotalCount)> GetPagedByDocumentIdAsync(
        long documentId,
        int pageNumber,
        int pageSize,
        bool sortDescending = false)
    {
        var query = _context.DocumentComments
            .Where(c => c.DocumentId == documentId && c.ParentId == null)
            .Include(c => c.User)
            .AsQueryable();

        query = sortDescending
            ? query.OrderByDescending(c => c.CreatedAt)
            : query.OrderBy(c => c.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<DocumentComment>> GetAllByDocumentIdAsync(long documentId, bool sortDescending = false)
    {
        var query = _context.DocumentComments
            .Where(c => c.DocumentId == documentId)
            .Include(c => c.User)
            .Include(c => c.ReplyToUser)
            .AsQueryable();

        query = sortDescending
            ? query.OrderByDescending(c => c.CreatedAt)
            : query.OrderBy(c => c.CreatedAt);

        return await query.ToListAsync();
    }

    public async Task<List<DocumentComment>> GetRepliesByParentIdAsync(long parentId, bool sortDescending = false)
    {
        var query = _context.DocumentComments
            .Where(c => c.ParentId == parentId)
            .Include(c => c.User)
            .Include(c => c.ReplyToUser)
            .AsQueryable();

        query = sortDescending
            ? query.OrderByDescending(c => c.CreatedAt)
            : query.OrderBy(c => c.CreatedAt);

        return await query.ToListAsync();
    }

    public async Task<int> CountByDocumentIdAsync(long documentId)
    {
        return await _context.DocumentComments
            .CountAsync(c => c.DocumentId == documentId);
    }

    public async Task<int> CountRootCommentsByDocumentIdAsync(long documentId)
    {
        return await _context.DocumentComments
            .CountAsync(c => c.DocumentId == documentId && c.ParentId == null);
    }
}
