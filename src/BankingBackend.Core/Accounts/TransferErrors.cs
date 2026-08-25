using BankingBackend.Core.Common;

namespace BankingBackend.Core.Accounts;

public static class TransferErrors
{
    public static readonly Error SameAccount = new("Transfer.SameAccount", "A conta de origem e a de destino não podem ser a mesma.");
    public static readonly Error InvalidAmount = new("Transfer.InvalidAmount", "O valor deve ser maior do que zero.");
}