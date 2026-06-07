using KnowledgeBase.Application.DTOs.Points;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Application.Options;
using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace KnowledgeBase.Application.Services;

public class PointService : IPointService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PointOptions _options;

    public PointService(IUnitOfWork unitOfWork, IOptions<PointOptions> options)
    {
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    public async Task<UserPointSummaryDto> GetUserPointSummaryAsync(long userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("用户不存在");
        }

        var currentLevel = await _unitOfWork.Levels.GetLevelByPointsAsync(user.TotalPoints);
        var nextLevel = currentLevel != null
            ? await _unitOfWork.Levels.GetNextLevelAsync(currentLevel.LevelNumber)
            : null;

        var pointsToNextLevel = nextLevel != null
            ? nextLevel.RequiredPoints - user.TotalPoints
            : 0;

        return new UserPointSummaryDto
        {
            UserId = userId,
            TotalPoints = user.TotalPoints,
            LevelNumber = currentLevel?.LevelNumber ?? 0,
            LevelName = currentLevel?.Name,
            LevelIcon = currentLevel?.Icon,
            LevelColor = currentLevel?.Color,
            PointsToNextLevel = pointsToNextLevel,
            NextLevelRequiredPoints = nextLevel?.RequiredPoints ?? 0
        };
    }

    public async Task<List<PointRecordDto>> GetPointRecordsAsync(long userId, PointPagedRequest request)
    {
        var records = await _unitOfWork.PointRecords.GetByUserIdAsync(userId, request.PageNumber, request.PageSize);
        return records.Select(MapToDto).ToList();
    }

    public async Task<int> GetPointRecordCountAsync(long userId)
    {
        return await _unitOfWork.PointRecords.GetCountByUserIdAsync(userId);
    }

    public async Task AddPointsAsync(long userId, int points, PointSource source, string? description = null, long? referenceId = null, string? referenceType = null)
    {
        if (points <= 0)
        {
            return;
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("用户不存在");
        }

        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var record = new PointRecord
            {
                UserId = userId,
                Points = points,
                Source = source,
                Description = description,
                ReferenceId = referenceId,
                ReferenceType = referenceType,
                CreatedBy = userId
            };

            await _unitOfWork.PointRecords.AddAsync(record);

            user.TotalPoints += points;
            user.UpdatedBy = userId;
            await _unitOfWork.Users.UpdateAsync(user);

            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task CheckAndUpdateLevelAsync(long userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("用户不存在");
        }

        var level = await _unitOfWork.Levels.GetLevelByPointsAsync(user.TotalPoints);
        if (level != null && user.LevelId != level.Id)
        {
            user.LevelId = level.Id;
            user.UpdatedBy = userId;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task AwardForPublishDocumentAsync(long userId, long documentId)
    {
        var hasAwarded = await _unitOfWork.PointRecords.ExistsBySourceAndReferenceAsync(
            userId, PointSource.PublishDocument, documentId, "Document");

        if (hasAwarded)
        {
            return;
        }

        await AddPointsAsync(userId, _options.PublishDocumentPoints, PointSource.PublishDocument,
            "发布文档获得积分", documentId, "Document");
        await CheckAndUpdateLevelAsync(userId);
    }

    public async Task AwardForCommentAsync(long userId, long commentId, long documentId)
    {
        await AddPointsAsync(userId, _options.CommentPoints, PointSource.Comment,
            "发表评论获得积分", commentId, "Comment");
        await CheckAndUpdateLevelAsync(userId);
    }

    public async Task AwardForLikedAsync(long authorUserId, long documentId, long likedByUserId)
    {
        await AddPointsAsync(authorUserId, _options.LikedByOthersPoints, PointSource.LikedByOthers,
            "文档被点赞获得积分", documentId, "DocumentLike");
        await CheckAndUpdateLevelAsync(authorUserId);
    }

    public async Task RevokeForLikedAsync(long authorUserId, long documentId, long likedByUserId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(authorUserId);
        if (user == null)
        {
            return;
        }

        user.TotalPoints = Math.Max(0, user.TotalPoints - _options.LikedByOthersPoints);
        user.UpdatedBy = authorUserId;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        await CheckAndUpdateLevelAsync(authorUserId);
    }

    private static PointRecordDto MapToDto(PointRecord record)
    {
        return new PointRecordDto
        {
            Id = record.Id,
            UserId = record.UserId,
            Points = record.Points,
            Source = record.Source,
            SourceText = GetSourceText(record.Source),
            Description = record.Description,
            ReferenceId = record.ReferenceId,
            ReferenceType = record.ReferenceType,
            CreatedAt = record.CreatedAt
        };
    }

    private static string GetSourceText(PointSource source)
    {
        return source switch
        {
            PointSource.PublishDocument => "发布文档",
            PointSource.Comment => "发表评论",
            PointSource.LikedByOthers => "被点赞",
            PointSource.SystemReward => "系统奖励",
            _ => "未知来源"
        };
    }
}
