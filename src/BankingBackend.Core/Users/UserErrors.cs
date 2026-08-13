using BankingBackend.Core.Common;

namespace BankingBackend.Core.Users;

public static class EmailErrors
{
    public static readonly Error Empty = new("Email.Empty", "O E-mail não pode ser vazio.");
    public static readonly Error InvalidFormat = new("Email.InvalidFormat", "O formato do e-mail é inválido.");
    public static readonly Error TooLong = new("Email.TooLong", "O e-mail não pode ter mais de 256 caracteres.");
}

public static class CpfErrors
{
    public static readonly Error Empty = new("Cpf.Empty", "O CPF não pode ser vazio.");
    public static readonly Error InvalidLength = new("Cpf.InvalidLength", "O CPF deve ter 11 digitos.");
    public static readonly Error Invalid = new("Cpf.Invalid", "O CPF informado é inválido.");
}

public static class UserErrors
{
    public static readonly Error PasswordHashEmpty = new("User.PasswordHashEmpty", "O hash de senha é obrigatório.");
    public static readonly Error AlreadyActive = new("User.AlreadyActive", "O usuárop já está ativo.");
    public static readonly Error AlreadyInactive = new("User.AlreadInactive", "o usuário já está inativo.");
}