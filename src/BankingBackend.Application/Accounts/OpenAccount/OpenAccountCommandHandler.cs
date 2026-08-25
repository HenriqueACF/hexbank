using System.Reflection.Metadata;
using BankingBackend.Core.Accounts;
using BankingBackend.Core.Common;
using MediatR;

namespace BankingBackend.Application.Accounts.OpenAccount;

public sealed class OpenAccountCommandHandler : IRequestHandler<OpenAccountCommand, Result<OpenAccountResponse>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public OpenAccountCommandHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result<OpenAccountResponse>> Handle(
        OpenAccountCommand request,
        CancellationToken cancellationToken)
    {
        var accountResult = Account.Open(request.UserId);
        if (accountResult.IsFailure)
            return Result.Failure<OpenAccountResponse>(accountResult.Error);

        var account = accountResult.Value;
        _accountRepository.Add(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new OpenAccountResponse(account.Id, account.Number, account.Balance.Amount);
    }
    
}
