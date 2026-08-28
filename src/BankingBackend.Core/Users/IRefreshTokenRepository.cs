namespace BankingBackend.Core.Users;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByValueAsync(Guid value, CancellationToken cancellationToken = default);
    void Add(RefreshToken refreshToken);
}