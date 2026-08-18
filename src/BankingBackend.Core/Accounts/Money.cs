using System.Globalization;
using BankingBackend.Core.Common;

namespace BankingBackend.Core.Accounts;

public sealed class Money: ValueObject
{
    public static readonly Money Zero = new(0m);
    private Money(decimal amount) => Amount = amount;
    public decimal Amount { get; }

    public static Result<Money> Create(decimal amount)
    {
        if (amount < 0)
            return Result.Failure<Money>(MoneyErrors.Negative);

        return new Money(amount);
    }

    public Money Add(Money other) => new(Amount + other.Amount);

    public Result<Money> Subtract(Money other)
    {
        if (other.Amount > Amount)
            return Result.Failure<Money>(MoneyErrors.Negative);

        return new Money(Amount - other.Amount);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }

    public override string ToString() => Amount.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
}