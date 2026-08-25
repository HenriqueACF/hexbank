using BankingBackend.Core.Common;
using MediatR;

namespace BankingBackend.Application.Accounts.OpenAccount;

public sealed record OpenAccountCommand(Guid UserId) : IRequest<Result<OpenAccountResponse>>;