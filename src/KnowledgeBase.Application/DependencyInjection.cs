using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Application.Services;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDocumentVersionService, DocumentVersionService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<ILikeService, LikeService>();
        services.AddScoped<IFollowService, FollowService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IViewHistoryService, ViewHistoryService>();
        services.AddScoped<IOperationLogService, OperationLogService>();
        services.AddScoped<IPasswordValidator, PasswordValidator>();

        return services;
    }
}
