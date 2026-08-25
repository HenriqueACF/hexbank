namespace BankingBackend.Application.Accounts.WithDraw;

public sealed record WithdrawResponse(Guid AccountId, decimal Balance);