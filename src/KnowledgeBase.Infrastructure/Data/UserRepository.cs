using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Infrastructure.Data;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
    }

    public async Task<User> AddAsync(User entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.Users.AddAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(User entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.Users.FindAsync(id);
        if (entity != null)
        {
            _context.Users.Remove(entity);
        }
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Users.AnyAsync(u => u.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetPagedAsync(int pageNumber, int pageSize, string? keyword)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(u => u.Username.Contains(keyword)
                                  || u.Email.Contains(keyword)
                                  || u.Nickname != null && u.Nickname.Contains(keyword));
        }

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string? keyword)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(u => u.Username.Contains(keyword)
                                  || u.Email.Contains(keyword)
                                  || u.Nickname != null && u.Nickname.Contains(keyword));
        }

        return await query.CountAsync();
    }
}
