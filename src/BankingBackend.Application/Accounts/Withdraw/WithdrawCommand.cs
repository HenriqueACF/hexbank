using BankingBackend.Core.Common;
using MediatR;

namespace BankingBackend.Application.Accounts.Withdraw;

public sealed record WithdrawCommand(Guid UserId, Guid AccountId, decimal Amount) :IRequest<Result<WithdrawResponse>>;