using KnowledgeBase.Domain.Interfaces;

namespace KnowledgeBase.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IUserRepository? _users;
    private ICategoryRepository? _categories;
    private IDocumentRepository? _documents;
    private IDocumentVersionRepository? _documentVersions;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public IDocumentRepository Documents => _documents ??= new DocumentRepository(_context);
    public IDocumentVersionRepository DocumentVersions => _documentVersions ??= new DocumentVersionRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
