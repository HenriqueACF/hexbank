using BankingBackend.Core.Common;

namespace BankingBackend.Application.Users.Login;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = new("Auth.InvalidCredentials", "E-mail ou senha inválidos");

    public static readonly Error InactiveAccount =
        new("Auth.InactiveAccount", "Esta conta está inativa. Entre em contato com o suporte.");
}