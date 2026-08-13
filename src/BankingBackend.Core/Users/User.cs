using BankingBackend.Core.Common;

namespace BankingBackend.Core.Users;

public sealed class User : Entity
{
    private User(
        Guid id,
        Cpf cpf,
        Email email,
        string passwordHash,
        UserRole role,
        DateTime createdAtUtc) : base(id)
    {
        Cpf = cpf;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    private User() { } 

    public Cpf Cpf { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static Result<User> Create(
        Cpf cpf,
        Email email,
        string passwordHash,
        UserRole role)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure<User>(UserErrors.PasswordHashEmpty);

        var user = new User(
            Guid.NewGuid(),
            cpf,
            email,
            passwordHash,
            role,
            DateTime.UtcNow);

        user.RaiseDomainEvent(new UserCreatedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            user.Id));

        return user;
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(UserErrors.AlreadyInactive);

        IsActive = false;
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure(UserErrors.AlreadyActive);

        IsActive = true;
        return Result.Success();
    }

    public void ChangeEmail(Email email) => Email = email;
}