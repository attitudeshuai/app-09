using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Infrastructure.Data;

public class DocumentLikeRepository : IDocumentLikeRepository
{
    private readonly AppDbContext _context;

    public DocumentLikeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsLikedAsync(long userId, long documentId)
    {
        return await _context.DocumentLikes
            .AnyAsync(dl => dl.UserId == userId && dl.DocumentId == documentId);
    }

    public async Task<DocumentLike?> GetByUserAndDocumentAsync(long userId, long documentId)
    {
        return await _context.DocumentLikes
            .FirstOrDefaultAsync(dl => dl.UserId == userId && dl.DocumentId == documentId);
    }

    public async Task AddAsync(DocumentLike like)
    {
        like.CreatedAt = DateTime.UtcNow;
        await _context.DocumentLikes.AddAsync(like);
    }

    public async Task DeleteAsync(long userId, long documentId)
    {
        var like = await _context.DocumentLikes
            .FirstOrDefaultAsync(dl => dl.UserId == userId && dl.DocumentId == documentId);
        if (like != null)
        {
            _context.DocumentLikes.Remove(like);
        }
    }

    public async Task<int> GetLikeCountAsync(long documentId)
    {
        return await _context.DocumentLikes
            .CountAsync(dl => dl.DocumentId == documentId);
    }

    public async Task<List<long>> GetLikedDocumentIdsAsync(long userId, IEnumerable<long> documentIds)
    {
        return await _context.DocumentLikes
            .Where(dl => dl.UserId == userId && documentIds.Contains(dl.DocumentId))
            .Select(dl => dl.DocumentId)
            .ToListAsync();
    }

    public async Task DeleteByDocumentIdAsync(long documentId)
    {
        var likes = await _context.DocumentLikes
            .Where(dl => dl.DocumentId == documentId)
            .ToListAsync();
        _context.DocumentLikes.RemoveRange(likes);
    }
}
