using BankingBackend.Core.Users;
using Microsoft.Extensions.Options;

namespace BankingBackend.Infrastructure.Authentication;

public sealed class RefreshTokenProvider : IRefreshTokenProvider
{
    private readonly JwtSettings _settings;

    public RefreshTokenProvider(IOptions<JwtSettings> options) => _settings = options.Value;
    public RefreshToken Create(Guid userId)
    {
        var expiresAtUtc = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationInDays);
        return RefreshToken.Create(userId, expiresAtUtc).Value;
    }
}