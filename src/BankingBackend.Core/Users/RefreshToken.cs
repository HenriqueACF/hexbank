using BankingBackend.Core.Common;

namespace BankingBackend.Core.Users;

public sealed class RefreshToken : Entity
{
    private RefreshToken(Guid id, Guid userId, Guid value, DateTime createdAtUtc, DateTime expiresAtUtc) : base(id)
    {
        UserId = userId;
        Value = value;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    private RefreshToken() {}
    
    public Guid UserId { get; private set; }
    public Guid Value { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }

    public bool IsRevoked => RevokedAtUtc is not null;
    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAtUtc;

    public static Result<RefreshToken> Create(Guid userId, DateTime expiresAtUtc)
    {
        if (expiresAtUtc <= DateTime.UtcNow)
        {
            return Result.Failure<RefreshToken>(RefreshTokenErrors.ExpirationInvalid);
        }
        
        return new RefreshToken(Guid.NewGuid(),
            userId,
            Guid.NewGuid(),
            DateTime.UtcNow,
            expiresAtUtc);
    }

    public Result Revoke()
    {
        if (IsRevoked)
        {
            return Result.Failure(RefreshTokenErrors.AlreadyRevoked);
        }

        RevokedAtUtc = DateTime.UtcNow;
        return Result.Success();
    }
}