using KnowledgeBase.Application.DTOs.Levels;

namespace KnowledgeBase.Application.Interfaces;

public interface ILevelService
{
    Task<List<LevelDto>> GetAllAsync();
    Task<LevelDto?> GetByIdAsync(long id);
    Task<LevelDto?> GetByLevelNumberAsync(int levelNumber);
    Task<LevelDto> CreateAsync(CreateLevelRequest request, long currentUserId);
    Task UpdateAsync(long id, UpdateLevelRequest request, long currentUserId);
    Task DeleteAsync(long id);
}
