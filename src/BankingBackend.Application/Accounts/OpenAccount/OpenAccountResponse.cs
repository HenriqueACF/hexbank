namespace BankingBackend.Application.Accounts.OpenAccount;

public sealed record OpenAccountResponse(Guid AccountId, string Number, decimal Balance);