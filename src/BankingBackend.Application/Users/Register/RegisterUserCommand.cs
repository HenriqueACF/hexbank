using BankingBackend.Core.Common;
using MediatR;

namespace BankingBackend.Application.Users.Register;

public sealed record RegisterUserCommand(string Cpf, string Email, string Password): IRequest<Result<RegisterUserResponse>>;
