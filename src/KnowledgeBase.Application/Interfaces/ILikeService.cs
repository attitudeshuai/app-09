using KnowledgeBase.Application.DTOs.Likes;
using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Application.Interfaces;

public interface ILikeService
{
    Task<bool> IsLikedAsync(long userId, long documentId);
    Task<int> GetLikeCountAsync(long documentId, bool isAuthenticated = false, UserRole? userRole = null);
    Task<LikeStatusDto> GetLikeStatusAsync(long userId, long documentId, bool isAuthenticated = false, UserRole? userRole = null);
    Task<LikeStatusDto> ToggleLikeAsync(long userId, long documentId, UserRole? userRole = null);
    Task<List<long>> GetLikedDocumentIdsAsync(long userId, IEnumerable<long> documentIds);
    Task ClearCacheByDocumentIdAsync(long documentId);
}
