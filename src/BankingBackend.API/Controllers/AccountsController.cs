using System.Security.Claims;
using BankingBackend.Application.Accounts.Deposit;
using BankingBackend.Application.Accounts.OpenAccount;
using BankingBackend.Application.Accounts.Withdraw;
using BankingBackend.Core.Common;
using BankingBackend.Application.Accounts.Transfer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingBackend.API.Controllers;

[ApiController]
[Route("api/v1/accounts")]
[Authorize]
public sealed class AccountsController : ControllerBase
{
    private readonly ISender _sender;

    public AccountsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> OpenAccount(CancellationToken cancellationToken)
    {
        var command = new OpenAccountCommand(GetCurrentUserId());
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    [HttpPost("{accountId:guid}/deposit")]
    public async Task<IActionResult> Deposit(
        Guid accountId,
        DepositCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            command with { AccountId = accountId, UserId = GetCurrentUserId() },
            cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    [HttpPost("{accountId:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(
        Guid accountId,
        WithdrawCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            command with { AccountId = accountId, UserId = GetCurrentUserId() },
            cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }
    
    [HttpPost("{fromAccountId:guid}/transfers")]
    public async Task<IActionResult> Transfer(
        Guid fromAccountId,
        TransferCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            command with { FromAccountId = fromAccountId, UserId = GetCurrentUserId() },
            cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private IActionResult MapError(Error error) => error.Code switch
    {
        "Account.NotFound" => NotFound(new { error.Code, error.Message }),
        "Account.NotOwner" => StatusCode(StatusCodes.Status403Forbidden, new { error.Code, error.Message }),
        "Account.AccountInactive" => StatusCode(StatusCodes.Status409Conflict, new { error.Code, error.Message }),
        _ => BadRequest(new { error.Code, error.Message })
    };
}