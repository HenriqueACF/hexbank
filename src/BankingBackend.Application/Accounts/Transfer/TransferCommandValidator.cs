using FluentValidation;

namespace BankingBackend.Application.Accounts.Transfer;

public sealed class TransferCommandValidator : AbstractValidator<TransferCommand>
{
    public TransferCommandValidator()
    {
        RuleFor(c => c.FromAccountId).NotEmpty();
        RuleFor(c => c.ToAccountId).NotEmpty();
        RuleFor(c => c.Amount).GreaterThan(0).WithMessage("O valor deve ser maior do que zero.");
    }
}