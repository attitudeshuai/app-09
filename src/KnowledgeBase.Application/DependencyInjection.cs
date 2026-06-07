using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Application.Services;
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

        return services;
    }
}
