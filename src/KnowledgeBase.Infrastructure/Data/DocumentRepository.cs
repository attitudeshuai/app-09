using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Infrastructure.Data;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> GetByIdAsync(long id)
    {
        return await _context.Documents
            .Include(d => d.Category)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<Document>> GetAllAsync()
    {
        return await _context.Documents
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .ToListAsync();
    }

    public async Task<Document> AddAsync(Document entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.Version = 1;
        await _context.Documents.AddAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(Document entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Documents.Update(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.Documents.FindAsync(id);
        if (entity != null)
        {
            _context.Documents.Remove(entity);
        }
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.Documents.AnyAsync(d => d.Id == id);
    }

    public async Task<(IEnumerable<Document> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? keyword,
        long? categoryId,
        DocumentStatus? status,
        string? tag,
        bool applyVisibilityFilter = false,
        bool isAuthenticated = false,
        UserRole? userRole = null)
    {
        var query = _context.Documents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(d => d.Title.Contains(keyword)
                                  || d.Content.Contains(keyword)
                                  || d.Tags != null && d.Tags.Contains(keyword));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(d => d.CategoryId == categoryId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        if (applyVisibilityFilter)
        {
            query = ApplyVisibilityFilter(query, isAuthenticated, userRole);
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var allDocuments = await query
                .Include(d => d.Category)
                .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
                .ToListAsync();

            var filtered = allDocuments.Where(d => HasExactTag(d.Tags, tag)).ToList();
            var totalCount = filtered.Count;
            var items = filtered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (items, totalCount);
        }
        else
        {
            var totalCount = await query.CountAsync();
            var items = await query
                .Include(d => d.Category)
                .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }

    public async Task<IEnumerable<Document>> SearchAsync(string keyword, int pageNumber, int pageSize,
        bool isAuthenticated = false,
        UserRole? userRole = null)
    {
        var query = _context.Documents
            .Where(d => d.Status == DocumentStatus.Published &&
                       (d.Title.Contains(keyword) ||
                        d.Content.Contains(keyword) ||
                        (d.Tags != null && d.Tags.Contains(keyword))));

        query = ApplyVisibilityFilter(query, isAuthenticated, userRole);

        return await query
            .Include(d => d.Category)
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> SearchCountAsync(string keyword,
        bool isAuthenticated = false,
        UserRole? userRole = null)
    {
        var query = _context.Documents
            .Where(d => d.Status == DocumentStatus.Published &&
                       (d.Title.Contains(keyword) ||
                        d.Content.Contains(keyword) ||
                        (d.Tags != null && d.Tags.Contains(keyword))));

        query = ApplyVisibilityFilter(query, isAuthenticated, userRole);

        return await query.CountAsync();
    }

    public async Task IncrementViewCountAsync(long id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document != null)
        {
            document.ViewCount++;
            document.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<IEnumerable<Document>> GetByCategoryIdAsync(long categoryId)
    {
        return await _context.Documents
            .Where(d => d.CategoryId == categoryId)
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> UpdateCategoryIdAsync(long sourceCategoryId, long targetCategoryId, long updatedBy)
    {
        var now = DateTime.UtcNow;
        return await _context.Documents
            .Where(d => d.CategoryId == sourceCategoryId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(d => d.CategoryId, targetCategoryId)
                .SetProperty(d => d.UpdatedAt, now)
                .SetProperty(d => d.UpdatedBy, updatedBy));
    }

    public async Task<IEnumerable<Document>> GetScheduledDocumentsToPublishAsync(DateTime now)
    {
        return await _context.Documents
            .Where(d => d.Status == DocumentStatus.Scheduled
                     && d.PublishTime.HasValue
                     && d.PublishTime.Value <= now)
            .ToListAsync();
    }

    public async Task<int> GetDocumentCountByUserIdAsync(long userId, DocumentStatus? status = null)
    {
        var query = _context.Documents.Where(d => d.CreatedBy == userId);

        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        return await query.CountAsync();
    }

    public async Task<IEnumerable<Document>> GetByUserIdAsync(long userId, DocumentStatus? status = null)
    {
        var query = _context.Documents.Where(d => d.CreatedBy == userId);

        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        return await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> CanViewDocumentAsync(long documentId, bool isAuthenticated, UserRole? userRole)
    {
        var query = _context.Documents.Where(d => d.Id == documentId && d.Status == DocumentStatus.Published);
        query = ApplyVisibilityFilter(query, isAuthenticated, userRole);
        return await query.AnyAsync();
    }

    public async Task<Dictionary<string, int>> GetAllTagsWithCountAsync(
        bool applyVisibilityFilter = false,
        bool isAuthenticated = false,
        UserRole? userRole = null)
    {
        var query = _context.Documents
            .Where(d => d.Tags != null && d.Tags != string.Empty);

        if (applyVisibilityFilter)
        {
            query = ApplyVisibilityFilter(query, isAuthenticated, userRole);
        }

        var tagsList = await query.Select(d => d.Tags).ToListAsync();
        var tagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var tags in tagsList)
        {
            if (string.IsNullOrWhiteSpace(tags)) continue;

            var tagArray = tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var tag in tagArray)
            {
                var trimmedTag = tag.Trim();
                if (string.IsNullOrWhiteSpace(trimmedTag)) continue;

                if (tagCounts.ContainsKey(trimmedTag))
                {
                    tagCounts[trimmedTag]++;
                }
                else
                {
                    tagCounts[trimmedTag] = 1;
                }
            }
        }

        return tagCounts;
    }

    public async Task<IEnumerable<string>> SearchTagsAsync(string keyword, int limit = 10,
        bool applyVisibilityFilter = false,
        bool isAuthenticated = false,
        UserRole? userRole = null)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return Enumerable.Empty<string>();
        }

        var allTags = await GetAllTagsWithCountAsync(applyVisibilityFilter, isAuthenticated, userRole);

        return allTags
            .Where(kvp => kvp.Key.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key)
            .Take(limit)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    private static bool HasExactTag(string? tags, string tag)
    {
        if (string.IsNullOrWhiteSpace(tags) || string.IsNullOrWhiteSpace(tag))
        {
            return false;
        }

        var tagArray = tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return tagArray.Any(t => t.Trim().Equals(tag.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static IQueryable<Document> ApplyVisibilityFilter(IQueryable<Document> query, bool isAuthenticated, UserRole? userRole)
    {
        if (!isAuthenticated)
        {
            query = query.Where(d => d.Visibility == DocumentVisibility.Public);
        }
        else if (userRole.HasValue)
        {
            var roleName = userRole.Value.ToString();
            query = query.Where(d =>
                d.Visibility == DocumentVisibility.Public ||
                d.Visibility == DocumentVisibility.AuthenticatedUsers ||
                (d.Visibility == DocumentVisibility.RoleSpecific &&
                 d.AllowedRoles != null &&
                 d.AllowedRoles.Contains(roleName)));
        }
        else
        {
            query = query.Where(d =>
                d.Visibility == DocumentVisibility.Public ||
                d.Visibility == DocumentVisibility.AuthenticatedUsers);
        }

        return query;
    }
}
