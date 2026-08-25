using System.Reflection.Metadata;
using BankingBackend.Core.Accounts;
using BankingBackend.Core.Common;
using FluentValidation;
using MediatR;

namespace BankingBackend.Application.Accounts.Deposit;

public sealed class DepositCommandHandler : IRequestHandler<DepositCommand, Result<DepositResponse>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<DepositCommand> _validator;

    public DepositCommandHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        IValidator<DepositCommand> validator)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result<DepositResponse>> Handle(
        DepositCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var message = string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<DepositResponse>(new Error("Validation.Failed", message));
        }

        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account is null)
            return Result.Failure<DepositResponse>(AccountErrors.NotFound);
        
        if (account.UserId != request.UserId)
            return Result.Failure<DepositResponse>(AccountErrors.NotOwner);

        var moneyResult = Money.Create(request.Amount);
        if (moneyResult.IsFailure)
            return Result.Failure<DepositResponse>(moneyResult.Error);

        var depositResult = account.Deposit(moneyResult.Value);
        if (depositResult.IsFailure)
            return Result.Failure<DepositResponse>(depositResult.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DepositResponse(account.Id, account.Balance.Amount);
    }
    
}