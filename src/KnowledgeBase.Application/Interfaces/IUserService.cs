using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.DTOs.Users;

namespace KnowledgeBase.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(long id);
    Task<PagedResult<UserDto>> GetPagedAsync(UserPagedRequest request);
    Task<UserDto> CreateAsync(CreateUserRequest request, long currentUserId);
    Task UpdateAsync(long id, UpdateUserRequest request, long currentUserId);
    Task DeleteAsync(long id);
    Task ChangePasswordAsync(long id, ChangePasswordRequest request);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<UserProfileDto> GetProfileAsync(long userId);
    Task UpdateProfileAsync(long userId, UpdateProfileRequest request);
}
