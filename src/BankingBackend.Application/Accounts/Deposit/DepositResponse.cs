namespace BankingBackend.Application.Accounts.Deposit;

public sealed record DepositResponse(Guid AccountId, decimal Balance);