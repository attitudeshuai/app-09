using KnowledgeBase.Application.DTOs.Badges;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;

namespace KnowledgeBase.Application.Services;

public class BadgeService : IBadgeService
{
    private readonly IUnitOfWork _unitOfWork;

    public BadgeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<BadgeDto>> GetAllAsync()
    {
        var badges = await _unitOfWork.Badges.GetAllAsync();
        return badges.Select(MapToDto).ToList();
    }

    public async Task<List<BadgeDto>> GetAllActiveAsync()
    {
        var badges = await _unitOfWork.Badges.GetAllActiveAsync();
        return badges.Select(MapToDto).ToList();
    }

    public async Task<BadgeDto?> GetByIdAsync(long id)
    {
        var badge = await _unitOfWork.Badges.GetByIdAsync(id);
        return badge != null ? MapToDto(badge) : null;
    }

    public async Task<List<UserBadgeDto>> GetUserBadgesAsync(long userId)
    {
        var userBadges = await _unitOfWork.UserBadges.GetByUserIdAsync(userId);
        return userBadges.Select(ub => new UserBadgeDto
        {
            BadgeId = ub.BadgeId,
            Name = ub.Badge?.Name ?? string.Empty,
            Icon = ub.Badge?.Icon ?? string.Empty,
            Description = ub.Badge?.Description,
            Type = ub.Badge?.Type ?? BadgeType.Points,
            EarnedAt = ub.EarnedAt
        }).ToList();
    }

    public async Task<BadgeDto> CreateAsync(CreateBadgeRequest request, long currentUserId)
    {
        var badge = new Badge
        {
            Name = request.Name,
            Icon = request.Icon,
            Description = request.Description,
            Type = request.Type,
            RequiredValue = request.RequiredValue,
            IsActive = request.IsActive,
            CreatedBy = currentUserId
        };

        await _unitOfWork.Badges.AddAsync(badge);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(badge);
    }

    public async Task UpdateAsync(long id, UpdateBadgeRequest request, long currentUserId)
    {
        var badge = await _unitOfWork.Badges.GetByIdAsync(id);
        if (badge == null)
        {
            throw new KeyNotFoundException("勋章不存在");
        }

        if (request.Name != null) badge.Name = request.Name;
        if (request.Icon != null) badge.Icon = request.Icon;
        if (request.Description != null) badge.Description = request.Description;
        if (request.Type.HasValue) badge.Type = request.Type.Value;
        if (request.RequiredValue.HasValue) badge.RequiredValue = request.RequiredValue.Value;
        if (request.IsActive.HasValue) badge.IsActive = request.IsActive.Value;

        badge.UpdatedBy = currentUserId;
        await _unitOfWork.Badges.UpdateAsync(badge);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        if (!await _unitOfWork.Badges.ExistsAsync(id))
        {
            throw new KeyNotFoundException("勋章不存在");
        }

        await _unitOfWork.Badges.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CheckAndAwardBadgesAsync(long userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("用户不存在");
        }

        var allBadges = await _unitOfWork.Badges.GetAllActiveAsync();
        var userBadges = await _unitOfWork.UserBadges.GetUserBadgesAsync(userId);
        var userBadgeIds = userBadges.Select(b => b.Id).ToHashSet();

        var documentCount = await _unitOfWork.Documents.GetDocumentCountByUserIdAsync(userId, DocumentStatus.Published);
        var likeCount = await GetUserTotalLikesAsync(userId);
        var commentCount = await GetUserTotalCommentsAsync(userId);
        var followerCount = await _unitOfWork.UserFollows.GetFollowerCountAsync(userId);

        foreach (var badge in allBadges)
        {
            if (userBadgeIds.Contains(badge.Id))
            {
                continue;
            }

            var shouldAward = badge.Type switch
            {
                BadgeType.Points => user.TotalPoints >= badge.RequiredValue,
                BadgeType.Documents => documentCount >= badge.RequiredValue,
                BadgeType.Likes => likeCount >= badge.RequiredValue,
                BadgeType.Comments => commentCount >= badge.RequiredValue,
                BadgeType.Followers => followerCount >= badge.RequiredValue,
                _ => false
            };

            if (shouldAward)
            {
                var userBadge = new UserBadge
                {
                    UserId = userId,
                    BadgeId = badge.Id
                };
                await _unitOfWork.UserBadges.AddAsync(userBadge);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<int> GetUserTotalLikesAsync(long userId)
    {
        var documents = (await _unitOfWork.Documents.GetByUserIdAsync(userId)).ToList();
        var totalLikes = 0;
        foreach (var doc in documents)
        {
            totalLikes += doc.LikeCount;
        }
        return totalLikes;
    }

    private async Task<int> GetUserTotalCommentsAsync(long userId)
    {
        var documents = (await _unitOfWork.Documents.GetByUserIdAsync(userId)).ToList();
        var totalComments = 0;
        foreach (var doc in documents)
        {
            totalComments += await _unitOfWork.DocumentComments.CountByDocumentIdAsync(doc.Id);
        }
        return totalComments;
    }

    private static BadgeDto MapToDto(Badge badge)
    {
        return new BadgeDto
        {
            Id = badge.Id,
            Name = badge.Name,
            Icon = badge.Icon,
            Description = badge.Description,
            Type = badge.Type,
            RequiredValue = badge.RequiredValue,
            IsActive = badge.IsActive
        };
    }
}
