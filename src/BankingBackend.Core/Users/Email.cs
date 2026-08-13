using System.Text.RegularExpressions;
using BankingBackend.Core.Common;

namespace BankingBackend.Core.Users;

public sealed class Email : ValueObject
{
    private const int MaxLength = 256;
    private static readonly Regex Pattern = new (@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled);

    private Email(string value) => Value = value;
    public string Value { get; }

    public static Result<Email> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Email>(EmailErrors.Empty);

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
            return Result.Failure<Email>(EmailErrors.TooLong);
        
        if (!Pattern.IsMatch(normalized))
            return Result.Failure<Email>(EmailErrors.InvalidFormat);

        return new Email(normalized);
    }
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

}