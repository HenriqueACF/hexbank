using FluentValidation;

namespace BankingBackend.Application.Accounts.WithDraw;

public sealed class WithdrawCommandValidator : AbstractValidator<WithdrawCommand>
{
    public WithdrawCommandValidator()
    {
        RuleFor(c => c.AccountId).NotEmpty();
        RuleFor(c => c.Amount).GreaterThan(0).WithMessage("O valor deve ser maior do que zero.");

    }
}