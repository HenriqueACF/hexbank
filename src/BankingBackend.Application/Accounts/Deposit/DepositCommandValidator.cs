using FluentValidation;

namespace BankingBackend.Application.Accounts.Deposit;

public sealed class DepositCommandValidator: AbstractValidator<DepositCommand>
{
    public DepositCommandValidator()
    {
        RuleFor(c => c.AccountId).NotEmpty();
        RuleFor(c => c.Amount).GreaterThan(0).WithMessage("O valor deve ser maior do que 0.");
    }
}