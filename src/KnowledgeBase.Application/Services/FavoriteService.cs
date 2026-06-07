using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.DTOs.Favorites;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;

namespace KnowledgeBase.Application.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IUnitOfWork _unitOfWork;

    public FavoriteService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> IsFavoritedAsync(long userId, long documentId)
    {
        if (userId <= 0)
        {
            return false;
        }
        return await _unitOfWork.DocumentFavorites.IsFavoritedAsync(userId, documentId);
    }

    public async Task AddFavoriteAsync(long userId, long documentId)
    {
        if (!await _unitOfWork.Documents.ExistsAsync(documentId))
        {
            throw new KeyNotFoundException("文档不存在");
        }

        if (await _unitOfWork.DocumentFavorites.IsFavoritedAsync(userId, documentId))
        {
            return;
        }

        var favorite = new DocumentFavorite
        {
            UserId = userId,
            DocumentId = documentId
        };

        await _unitOfWork.DocumentFavorites.AddAsync(favorite);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveFavoriteAsync(long userId, long documentId)
    {
        if (!await _unitOfWork.DocumentFavorites.IsFavoritedAsync(userId, documentId))
        {
            return;
        }

        await _unitOfWork.DocumentFavorites.DeleteAsync(userId, documentId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PagedResult<FavoriteDocumentDto>> GetMyFavoritesAsync(long userId, FavoritePagedRequest request)
    {
        var (items, totalCount) = await _unitOfWork.DocumentFavorites.GetFavoritesByUserIdAsync(
            userId, request.PageNumber, request.PageSize);

        return new PagedResult<FavoriteDocumentDto>
        {
            Items = items.Select(MapToFavoriteDocumentDto),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    private static FavoriteDocumentDto MapToFavoriteDocumentDto(DocumentFavorite favorite)
    {
        return new FavoriteDocumentDto
        {
            Id = favorite.Document!.Id,
            Title = favorite.Document.Title,
            Summary = favorite.Document.Summary,
            Tags = favorite.Document.Tags,
            CategoryId = favorite.Document.CategoryId,
            CategoryName = favorite.Document.Category?.Name,
            Status = (int)favorite.Document.Status,
            ViewCount = favorite.Document.ViewCount,
            Version = favorite.Document.Version,
            CreatedAt = favorite.Document.CreatedAt,
            UpdatedAt = favorite.Document.UpdatedAt,
            FavoritedAt = favorite.CreatedAt
        };
    }
}
