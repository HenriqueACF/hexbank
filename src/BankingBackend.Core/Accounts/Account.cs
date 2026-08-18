using BankingBackend.Core.Common;

namespace BankingBackend.Core.Accounts;

public sealed class Account: Entity
{
    private Account(
        Guid id,
        Guid userId,
        string number,
        Money balance,
        DateTime createdAtUtc) : base(id)
    {
        UserId = userId;
        Number = number;
        Balance = balance;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }
    
    private Account(){}
    
    public Guid UserId { get; private set; }
    public string Number { get; private set; } = null!;
    public Money Balance { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static Result<Account> Open(Guid userId)
    {
        var account = new Account(
            Guid.NewGuid(),
            userId,
            GenerateNumber(),
            Money.Zero,
            DateTime.UtcNow);
        
        account.RaiseDomainEvent(new AccountOpenedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            account.Id,
            userId));

        return account;
    }

    public Result Deposit(Money amount)
    {
        if (amount.Amount <= 0)
            return Result.Failure(AccountErrors.InvalidAmout);

        if (!IsActive)
            return Result.Failure(AccountErrors.AccountInactive);

        Balance = Balance.Add(amount);
        return Result.Success();
    }

    public Result Withdraw(Money amount)
    {
        if (amount.Amount <= 0)
            return Result.Failure(AccountErrors.InvalidAmout);

        if (!IsActive)
            return Result.Failure(AccountErrors.AccountInactive);

        if (amount.Amount > Balance.Amount)
        {
            return Result.Failure(AccountErrors.InsufficientBalance);
        }

        Balance = Balance.Subtract(amount).Value;
        return Result.Success();
    }

    public Result Close()
    {
        if (!IsActive)
            return Result.Failure(AccountErrors.AlreadyInactive);

        IsActive = false;
        return Result.Success();
    }

    public Result Reopen()
    {
        if (IsActive)
            return Result.Failure(AccountErrors.AlreadyActive);

        IsActive = true;
        return Result.Success();
    }
    
    private static string GenerateNumber() =>
        Random.Shared.Next(10_000_000, 99_999_999).ToString();
    
    
}