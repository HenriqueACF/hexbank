using BankingBackend.Core.Common;

namespace BankingBackend.Core.Accounts;

public static class MoneyErrors
{
    public static readonly Error Negative = new("Money.Negative", "O valor não pode ser negativo.");
}

public static class AccountErrors
{
    public static readonly Error InvalidAmout = new("Account.InvalidAmout", "O valor na operação deve ser maior do que zero.");

    public static readonly Error InsufficientBalance =
        new("Account.InsufficentceBalance", "Saldo insuficiente para esta operação");

    public static readonly Error AccountInactive = new("Account.AccountInactive", "Está conta está inativa.");
    
    public static readonly Error AlreadyActive = new("Account.AlreadActive", "Está conta já está ativa.");

    public static readonly Error AlreadyInactive = new("Account.AlreadyInactive", "Esta conta está inativa.");
}