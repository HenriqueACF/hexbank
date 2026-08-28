using FluentValidation;

namespace BankingBackend.Application.Users.Refresh;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(c => c.RefreshTokenValue).NotEmpty();
    }
}