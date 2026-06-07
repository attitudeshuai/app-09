using KnowledgeBase.Application.DTOs.Documents;
using KnowledgeBase.Application.DTOs.DocumentVersions;
using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;

namespace KnowledgeBase.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILikeService _likeService;
    private readonly IPointService _pointService;
    private readonly IBadgeService _badgeService;

    public DocumentService(IUnitOfWork unitOfWork, ILikeService likeService, IPointService pointService, IBadgeService badgeService)
    {
        _unitOfWork = unitOfWork;
        _likeService = likeService;
        _pointService = pointService;
        _badgeService = badgeService;
    }

    public async Task<DocumentDto> GetByIdAsync(long id, long? userId = null, UserRole? userRole = null)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(id);
        if (document == null)
        {
            throw new KeyNotFoundException("文档不存在");
        }

        if (document.Status == DocumentStatus.Published)
        {
            var isAuthenticated = userId.HasValue && userId.Value > 0;
            var canView = await _unitOfWork.Documents.CanViewDocumentAsync(id, isAuthenticated, userRole);
            if (!canView)
            {
                throw new UnauthorizedAccessException("无权查看该文档");
            }
        }

        var isFavorited = userId.HasValue && userId.Value > 0
            ? await _unitOfWork.DocumentFavorites.IsFavoritedAsync(userId.Value, id)
            : false;

        var isLiked = userId.HasValue && userId.Value > 0
            ? await _unitOfWork.DocumentLikes.IsLikedAsync(userId.Value, id)
            : false;

        return MapToDto(document, isFavorited, isLiked);
    }

    public async Task<PagedResult<DocumentListDto>> GetPagedAsync(DocumentPagedRequest request, long? userId = null, UserRole? userRole = null, bool applyVisibilityFilter = false)
    {
        DocumentStatus? status = request.Status.HasValue ? (DocumentStatus?)request.Status.Value : null;
        var isAuthenticated = userId.HasValue && userId.Value > 0;
        var (items, totalCount) = await _unitOfWork.Documents.GetPagedAsync(
            request.PageNumber, request.PageSize, request.Keyword, request.CategoryId, status, request.Tag,
            applyVisibilityFilter, isAuthenticated, userRole);

        var favoritedIds = userId.HasValue && userId.Value > 0
            ? await _unitOfWork.DocumentFavorites.GetFavoritedDocumentIdsAsync(userId.Value, items.Select(d => d.Id))
            : new List<long>();

        var likedIds = userId.HasValue && userId.Value > 0
            ? await _unitOfWork.DocumentLikes.GetLikedDocumentIdsAsync(userId.Value, items.Select(d => d.Id))
            : new List<long>();

        return new PagedResult<DocumentListDto>
        {
            Items = items.Select(d => MapToListDto(d, favoritedIds.Contains(d.Id), likedIds.Contains(d.Id))),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<PagedResult<DocumentListDto>> SearchAsync(string keyword, int pageNumber, int pageSize, long? userId = null, UserRole? userRole = null)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new PagedResult<DocumentListDto>
            {
                Items = new List<DocumentListDto>(),
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        var isAuthenticated = userId.HasValue && userId.Value > 0;
        var items = await _unitOfWork.Documents.SearchAsync(keyword, pageNumber, pageSize, isAuthenticated, userRole);
        var totalCount = await _unitOfWork.Documents.SearchCountAsync(keyword, isAuthenticated, userRole);

        var favoritedIds = userId.HasValue && userId.Value > 0
            ? await _unitOfWork.DocumentFavorites.GetFavoritedDocumentIdsAsync(userId.Value, items.Select(d => d.Id))
            : new List<long>();

        var likedIds = userId.HasValue && userId.Value > 0
            ? await _unitOfWork.DocumentLikes.GetLikedDocumentIdsAsync(userId.Value, items.Select(d => d.Id))
            : new List<long>();

        return new PagedResult<DocumentListDto>
        {
            Items = items.Select(d => MapToListDto(d, favoritedIds.Contains(d.Id), likedIds.Contains(d.Id))),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<DocumentDto> CreateAsync(CreateDocumentRequest request, long currentUserId)
    {
        if (!await _unitOfWork.Categories.ExistsAsync(request.CategoryId))
        {
            throw new KeyNotFoundException("分类不存在");
        }

        var status = (DocumentStatus)request.Status;
        if (request.PublishTime.HasValue && request.PublishTime.Value > DateTime.UtcNow)
        {
            status = DocumentStatus.Scheduled;
        }

        var document = new Document
        {
            Title = request.Title,
            Content = request.Content,
            Summary = request.Summary,
            Tags = request.Tags,
            CategoryId = request.CategoryId,
            Status = status,
            Visibility = (DocumentVisibility)request.Visibility,
            AllowedRoles = request.AllowedRoles,
            PublishTime = request.PublishTime,
            ScheduledBy = status == DocumentStatus.Scheduled ? currentUserId : null,
            ViewCount = 0,
            Version = 1,
            CreatedBy = currentUserId
        };

        await _unitOfWork.Documents.AddAsync(document);
        await _unitOfWork.SaveChangesAsync();

        var version = new DocumentVersion
        {
            DocumentId = document.Id,
            VersionNumber = 1,
            Title = document.Title,
            Content = document.Content,
            Summary = document.Summary,
            Tags = document.Tags,
            ChangeLog = "初始版本",
            CreatedBy = currentUserId
        };

        await _unitOfWork.DocumentVersions.AddAsync(version);
        await _unitOfWork.SaveChangesAsync();

        if (document.Status == DocumentStatus.Published)
        {
            await _pointService.AwardForPublishDocumentAsync(currentUserId, document.Id);
            await _badgeService.CheckAndAwardBadgesAsync(currentUserId);
        }

        return MapToDto(document);
    }

    public async Task<DocumentDto> UpdateAsync(long id, UpdateDocumentRequest request, long currentUserId)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(id);
        if (document == null)
        {
            throw new KeyNotFoundException("文档不存在");
        }

        var originalStatus = document.Status;

        if (!await _unitOfWork.Categories.ExistsAsync(request.CategoryId))
        {
            throw new KeyNotFoundException("分类不存在");
        }

        var oldVersion = new DocumentVersion
        {
            DocumentId = document.Id,
            VersionNumber = document.Version,
            Title = document.Title,
            Content = document.Content,
            Summary = document.Summary,
            Tags = document.Tags,
            ChangeLog = request.ChangeLog ?? "更新文档",
            CreatedBy = currentUserId
        };

        await _unitOfWork.DocumentVersions.AddAsync(oldVersion);

        document.Title = request.Title;
        document.Content = request.Content;
        document.Summary = request.Summary;
        document.Tags = request.Tags;
        document.CategoryId = request.CategoryId;

        if (request.PublishTime.HasValue)
        {
            document.PublishTime = request.PublishTime.Value;
            if (request.PublishTime.Value > DateTime.UtcNow)
            {
                document.Status = DocumentStatus.Scheduled;
                document.ScheduledBy = currentUserId;
                document.IsAutoPublished = false;
            }
            else
            {
                document.Status = DocumentStatus.Published;
                document.IsAutoPublished = false;
            }
        }
        else if (request.Status.HasValue)
        {
            document.Status = (DocumentStatus)request.Status.Value;
            if (document.Status != DocumentStatus.Scheduled)
            {
                document.PublishTime = null;
                document.ScheduledBy = null;
            }
        }

        if (request.Visibility.HasValue)
        {
            document.Visibility = (DocumentVisibility)request.Visibility.Value;
            document.AllowedRoles = request.AllowedRoles;
        }

        document.Version++;
        document.UpdatedBy = currentUserId;

        await _unitOfWork.Documents.UpdateAsync(document);
        await _unitOfWork.SaveChangesAsync();

        if (originalStatus != DocumentStatus.Published && document.Status == DocumentStatus.Published)
        {
            await _pointService.AwardForPublishDocumentAsync(currentUserId, document.Id);
            await _badgeService.CheckAndAwardBadgesAsync(currentUserId);
        }

        return MapToDto(document);
    }

    public async Task DeleteAsync(long id)
    {
        if (!await _unitOfWork.Documents.ExistsAsync(id))
        {
            throw new KeyNotFoundException("文档不存在");
        }

        await _unitOfWork.Documents.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
        await _likeService.ClearCacheByDocumentIdAsync(id);
    }

    public async Task IncrementViewCountAsync(long id)
    {
        await _unitOfWork.Documents.IncrementViewCountAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(long id, int status, long currentUserId)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(id);
        if (document == null)
        {
            throw new KeyNotFoundException("文档不存在");
        }

        var originalStatus = document.Status;

        document.Status = (DocumentStatus)status;
        if (document.Status != DocumentStatus.Scheduled)
        {
            document.PublishTime = null;
            document.ScheduledBy = null;
        }
        document.UpdatedBy = currentUserId;

        await _unitOfWork.Documents.UpdateAsync(document);
        await _unitOfWork.SaveChangesAsync();

        if (originalStatus != DocumentStatus.Published && document.Status == DocumentStatus.Published)
        {
            await _pointService.AwardForPublishDocumentAsync(document.CreatedBy, document.Id);
            await _badgeService.CheckAndAwardBadgesAsync(document.CreatedBy);
        }
    }

    public async Task UpdateVisibilityAsync(long id, UpdateVisibilityRequest request, long currentUserId)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(id);
        if (document == null)
        {
            throw new KeyNotFoundException("文档不存在");
        }

        document.Visibility = (DocumentVisibility)request.Visibility;
        document.AllowedRoles = request.AllowedRoles;
        document.UpdatedBy = currentUserId;

        await _unitOfWork.Documents.UpdateAsync(document);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> PublishScheduledDocumentsAsync()
    {
        var now = DateTime.UtcNow;
        var scheduledDocuments = await _unitOfWork.Documents.GetScheduledDocumentsToPublishAsync(now);

        var publishedCount = 0;
        foreach (var document in scheduledDocuments)
        {
            document.Status = DocumentStatus.Published;
            document.IsAutoPublished = true;
            document.UpdatedAt = now;
            publishedCount++;
        }

        if (publishedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync();

            foreach (var document in scheduledDocuments)
            {
                await _pointService.AwardForPublishDocumentAsync(document.CreatedBy, document.Id);
                await _badgeService.CheckAndAwardBadgesAsync(document.CreatedBy);
            }
        }

        return publishedCount;
    }

    public async Task<BatchOperationResult> BatchUpdateStatusAsync(List<long> ids, int status, long currentUserId)
    {
        var result = new BatchOperationResult();

        if (ids == null || ids.Count == 0)
        {
            return result;
        }

        var targetStatus = (DocumentStatus)status;

        foreach (var id in ids)
        {
            try
            {
                var document = await _unitOfWork.Documents.GetByIdAsync(id);
                if (document == null)
                {
                    result.Failures.Add(new BatchFailureItem { Id = id, Message = "文档不存在" });
                    result.FailureCount++;
                    continue;
                }

                document.Status = targetStatus;
                if (targetStatus != DocumentStatus.Scheduled)
                {
                    document.PublishTime = null;
                    document.ScheduledBy = null;
                }
                document.UpdatedBy = currentUserId;

                await _unitOfWork.Documents.UpdateAsync(document);
                result.SuccessIds.Add(id);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Failures.Add(new BatchFailureItem { Id = id, Message = ex.Message });
                result.FailureCount++;
            }
        }

        if (result.SuccessCount > 0)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return result;
    }

    public async Task<BatchOperationResult> BatchUpdateVisibilityAsync(List<long> ids, UpdateVisibilityRequest request, long currentUserId)
    {
        var result = new BatchOperationResult();

        if (ids == null || ids.Count == 0)
        {
            return result;
        }

        var targetVisibility = (DocumentVisibility)request.Visibility;

        foreach (var id in ids)
        {
            try
            {
                var document = await _unitOfWork.Documents.GetByIdAsync(id);
                if (document == null)
                {
                    result.Failures.Add(new BatchFailureItem { Id = id, Message = "文档不存在" });
                    result.FailureCount++;
                    continue;
                }

                document.Visibility = targetVisibility;
                document.AllowedRoles = request.AllowedRoles;
                document.UpdatedBy = currentUserId;

                await _unitOfWork.Documents.UpdateAsync(document);
                result.SuccessIds.Add(id);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Failures.Add(new BatchFailureItem { Id = id, Message = ex.Message });
                result.FailureCount++;
            }
        }

        if (result.SuccessCount > 0)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return result;
    }

    public async Task<BatchOperationResult> BatchMoveCategoryAsync(List<long> ids, long categoryId, long currentUserId)
    {
        var result = new BatchOperationResult();

        if (ids == null || ids.Count == 0)
        {
            return result;
        }

        if (!await _unitOfWork.Categories.ExistsAsync(categoryId))
        {
            foreach (var id in ids)
            {
                result.Failures.Add(new BatchFailureItem { Id = id, Message = "目标分类不存在" });
                result.FailureCount++;
            }
            return result;
        }

        foreach (var id in ids)
        {
            try
            {
                var document = await _unitOfWork.Documents.GetByIdAsync(id);
                if (document == null)
                {
                    result.Failures.Add(new BatchFailureItem { Id = id, Message = "文档不存在" });
                    result.FailureCount++;
                    continue;
                }

                document.CategoryId = categoryId;
                document.UpdatedBy = currentUserId;

                await _unitOfWork.Documents.UpdateAsync(document);
                result.SuccessIds.Add(id);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Failures.Add(new BatchFailureItem { Id = id, Message = ex.Message });
                result.FailureCount++;
            }
        }

        if (result.SuccessCount > 0)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return result;
    }

    public async Task<BatchOperationResult> BatchDeleteAsync(List<long> ids)
    {
        var result = new BatchOperationResult();

        if (ids == null || ids.Count == 0)
        {
            return result;
        }

        foreach (var id in ids)
        {
            try
            {
                if (!await _unitOfWork.Documents.ExistsAsync(id))
                {
                    result.Failures.Add(new BatchFailureItem { Id = id, Message = "文档不存在" });
                    result.FailureCount++;
                    continue;
                }

                await _unitOfWork.Documents.DeleteAsync(id);
                result.SuccessIds.Add(id);
                result.SuccessCount++;
                await _likeService.ClearCacheByDocumentIdAsync(id);
            }
            catch (Exception ex)
            {
                result.Failures.Add(new BatchFailureItem { Id = id, Message = ex.Message });
                result.FailureCount++;
            }
        }

        if (result.SuccessCount > 0)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return result;
    }

    private static DocumentDto MapToDto(Document document, bool isFavorited = false, bool isLiked = false)
    {
        return new DocumentDto
        {
            Id = document.Id,
            Title = document.Title,
            Content = document.Content,
            Summary = document.Summary,
            Tags = document.Tags,
            CategoryId = document.CategoryId,
            CategoryName = document.Category?.Name,
            Status = (int)document.Status,
            Visibility = (int)document.Visibility,
            AllowedRoles = document.AllowedRoles,
            ViewCount = document.ViewCount,
            LikeCount = document.LikeCount,
            Version = document.Version,
            PublishTime = document.PublishTime,
            IsAutoPublished = document.IsAutoPublished,
            ScheduledBy = document.ScheduledBy,
            CreatedBy = document.CreatedBy,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            IsFavorited = isFavorited,
            IsLiked = isLiked
        };
    }

    public async Task<IEnumerable<TagCloudDto>> GetTagCloudAsync(long? userId = null, UserRole? userRole = null, bool applyVisibilityFilter = false)
    {
        var isAuthenticated = userId.HasValue && userId.Value > 0;
        var tagCounts = await _unitOfWork.Documents.GetAllTagsWithCountAsync(
            applyVisibilityFilter, isAuthenticated, userRole);

        return tagCounts
            .OrderByDescending(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key)
            .Select(kvp => new TagCloudDto
            {
                Name = kvp.Key,
                Count = kvp.Value
            })
            .ToList();
    }

    public async Task<IEnumerable<string>> SearchTagsAsync(string keyword, int limit = 10, long? userId = null, UserRole? userRole = null, bool applyVisibilityFilter = false)
    {
        var isAuthenticated = userId.HasValue && userId.Value > 0;
        return await _unitOfWork.Documents.SearchTagsAsync(
            keyword, limit, applyVisibilityFilter, isAuthenticated, userRole);
    }

    private static DocumentListDto MapToListDto(Document document, bool isFavorited = false, bool isLiked = false)
    {
        return new DocumentListDto
        {
            Id = document.Id,
            Title = document.Title,
            Summary = document.Summary,
            Tags = document.Tags,
            CategoryId = document.CategoryId,
            CategoryName = document.Category?.Name,
            Status = (int)document.Status,
            Visibility = (int)document.Visibility,
            AllowedRoles = document.AllowedRoles,
            ViewCount = document.ViewCount,
            LikeCount = document.LikeCount,
            Version = document.Version,
            PublishTime = document.PublishTime,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            IsFavorited = isFavorited,
            IsLiked = isLiked
        };
    }
}
