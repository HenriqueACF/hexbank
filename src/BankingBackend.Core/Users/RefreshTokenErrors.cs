using BankingBackend.Core.Common;

namespace BankingBackend.Core.Users;

public static class RefreshTokenErrors
{
    public static readonly Error ExpirationInvalid =
        new("RefreshToken.ExpirationInvalid", "A expiração do refresh token deve ser no futuro.");

    public static readonly Error AlreadyRevoked =
        new("RefreshToken.AlreadyRevoked", "O refresh token já está revogado.");

    public static readonly Error Invalid =
        new("RefreshToken.Invalid", "Refresh token inválido ou expirado.");
}