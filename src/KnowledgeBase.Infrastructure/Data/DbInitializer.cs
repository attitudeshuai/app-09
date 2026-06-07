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

        if (!await context.Levels.AnyAsync())
        {
            var levels = new List<Level>
            {
                new Level { Name = "新手", LevelNumber = 1, RequiredPoints = 0, Icon = "🌱", Color = "#9CA3AF", Description = "刚刚加入的新用户", CreatedBy = 1 },
                new Level { Name = "学徒", LevelNumber = 2, RequiredPoints = 50, Icon = "🌿", Color = "#34D399", Description = "开始学习成长", CreatedBy = 1 },
                new Level { Name = "进阶者", LevelNumber = 3, RequiredPoints = 200, Icon = "🌳", Color = "#10B981", Description = "掌握了不少知识", CreatedBy = 1 },
                new Level { Name = "达人", LevelNumber = 4, RequiredPoints = 500, Icon = "⭐", Color = "#FBBF24", Description = "社区中的活跃用户", CreatedBy = 1 },
                new Level { Name = "专家", LevelNumber = 5, RequiredPoints = 1000, Icon = "🌟", Color = "#F59E0B", Description = "知识渊博的专家", CreatedBy = 1 },
                new Level { Name = "大师", LevelNumber = 6, RequiredPoints = 3000, Icon = "💎", Color = "#8B5CF6", Description = "社区的中流砥柱", CreatedBy = 1 },
                new Level { Name = "宗师", LevelNumber = 7, RequiredPoints = 8000, Icon = "👑", Color = "#EF4444", Description = "传说级别的存在", CreatedBy = 1 }
            };

            context.Levels.AddRange(levels);
            await context.SaveChangesAsync();
        }

        if (!await context.Badges.AnyAsync())
        {
            var badges = new List<Badge>
            {
                new Badge { Name = "初出茅庐", Icon = "📝", Description = "发布第一篇文档", Type = BadgeType.Documents, RequiredValue = 1, IsActive = true, CreatedBy = 1 },
                new Badge { Name = "勤笔达人", Icon = "✍️", Description = "发布10篇文档", Type = BadgeType.Documents, RequiredValue = 10, IsActive = true, CreatedBy = 1 },
                new Badge { Name = "著作等身", Icon = "📚", Description = "发布50篇文档", Type = BadgeType.Documents, RequiredValue = 50, IsActive = true, CreatedBy = 1 },
                new Badge { Name = "小试牛刀", Icon = "💬", Description = "发表第一条评论", Type = BadgeType.Comments, RequiredValue = 1, IsActive = true, CreatedBy = 1 },
                new Badge { Name = "评论达人", Icon = "🗣️", Description = "发表50条评论", Type = BadgeType.Comments, RequiredValue = 50, IsActive = true, CreatedBy = 1 },
                new Badge { Name = "人气王", Icon = "❤️", Description = "获得100个点赞", Type = BadgeType.Likes, RequiredValue = 100, IsActive = true, CreatedBy = 1 },
                new Badge { Name = "万人迷", Icon = "💖", Description = "获得500个点赞", Type = BadgeType.Likes, RequiredValue = 500, IsActive = true, CreatedBy = 1 },
                new Badge { Name = "积分新秀", Icon = "🥉", Description = "积分达到100", Type = BadgeType.Points, RequiredValue = 100, IsActive = true, CreatedBy = 1 },
                new Badge { Name = "积分达人", Icon = "🥈", Description = "积分达到500", Type = BadgeType.Points, RequiredValue = 500, IsActive = true, CreatedBy = 1 },
                new Badge { Name = "积分王者", Icon = "🥇", Description = "积分达到2000", Type = BadgeType.Points, RequiredValue = 2000, IsActive = true, CreatedBy = 1 },
                new Badge { Name = "粉丝初成", Icon = "👥", Description = "拥有10个粉丝", Type = BadgeType.Followers, RequiredValue = 10, IsActive = true, CreatedBy = 1 },
                new Badge { Name = "意见领袖", Icon = "🎤", Description = "拥有100个粉丝", Type = BadgeType.Followers, RequiredValue = 100, IsActive = true, CreatedBy = 1 }
            };

            context.Badges.AddRange(badges);
            await context.SaveChangesAsync();
        }
    }
}
