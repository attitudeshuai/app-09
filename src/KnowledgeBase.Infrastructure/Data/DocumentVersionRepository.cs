using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Infrastructure.Data;

public class DocumentVersionRepository : IDocumentVersionRepository
{
    private readonly AppDbContext _context;

    public DocumentVersionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentVersion?> GetByIdAsync(long id)
    {
        return await _context.DocumentVersions
            .Include(v => v.Document)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<IEnumerable<DocumentVersion>> GetAllAsync()
    {
        return await _context.DocumentVersions
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task<DocumentVersion> AddAsync(DocumentVersion entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.DocumentVersions.AddAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(DocumentVersion entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.DocumentVersions.Update(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.DocumentVersions.FindAsync(id);
        if (entity != null)
        {
            _context.DocumentVersions.Remove(entity);
        }
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.DocumentVersions.AnyAsync(v => v.Id == id);
    }

    public async Task<IEnumerable<DocumentVersion>> GetByDocumentIdAsync(long documentId)
    {
        return await _context.DocumentVersions
            .Where(v => v.DocumentId == documentId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();
    }

    public async Task<DocumentVersion?> GetByDocumentIdAndVersionAsync(long documentId, int versionNumber)
    {
        return await _context.DocumentVersions
            .FirstOrDefaultAsync(v => v.DocumentId == documentId && v.VersionNumber == versionNumber);
    }

    public async Task<int> GetLatestVersionNumberAsync(long documentId)
    {
        return await _context.DocumentVersions
            .Where(v => v.DocumentId == documentId)
            .MaxAsync(v => (int?)v.VersionNumber) ?? 0;
    }
}
