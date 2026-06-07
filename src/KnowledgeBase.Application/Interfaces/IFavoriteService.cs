using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.DTOs.Documents;
using KnowledgeBase.Application.DTOs.Favorites;
using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Application.Interfaces;

public interface IFavoriteService
{
    Task<bool> IsFavoritedAsync(long userId, long documentId);
    Task AddFavoriteAsync(long userId, long documentId, UserRole? userRole = null);
    Task RemoveFavoriteAsync(long userId, long documentId);
    Task<PagedResult<FavoriteDocumentDto>> GetMyFavoritesAsync(long userId, FavoritePagedRequest request, UserRole? userRole = null);
}
