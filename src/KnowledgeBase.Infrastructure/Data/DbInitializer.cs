using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task Initialize(AppDbContext context, IServiceProvider serviceProvider)
    {
        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>();

        if (!await context.Users.AnyAsync(u => u.Username == "admin"))
        {
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@knowledgebase.local",
                PasswordHash = passwordHasher.HashPassword("admin123"),
                Nickname = "系统管理员",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedBy = 1
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
            adminUser.CreatedBy = adminUser.Id;
            await context.SaveChangesAsync();
        }

        if (!await context.Categories.AnyAsync())
        {
            var rootCategory1 = new Category
            {
                Name = "技术文档",
                Description = "各类技术文档和教程",
                SortOrder = 1,
                CreatedBy = 1
            };

            var rootCategory2 = new Category
            {
                Name = "产品文档",
                Description = "产品介绍和使用手册",
                SortOrder = 2,
                CreatedBy = 1
            };

            var rootCategory3 = new Category
            {
                Name = "运营文档",
                Description = "运营相关文档和资料",
                SortOrder = 3,
                CreatedBy = 1
            };

            context.Categories.AddRange(rootCategory1, rootCategory2, rootCategory3);
            await context.SaveChangesAsync();

            var subCategory1 = new Category
            {
                Name = "后端开发",
                Description = "后端开发技术文档",
                ParentId = rootCategory1.Id,
                SortOrder = 1,
                CreatedBy = 1
            };

            var subCategory2 = new Category
            {
                Name = "前端开发",
                Description = "前端开发技术文档",
                ParentId = rootCategory1.Id,
                SortOrder = 2,
                CreatedBy = 1
            };

            var subCategory3 = new Category
            {
                Name = "数据库",
                Description = "数据库相关技术文档",
                ParentId = rootCategory1.Id,
                SortOrder = 3,
                CreatedBy = 1
            };

            context.Categories.AddRange(subCategory1, subCategory2, subCategory3);
            await context.SaveChangesAsync();

            var sampleDoc1 = new Document
            {
                Title = "知识库管理系统介绍",
                Content = "<h1>知识库管理系统</h1><p>这是一个基于 .NET 8 构建的知识库管理系统，支持多级分类、富文本文档管理、版本控制等功能。</p><h2>主要功能</h2><ul><li>用户管理</li><li>多级分类树</li><li>富文本文档</li><li>版本控制</li><li>全文搜索</li><li>数据统计</li></ul>",
                Summary = "知识库管理系统功能介绍",
                Tags = "知识库,介绍,功能",
                CategoryId = rootCategory2.Id,
                Status = DocumentStatus.Published,
                ViewCount = 0,
                Version = 1,
                CreatedBy = 1
            };

            context.Documents.Add(sampleDoc1);
            await context.SaveChangesAsync();

            var version1 = new DocumentVersion
            {
                DocumentId = sampleDoc1.Id,
                VersionNumber = 1,
                Title = sampleDoc1.Title,
                Content = sampleDoc1.Content,
                Summary = sampleDoc1.Summary,
                Tags = sampleDoc1.Tags,
                ChangeLog = "初始版本",
                CreatedBy = 1
            };

            context.DocumentVersions.Add(version1);
            await context.SaveChangesAsync();
        }
    }
}
