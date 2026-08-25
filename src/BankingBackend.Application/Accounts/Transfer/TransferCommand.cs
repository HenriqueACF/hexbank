using BankingBackend.Core.Common;
using MediatR;

namespace BankingBackend.Application.Accounts.Transfer;

public sealed record TransferCommand(
    Guid UserId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount)
    : IRequest<Result<TransferResponse>>;