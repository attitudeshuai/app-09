using KnowledgeBase.Application.DTOs.Points;
using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Application.Interfaces;

public interface IPointService
{
    Task<UserPointSummaryDto> GetUserPointSummaryAsync(long userId);
    Task<List<PointRecordDto>> GetPointRecordsAsync(long userId, PointPagedRequest request);
    Task<int> GetPointRecordCountAsync(long userId);
    Task AddPointsAsync(long userId, int points, PointSource source, string? description = null, long? referenceId = null, string? referenceType = null);
    Task CheckAndUpdateLevelAsync(long userId);
    Task AwardForPublishDocumentAsync(long userId, long documentId);
    Task AwardForCommentAsync(long userId, long commentId, long documentId);
    Task AwardForLikedAsync(long authorUserId, long documentId, long likedByUserId);
    Task RevokeForLikedAsync(long authorUserId, long documentId, long likedByUserId);
}
