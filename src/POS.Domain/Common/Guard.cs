namespace Pos.Domain.Common;

internal static class Guard
{
    public static string RequireText(string? value, string fieldName, int maxLength)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            throw new DomainException($"{fieldName} не может быть пустым.");
        }

        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"{fieldName} не может быть длиннее {maxLength} символов.");
        }

        return trimmed;
    }

    public static string? OptionalText(string? value, string fieldName, int maxLength)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"{fieldName} не может быть длиннее {maxLength} символов.");
        }

        return trimmed;
    }
}