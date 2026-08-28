using BankingBackend.Application.Users.Login;
using BankingBackend.Core.Common;
using MediatR;

namespace BankingBackend.Application.Users.Refresh;

public sealed record RefreshTokenCommand(Guid RefreshTokenValue) : IRequest<Result<LoginResponse>>;