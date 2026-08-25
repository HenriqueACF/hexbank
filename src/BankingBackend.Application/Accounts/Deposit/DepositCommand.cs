using BankingBackend.Core.Common;
using MediatR;

namespace BankingBackend.Application.Accounts.Deposit;

public sealed record DepositCommand(Guid UserId, Guid AccountId, decimal Amount): IRequest<Result<DepositResponse>>;