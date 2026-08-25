using BankingBackend.Core.Common;
using MediatR;

namespace BankingBackend.Application.Accounts.WithDraw;

public sealed record WithdrawCommand(Guid UserId, Guid AccountId, decimal Amount) :IRequest<Result<WithdrawResponse>>;