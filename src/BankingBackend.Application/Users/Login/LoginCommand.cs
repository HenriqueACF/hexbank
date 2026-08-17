using BankingBackend.Core.Common;
using MediatR;

namespace BankingBackend.Application.Users.Login;

public sealed record LoginComand(string Email, string Password) : IRequest<Result<LoginResponse>>;