using BankingBackend.Core.Accounts;
using BankingBackend.Core.Common;
using FluentValidation;
using MediatR;

namespace BankingBackend.Application.Accounts.Transfer;

public sealed class TransferCommandHandler : IRequestHandler<TransferCommand, Result<TransferResponse>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<TransferCommand> _validator;

    public TransferCommandHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        IValidator<TransferCommand> validator)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result<TransferResponse>> Handle(TransferCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var message = string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<TransferResponse>(new Error("Validation.Failed", message));
        }

        var from = await _accountRepository.GetByIdAsync(request.FromAccountId, cancellationToken);
        if (from is null)
            return Result.Failure<TransferResponse>(AccountErrors.NotFound);

        var to = await _accountRepository.GetByIdAsync(request.ToAccountId, cancellationToken);
        if (to is null)
            return Result.Failure<TransferResponse>(AccountErrors.NotFound);
        
        if (from.UserId != request.UserId)
            return Result.Failure<TransferResponse>(AccountErrors.NotOwner);

        var moneyResult = Money.Create(request.Amount);
        if (moneyResult.IsFailure)
            return Result.Failure<TransferResponse>(moneyResult.Error);

        var transferResult = TransferService.Transfer(from, to, moneyResult.Value);
        if (transferResult.IsFailure)
            return Result.Failure<TransferResponse>(transferResult.Error);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TransferResponse(from.Id, to.Id, request.Amount, from.Balance.Amount);
    }
}