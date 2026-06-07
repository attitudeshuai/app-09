namespace KnowledgeBase.Application.Options;

public class PointOptions
{
    public const string SectionName = "Points";

    public int PublishDocumentPoints { get; set; } = 10;
    public int CommentPoints { get; set; } = 2;
    public int LikedByOthersPoints { get; set; } = 1;
}
