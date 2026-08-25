namespace BankingBackend.Application.Accounts.Transfer;

public sealed record TransferResponse(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    decimal FromBalance);