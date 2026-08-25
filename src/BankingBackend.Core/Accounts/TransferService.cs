using BankingBackend.Core.Common;

namespace BankingBackend.Core.Accounts;

public static class TransferService
{
    public static Result Transfer(Account from, Account to, Money amount)
    {
        if (from.Id == to.Id)
            return Result.Failure(TransferErrors.SameAccount);
        
        if(amount.Amount <=0)
            return Result.Failure(TransferErrors.InvalidAmount);

        if (!from.IsActive || !to.IsActive)
            return Result.Failure(AccountErrors.AccountInactive);

        if (from.Balance.Amount < amount.Amount)
            return Result.Failure(AccountErrors.InsufficientBalance);

        var withdrawResult = from.Withdraw(amount);
        if (withdrawResult.IsFailure)
            return withdrawResult;

        var depositResult = to.Deposit(amount);
        if (depositResult.IsFailure)
            return depositResult;

        return Result.Success();
    }
}