namespace BankingBackend.Application.Accounts.Withdraw;

public sealed record WithdrawResponse(Guid AccountId, decimal Balance);