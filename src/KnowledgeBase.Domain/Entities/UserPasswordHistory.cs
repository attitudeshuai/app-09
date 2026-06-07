namespace KnowledgeBase.Domain.Entities;

public class UserPasswordHistory
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
