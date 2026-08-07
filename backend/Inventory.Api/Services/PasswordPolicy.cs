namespace Inventory.Api.Services;

// Minimum password rule applied whenever a password is set (registration or password
// change) — never on login, which only verifies an existing password.
public static class PasswordPolicy
{
    public const int MinLength = 12;

    public static string? Validate(string password)
    {
        if (password.Length < MinLength)
        {
            return $"Password must be at least {MinLength} characters long.";
        }

        if (!password.Any(char.IsUpper))
        {
            return "Password must contain at least one uppercase letter.";
        }

        if (!password.Any(char.IsLower))
        {
            return "Password must contain at least one lowercase letter.";
        }

        if (!password.Any(char.IsDigit))
        {
            return "Password must contain at least one digit.";
        }

        return null;
    }
}
