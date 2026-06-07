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

    public DocumentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DocumentDto> GetByIdAsync(long id, long? userId = null)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(id);
        if (document == null)
        {
            throw new KeyNotFoundException("文档不存在");
        }

        var isFavorited = userId.HasValue && userId.Value > 0
            ? await _unitOfWork.DocumentFavorites.IsFavoritedAsync(userId.Value, id)
            : false;

        return MapToDto(document, isFavorited);
    }

    public async Task<PagedResult<DocumentListDto>> GetPagedAsync(DocumentPagedRequest request, long? userId = null)
    {
        DocumentStatus? status = request.Status.HasValue ? (DocumentStatus?)request.Status.Value : null;
        var (items, totalCount) = await _unitOfWork.Documents.GetPagedAsync(
            request.PageNumber, request.PageSize, request.Keyword, request.CategoryId, status);

        var favoritedIds = userId.HasValue && userId.Value > 0
            ? await _unitOfWork.DocumentFavorites.GetFavoritedDocumentIdsAsync(userId.Value, items.Select(d => d.Id))
            : new List<long>();

        return new PagedResult<DocumentListDto>
        {
            Items = items.Select(d => MapToListDto(d, favoritedIds.Contains(d.Id))),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<PagedResult<DocumentListDto>> SearchAsync(string keyword, int pageNumber, int pageSize, long? userId = null)
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

        var items = await _unitOfWork.Documents.SearchAsync(keyword, pageNumber, pageSize);
        var totalCount = await _unitOfWork.Documents.SearchCountAsync(keyword);

        var favoritedIds = userId.HasValue && userId.Value > 0
            ? await _unitOfWork.DocumentFavorites.GetFavoritedDocumentIdsAsync(userId.Value, items.Select(d => d.Id))
            : new List<long>();

        return new PagedResult<DocumentListDto>
        {
            Items = items.Select(d => MapToListDto(d, favoritedIds.Contains(d.Id))),
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

        var document = new Document
        {
            Title = request.Title,
            Content = request.Content,
            Summary = request.Summary,
            Tags = request.Tags,
            CategoryId = request.CategoryId,
            Status = (DocumentStatus)request.Status,
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

        return MapToDto(document);
    }

    public async Task<DocumentDto> UpdateAsync(long id, UpdateDocumentRequest request, long currentUserId)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(id);
        if (document == null)
        {
            throw new KeyNotFoundException("文档不存在");
        }

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
        if (request.Status.HasValue)
        {
            document.Status = (DocumentStatus)request.Status.Value;
        }
        document.Version++;
        document.UpdatedBy = currentUserId;

        await _unitOfWork.Documents.UpdateAsync(document);
        await _unitOfWork.SaveChangesAsync();

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

        document.Status = (DocumentStatus)status;
        document.UpdatedBy = currentUserId;

        await _unitOfWork.Documents.UpdateAsync(document);
        await _unitOfWork.SaveChangesAsync();
    }

    private static DocumentDto MapToDto(Document document, bool isFavorited = false)
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
            ViewCount = document.ViewCount,
            Version = document.Version,
            CreatedBy = document.CreatedBy,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            IsFavorited = isFavorited
        };
    }

    private static DocumentListDto MapToListDto(Document document, bool isFavorited = false)
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
            ViewCount = document.ViewCount,
            Version = document.Version,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            IsFavorited = isFavorited
        };
    }
}
