using BankingBackend.API.Extensions;
using BankingBackend.Core.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace BankingBackend.Tests.Integration;

public class ResultMappingTests
{
    [Theory]
    [InlineData("Auth.InvalidCredentials", 401)]
    [InlineData("RefreshToken.Invalid", 401)]
    [InlineData("Auth.InactiveAccount", 403)]
    [InlineData("Account.NotOwner", 403)]
    [InlineData("Account.NotFound", 404)]
    [InlineData("User.EmailAlreadyInUse", 409)]
    [InlineData("User.CpfAlreadyInUse", 409)]
    [InlineData("Account.AccountInactive", 409)]
    [InlineData("Validation.Failed", 400)]
    [InlineData("Account.InsufficentceBalance", 400)]
    public void ToActionResult_Deve_MapearCodigoParaOStatusCorreto(string code, int expectedStatus)
    {
        var result = Result.Failure<string>(new Error(code, "mensagem"));

        var action = result.ToActionResult();

        ((IStatusCodeActionResult)action).StatusCode.Should().Be(expectedStatus);
    }

    [Fact]
    public void ToActionResult_Deve_RetornarOk_ComOValor_Quando_Sucesso()
    {
        var result = Result.Success("valor");

        var action = result.ToActionResult();

        action.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be("valor");
    }
}
