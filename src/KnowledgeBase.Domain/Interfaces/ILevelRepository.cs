using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Domain.Interfaces;

public interface ILevelRepository : IRepository<Level>
{
    Task<List<Level>> GetAllAsync();
    Task<Level?> GetByLevelNumberAsync(int levelNumber);
    Task<Level?> GetLevelByPointsAsync(int points);
    Task<Level?> GetNextLevelAsync(int currentLevelNumber);
}
