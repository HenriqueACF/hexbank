using BankingBackend.Core.Accounts;
using BankingBackend.Core.Common;
using FluentValidation;
using MediatR;

namespace BankingBackend.Application.Accounts.Withdraw;

public sealed class WithdrawCommandHandler
    : IRequestHandler<WithdrawCommand, Result<WithdrawResponse>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<WithdrawCommand> _validator;

    public WithdrawCommandHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        IValidator<WithdrawCommand> validator)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result<WithdrawResponse>> Handle(
        WithdrawCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var message = string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<WithdrawResponse>(new Error("Validation.Failed", message));
        }

        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account is null)
            return Result.Failure<WithdrawResponse>(AccountErrors.NotFound);

        if (account.UserId != request.UserId)
            return Result.Failure<WithdrawResponse>(AccountErrors.NotOwner);

        var moneyResult = Money.Create(request.Amount);
        if (moneyResult.IsFailure)
            return Result.Failure<WithdrawResponse>(moneyResult.Error);

        var withdrawResult = account.Withdraw(moneyResult.Value);
        if (withdrawResult.IsFailure)
            return Result.Failure<WithdrawResponse>(withdrawResult.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new WithdrawResponse(account.Id, account.Balance.Amount);
    }
}