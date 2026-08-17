using BankingBackend.Core.Common;
using MediatR;

namespace BankingBackend.Application.Users.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;