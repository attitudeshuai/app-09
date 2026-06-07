using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Domain.Interfaces;

public interface IDocumentFavoriteRepository
{
    Task<bool> IsFavoritedAsync(long userId, long documentId);
    Task<DocumentFavorite?> GetByUserAndDocumentAsync(long userId, long documentId);
    Task AddAsync(DocumentFavorite favorite);
    Task DeleteAsync(long userId, long documentId);
    Task<(IEnumerable<DocumentFavorite> Items, int TotalCount)> GetFavoritesByUserIdAsync(
        long userId,
        int pageNumber,
        int pageSize);
    Task<int> GetFavoriteCountByUserIdAsync(long userId);
    Task<List<long>> GetFavoritedDocumentIdsAsync(long userId, IEnumerable<long> documentIds);
}
