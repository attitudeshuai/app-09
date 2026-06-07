using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.DTOs.Documents;
using KnowledgeBase.Application.DTOs.Favorites;

namespace KnowledgeBase.Application.Interfaces;

public interface IFavoriteService
{
    Task<bool> IsFavoritedAsync(long userId, long documentId);
    Task AddFavoriteAsync(long userId, long documentId);
    Task RemoveFavoriteAsync(long userId, long documentId);
    Task<PagedResult<FavoriteDocumentDto>> GetMyFavoritesAsync(long userId, FavoritePagedRequest request);
}
