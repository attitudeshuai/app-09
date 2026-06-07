using KnowledgeBase.Application.DTOs.Likes;

namespace KnowledgeBase.Application.Interfaces;

public interface ILikeService
{
    Task<bool> IsLikedAsync(long userId, long documentId);
    Task<int> GetLikeCountAsync(long documentId);
    Task<LikeStatusDto> GetLikeStatusAsync(long userId, long documentId);
    Task<LikeStatusDto> ToggleLikeAsync(long userId, long documentId);
    Task<List<long>> GetLikedDocumentIdsAsync(long userId, IEnumerable<long> documentIds);
    Task ClearCacheByDocumentIdAsync(long documentId);
}
