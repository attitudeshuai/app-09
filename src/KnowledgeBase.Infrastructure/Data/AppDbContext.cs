using KnowledgeBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentVersion> DocumentVersions { get; set; }
    public DbSet<DocumentFavorite> DocumentFavorites { get; set; }
    public DbSet<DocumentComment> DocumentComments { get; set; }
    public DbSet<DocumentViewHistory> DocumentViewHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Username).HasMaxLength(50).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(100).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(u => u.Nickname).HasMaxLength(50);
            entity.Property(u => u.Avatar).HasMaxLength(500);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Name);
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Description).HasMaxLength(500);
            entity.HasOne(c => c.Parent)
                  .WithMany(c => c.Children)
                  .HasForeignKey(c => c.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(c => c.Documents)
                  .WithOne(d => d.Category)
                  .HasForeignKey(d => d.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasIndex(d => d.Title);
            entity.HasIndex(d => d.Status);
            entity.HasIndex(d => d.CategoryId);
            entity.Property(d => d.Title).HasMaxLength(200).IsRequired();
            entity.Property(d => d.Content).HasColumnType("longtext");
            entity.Property(d => d.Summary).HasMaxLength(500);
            entity.Property(d => d.Tags).HasMaxLength(500);
            entity.HasMany(d => d.Versions)
                  .WithOne(v => v.Document)
                  .HasForeignKey(v => v.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentVersion>(entity =>
        {
            entity.HasIndex(v => v.DocumentId);
            entity.HasIndex(v => new { v.DocumentId, v.VersionNumber }).IsUnique();
            entity.Property(v => v.Title).HasMaxLength(200).IsRequired();
            entity.Property(v => v.Content).HasColumnType("longtext");
            entity.Property(v => v.Summary).HasMaxLength(500);
            entity.Property(v => v.Tags).HasMaxLength(500);
            entity.Property(v => v.ChangeLog).HasMaxLength(500);
        });

        modelBuilder.Entity<DocumentFavorite>(entity =>
        {
            entity.HasKey(df => new { df.UserId, df.DocumentId });
            entity.HasIndex(df => df.UserId);
            entity.HasIndex(df => df.DocumentId);
            entity.HasOne(df => df.User)
                  .WithMany()
                  .HasForeignKey(df => df.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(df => df.Document)
                  .WithMany()
                  .HasForeignKey(df => df.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentComment>(entity =>
        {
            entity.HasIndex(c => c.DocumentId);
            entity.HasIndex(c => c.UserId);
            entity.HasIndex(c => c.CreatedAt);
            entity.Property(c => c.Content).HasColumnType("longtext").IsRequired();
            entity.HasOne(c => c.User)
                  .WithMany()
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(c => c.Document)
                  .WithMany()
                  .HasForeignKey(c => c.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentViewHistory>(entity =>
        {
            entity.HasKey(h => new { h.UserId, h.DocumentId });
            entity.HasIndex(h => h.UserId);
            entity.HasIndex(h => h.DocumentId);
            entity.HasIndex(h => h.ViewedAt);
            entity.HasOne(h => h.User)
                  .WithMany()
                  .HasForeignKey(h => h.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(h => h.Document)
                  .WithMany()
                  .HasForeignKey(h => h.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
