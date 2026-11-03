using BankingBackend.Core.Common;
using Microsoft.AspNetCore.Mvc;

namespace BankingBackend.API.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
        => result.IsSuccess ? new OkObjectResult(result.Value) : MapError(result.Error);

    private static IActionResult MapError(Error error)
    {
        var body = new { error.Code, error.Message };
        return error.Code switch
        {
            "Auth.InvalidCredentials" or "RefreshToken.Invalid" => new UnauthorizedObjectResult(body),
            "Auth.InactiveAccount" or "Account.NotOwner" => new ObjectResult(body)
            {
                StatusCode = StatusCodes.Status403Forbidden
            },
            "Account.NotFound" => new NotFoundObjectResult(body),
            "User.EmailAlreadyInUse"
                or "User.CpfAlreadyInUse"
                or "Account.AccountInactive"
              => new ConflictObjectResult(body),
            _ => new BadRequestObjectResult(body)
        };
    }
}