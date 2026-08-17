using BankingBackend.Application.Users.Login;
using BankingBackend.Core.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankingBackend.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    private IActionResult MapError(Error error) => error.Code switch
    {
        "Auth.InvalidCredentials" => Unauthorized(new { error.Code, error.Message }),
        "Auth.InactiveAccount" => StatusCode(StatusCodes.Status403Forbidden, new { error.Code, error.Message }),
        _ => BadRequest(new { error.Code, error.Message })
    };

}