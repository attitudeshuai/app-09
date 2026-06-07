namespace KnowledgeBase.Application.DTOs.Levels;

public class LevelDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int LevelNumber { get; set; }
    public int RequiredPoints { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public string? Description { get; set; }
}

public class CreateLevelRequest
{
    public string Name { get; set; } = string.Empty;
    public int LevelNumber { get; set; }
    public int RequiredPoints { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public string? Description { get; set; }
}

public class UpdateLevelRequest
{
    public string? Name { get; set; }
    public int? LevelNumber { get; set; }
    public int? RequiredPoints { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public string? Description { get; set; }
}
