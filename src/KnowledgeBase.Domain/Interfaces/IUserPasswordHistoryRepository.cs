using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Domain.Interfaces;

public interface IUserPasswordHistoryRepository : IRepository<UserPasswordHistory>
{
    Task<IEnumerable<UserPasswordHistory>> GetRecentByUserIdAsync(long userId, int count);
    Task AddAsync(long userId, string passwordHash);
}
