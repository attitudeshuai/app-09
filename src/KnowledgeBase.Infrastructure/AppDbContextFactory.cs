using KnowledgeBase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KnowledgeBase.Infrastructure;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseMySql("Server=localhost;Database=knowledge_base;User=root;Password=root;",
            ServerVersion.Parse("8.0.0"));

        return new AppDbContext(optionsBuilder.Options);
    }
}
