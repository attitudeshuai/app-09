using KnowledgeBase.Application.DTOs.Badges;

namespace KnowledgeBase.Application.Interfaces;

public interface IBadgeService
{
    Task<List<BadgeDto>> GetAllAsync();
    Task<List<BadgeDto>> GetAllActiveAsync();
    Task<BadgeDto?> GetByIdAsync(long id);
    Task<List<UserBadgeDto>> GetUserBadgesAsync(long userId);
    Task<BadgeDto> CreateAsync(CreateBadgeRequest request, long currentUserId);
    Task UpdateAsync(long id, UpdateBadgeRequest request, long currentUserId);
    Task DeleteAsync(long id);
    Task CheckAndAwardBadgesAsync(long userId);
}
