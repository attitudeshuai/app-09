using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Domain.Interfaces;

public interface IDocumentLikeRepository
{
    Task<bool> IsLikedAsync(long userId, long documentId);
    Task<DocumentLike?> GetByUserAndDocumentAsync(long userId, long documentId);
    Task AddAsync(DocumentLike like);
    Task DeleteAsync(long userId, long documentId);
    Task<int> GetLikeCountAsync(long documentId);
    Task<List<long>> GetLikedDocumentIdsAsync(long userId, IEnumerable<long> documentIds);
    Task DeleteByDocumentIdAsync(long documentId);
}
