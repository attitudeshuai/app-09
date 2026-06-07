namespace KnowledgeBase.Domain.Interfaces;

public interface IPasswordValidator
{
    PasswordValidationResult Validate(string password);
}

public class PasswordValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}
