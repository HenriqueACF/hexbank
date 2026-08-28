namespace BankingBackend.Core.Users;

public interface IRefreshTokenProvider
{
    RefreshToken Create(Guid userId);
}