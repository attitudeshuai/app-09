using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Infrastructure.Data;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(long id)
    {
        return await _context.Categories
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .ToListAsync();
    }

    public async Task<Category> AddAsync(Category entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.Categories.AddAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(Category entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Categories.Update(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.Categories.FindAsync(id);
        if (entity != null)
        {
            _context.Categories.Remove(entity);
        }
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Category>> GetTreeAsync()
    {
        var allCategories = await _context.Categories
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .ToListAsync();

        var rootCategories = allCategories
            .Where(c => c.ParentId == null)
            .ToList();

        foreach (var category in allCategories)
        {
            category.Children = allCategories
                .Where(c => c.ParentId == category.Id)
                .ToList();
        }

        return rootCategories;
    }

    public async Task<IEnumerable<Category>> GetByParentIdAsync(long? parentId)
    {
        return await _context.Categories
            .Where(c => c.ParentId == parentId)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetAllWithChildrenAsync()
    {
        return await _context.Categories
            .Include(c => c.Children)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .ToListAsync();
    }

    public async Task<bool> HasChildrenAsync(long id)
    {
        return await _context.Categories.AnyAsync(c => c.ParentId == id);
    }

    public async Task<bool> IsDescendantAsync(long categoryId, long ancestorId)
    {
        var currentId = categoryId;
        var visited = new HashSet<long>();

        while (currentId != 0)
        {
            if (visited.Contains(currentId))
            {
                return false;
            }
            visited.Add(currentId);

            var category = await _context.Categories
                .Where(c => c.Id == currentId)
                .Select(c => new { c.ParentId })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return false;
            }

            if (category.ParentId == ancestorId)
            {
                return true;
            }

            if (!category.ParentId.HasValue)
            {
                return false;
            }

            currentId = category.ParentId.Value;
        }

        return false;
    }
}
