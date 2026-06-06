namespace KnowledgeBase.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public int SortOrder { get; set; }
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
