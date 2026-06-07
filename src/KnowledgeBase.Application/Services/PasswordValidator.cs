using KnowledgeBase.Domain.Interfaces;
using System.Text.RegularExpressions;

namespace KnowledgeBase.Application.Services;

public class PasswordValidator : IPasswordValidator
{
    private const int MinimumLength = 8;

    public PasswordValidationResult Validate(string password)
    {
        var result = new PasswordValidationResult();

        if (string.IsNullOrWhiteSpace(password))
        {
            result.Errors.Add("密码不能为空");
            return result;
        }

        if (password.Length < MinimumLength)
        {
            result.Errors.Add($"密码长度不能少于 {MinimumLength} 位");
        }

        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            result.Errors.Add("密码必须包含小写字母");
        }

        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            result.Errors.Add("密码必须包含大写字母");
        }

        if (!Regex.IsMatch(password, @"[0-9]"))
        {
            result.Errors.Add("密码必须包含数字");
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }
}
