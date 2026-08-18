using BankingBackend.Core.Common;

namespace BankingBackend.Application.Users.Register;

public static class RegisterUserErrors
{
    public static readonly Error EmailAlreadyInUse = new("User.EmailAlreadyInUse", "Este E-mail já está cadastrado.");
    public static readonly Error CpfAlreadyInUse = new("User.CpfAlreadyInUse", "Este CPF já está cadastrado.");
}