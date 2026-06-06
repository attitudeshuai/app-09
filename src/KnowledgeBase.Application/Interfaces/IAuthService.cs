using KnowledgeBase.Application.DTOs.Auth;
using KnowledgeBase.Application.DTOs.Users;

namespace KnowledgeBase.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<UserDto> GetCurrentUserAsync(long userId);
}
