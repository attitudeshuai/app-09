using KnowledgeBase.Application.DTOs.Likes;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;

namespace KnowledgeBase.Application.Services;

public class LikeService : ILikeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly IPointService _pointService;
    private readonly IBadgeService _badgeService;
    private const string LikeCountCachePrefix = "likes:count:";
    private const string UserLikeCachePrefix = "likes:user:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
    private const int CacheRemoveMaxRetries = 3;
    private static readonly TimeSpan CacheRemoveRetryDelay = TimeSpan.FromMilliseconds(100);

    public LikeService(IUnitOfWork unitOfWork, ICacheService cacheService, IPointService pointService, IBadgeService badgeService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _pointService = pointService;
        _badgeService = badgeService;
    }

    public async Task<bool> IsLikedAsync(long userId, long documentId)
    {
        if (userId <= 0)
        {
            return false;
        }

        var cacheKey = GetUserLikeCacheKey(userId, documentId);
        var cached = await _cacheService.GetAsync<bool?>(cacheKey);
        if (cached.HasValue)
        {
            return cached.Value;
        }

        var isLiked = await _unitOfWork.DocumentLikes.IsLikedAsync(userId, documentId);
        await _cacheService.SetAsync(cacheKey, isLiked, CacheDuration);
        return isLiked;
    }

    public async Task<int> GetLikeCountAsync(long documentId, bool isAuthenticated = false, UserRole? userRole = null)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
        if (document == null)
        {
            throw new KeyNotFoundException("文档不存在");
        }

        if (document.Status == DocumentStatus.Published)
        {
            var canView = await _unitOfWork.Documents.CanViewDocumentAsync(documentId, isAuthenticated, userRole);
            if (!canView)
            {
                throw new UnauthorizedAccessException("无权查看该文档的点赞数");
            }
        }

        var cacheKey = GetLikeCountCacheKey(documentId);
        var cached = await _cacheService.GetAsync<int?>(cacheKey);
        if (cached.HasValue)
        {
            return cached.Value;
        }

        var count = await _unitOfWork.DocumentLikes.GetLikeCountAsync(documentId);
        await _cacheService.SetAsync(cacheKey, count, CacheDuration);
        return count;
    }

    public async Task<LikeStatusDto> GetLikeStatusAsync(long userId, long documentId, bool isAuthenticated = false, UserRole? userRole = null)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
        if (document == null)
        {
            throw new KeyNotFoundException("文档不存在");
        }

        if (document.Status == DocumentStatus.Published)
        {
            var canView = await _unitOfWork.Documents.CanViewDocumentAsync(documentId, isAuthenticated, userRole);
            if (!canView)
            {
                throw new UnauthorizedAccessException("无权查看该文档的点赞状态");
            }
        }

        var isLiked = await IsLikedAsync(userId, documentId);
        var likeCount = await GetLikeCountAsync(documentId, isAuthenticated, userRole);

        return new LikeStatusDto
        {
            DocumentId = documentId,
            IsLiked = isLiked,
            LikeCount = likeCount
        };
    }

    public async Task<LikeStatusDto> ToggleLikeAsync(long userId, long documentId, UserRole? userRole = null)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
        if (document == null)
        {
            throw new KeyNotFoundException("文档不存在");
        }

        if (document.Status != DocumentStatus.Published)
        {
            throw new InvalidOperationException("只能对已发布的文档点赞");
        }

        var canView = await _unitOfWork.Documents.CanViewDocumentAsync(documentId, true, userRole);
        if (!canView)
        {
            throw new UnauthorizedAccessException("无权对该文档点赞");
        }

        var isLiked = await _unitOfWork.DocumentLikes.IsLikedAsync(userId, documentId);

        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (isLiked)
            {
                await _unitOfWork.DocumentLikes.DeleteAsync(userId, documentId);
                document.LikeCount = Math.Max(0, document.LikeCount - 1);
            }
            else
            {
                var like = new DocumentLike
                {
                    UserId = userId,
                    DocumentId = documentId
                };
                await _unitOfWork.DocumentLikes.AddAsync(like);
                document.LikeCount++;
            }

            await _unitOfWork.Documents.UpdateAsync(document);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            var newIsLiked = !isLiked;
            await UpdateCacheAsync(userId, documentId, newIsLiked, document.LikeCount);

            if (document.CreatedBy != userId)
            {
                if (newIsLiked)
                {
                    await _pointService.AwardForLikedAsync(document.CreatedBy, documentId, userId);
                }
                else
                {
                    await _pointService.RevokeForLikedAsync(document.CreatedBy, documentId, userId);
                }
                await _badgeService.CheckAndAwardBadgesAsync(document.CreatedBy);
            }

            return new LikeStatusDto
            {
                DocumentId = documentId,
                IsLiked = newIsLiked,
                LikeCount = document.LikeCount
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<long>> GetLikedDocumentIdsAsync(long userId, IEnumerable<long> documentIds)
    {
        if (userId <= 0 || !documentIds.Any())
        {
            return new List<long>();
        }

        var likedIds = new List<long>();
        var idsToCheck = new List<long>();

        foreach (var docId in documentIds)
        {
            var cacheKey = GetUserLikeCacheKey(userId, docId);
            var cached = await _cacheService.GetAsync<bool?>(cacheKey);
            if (cached.HasValue)
            {
                if (cached.Value)
                {
                    likedIds.Add(docId);
                }
            }
            else
            {
                idsToCheck.Add(docId);
            }
        }

        if (idsToCheck.Any())
        {
            var dbLikedIds = await _unitOfWork.DocumentLikes.GetLikedDocumentIdsAsync(userId, idsToCheck);
            likedIds.AddRange(dbLikedIds);

            foreach (var docId in idsToCheck)
            {
                var cacheKey = GetUserLikeCacheKey(userId, docId);
                var isLiked = dbLikedIds.Contains(docId);
                await _cacheService.SetAsync(cacheKey, isLiked, CacheDuration);
            }
        }

        return likedIds;
    }

    public async Task ClearCacheByDocumentIdAsync(long documentId)
    {
        var countCacheKey = GetLikeCountCacheKey(documentId);
        await RemoveCacheWithRetryAsync(countCacheKey);
    }

    private async Task UpdateCacheAsync(long userId, long documentId, bool isLiked, int likeCount)
    {
        var countCacheKey = GetLikeCountCacheKey(documentId);
        var userCacheKey = GetUserLikeCacheKey(userId, documentId);

        await _cacheService.SetAsync(countCacheKey, likeCount, CacheDuration);
        await _cacheService.SetAsync(userCacheKey, isLiked, CacheDuration);
    }

    private async Task RemoveCacheWithRetryAsync(string key)
    {
        for (int i = 0; i < CacheRemoveMaxRetries; i++)
        {
            try
            {
                await _cacheService.RemoveAsync(key);
                return;
            }
            catch
            {
                if (i == CacheRemoveMaxRetries - 1)
                {
                    throw;
                }
                await Task.Delay(CacheRemoveRetryDelay);
            }
        }
    }

    private static string GetLikeCountCacheKey(long documentId)
    {
        return $"{LikeCountCachePrefix}{documentId}";
    }

    private static string GetUserLikeCacheKey(long userId, long documentId)
    {
        return $"{UserLikeCachePrefix}{userId}:{documentId}";
    }
}
