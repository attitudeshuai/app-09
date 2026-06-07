using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Infrastructure.Data;

public class DocumentFavoriteRepository : IDocumentFavoriteRepository
{
    private readonly AppDbContext _context;

    public DocumentFavoriteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsFavoritedAsync(long userId, long documentId)
    {
        return await _context.DocumentFavorites
            .AnyAsync(df => df.UserId == userId && df.DocumentId == documentId);
    }

    public async Task<DocumentFavorite?> GetByUserAndDocumentAsync(long userId, long documentId)
    {
        return await _context.DocumentFavorites
            .FirstOrDefaultAsync(df => df.UserId == userId && df.DocumentId == documentId);
    }

    public async Task AddAsync(DocumentFavorite favorite)
    {
        favorite.CreatedAt = DateTime.UtcNow;
        await _context.DocumentFavorites.AddAsync(favorite);
    }

    public async Task DeleteAsync(long userId, long documentId)
    {
        var favorite = await _context.DocumentFavorites
            .FirstOrDefaultAsync(df => df.UserId == userId && df.DocumentId == documentId);
        if (favorite != null)
        {
            _context.DocumentFavorites.Remove(favorite);
        }
    }

    public async Task<(IEnumerable<DocumentFavorite> Items, int TotalCount)> GetFavoritesByUserIdAsync(
        long userId,
        int pageNumber,
        int pageSize)
    {
        var query = _context.DocumentFavorites
            .Where(df => df.UserId == userId)
            .OrderByDescending(df => df.CreatedAt)
            .Include(df => df.Document)
                .ThenInclude(d => d.Category)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<int> GetFavoriteCountByUserIdAsync(long userId)
    {
        return await _context.DocumentFavorites
            .CountAsync(df => df.UserId == userId);
    }

    public async Task<List<long>> GetFavoritedDocumentIdsAsync(long userId, IEnumerable<long> documentIds)
    {
        return await _context.DocumentFavorites
            .Where(df => df.UserId == userId && documentIds.Contains(df.DocumentId))
            .Select(df => df.DocumentId)
            .ToListAsync();
    }
}
