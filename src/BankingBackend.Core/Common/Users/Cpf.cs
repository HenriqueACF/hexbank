namespace BankingBackend.Core.Common.Users;

public class Cpf: ValueObject
{
    private const int Length = 11;
    private Cpf(string value) => Value = value;
    public string Value { get; }

    public static Result<Cpf> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Cpf>(CpfErrors.Empty);

        var digits = new string(value.Where(char.IsDigit).ToArray());

        if (digits.Length != Length)
            return Result.Failure<Cpf>(CpfErrors.InvalidLength);

        if (!HasValidCheckedDigits(digits))
            return Result.Failure<Cpf>(CpfErrors.Invalid);

        return new Cpf(digits);
    }

    private static bool HasValidCheckedDigits(string digits)
    {
        if (digits.All(d => d == digits[0]))
            return false;

        var numbers = digits.Select(d => d - '0').ToArray();
        var first = CalculateCheckDigit(numbers, 9);
        if (numbers[9] != first)
            return false;
        
        var second = CalculateCheckDigit(numbers, 10);
        return numbers[10] == second;
    }

    private static int CalculateCheckDigit(int[] numbers, int count)
    {
        var weight = count + 1;
        var sum = 0;
        for (var i = 0; i < count; i++)
            sum += numbers[i] * weight--;

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}