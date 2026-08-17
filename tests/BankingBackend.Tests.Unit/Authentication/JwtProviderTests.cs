using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BankingBackend.Core.Users;
using BankingBackend.Infrastructure.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Options;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace BankingBackend.Tests.Unit.Authentication;

public class JwtProviderTests
{
    private static readonly JwtSettings Settings = new()
    {
        Issuer = "hexbank-test",
        Audience = "hexbank-test-client",
        SecretKey = "chave-de-teste-com-mais-de-32-bytes-para-hmac-sha256",
        ExpirationInMinutes = 60
    };

    private readonly JwtProvider _provider = new(Options.Create(Settings));

    private static User NewUser() => User.Create(
        Cpf.Create("11144477735").Value,
        Email.Create("henrique@hexbank.com").Value,
        "hash-fake",
        UserRole.Admin).Value;

    private static JwtSecurityToken Decode(string token) =>
        new JwtSecurityTokenHandler().ReadJwtToken(token);

    [Fact]
    public void Generate_Deve_ProduzirTokenComTresPartes()
    {
        var token = _provider.Generate(NewUser());

        token.Should().NotBeNullOrWhiteSpace();
        token.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public void Generate_Deve_IncluirIdEmailERole()
    {
        var user = NewUser();

        var token = Decode(_provider.Generate(user));

        token.Claims.Should().Contain(c =>
            c.Type == JwtRegisteredClaimNames.Sub &&
            c.Value == user.Id.ToString());

        token.Claims.Should().Contain(c =>
            c.Type == JwtRegisteredClaimNames.Email &&
            c.Value == user.Email.Value);

        token.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.Role &&
            c.Value == UserRole.Admin.ToString());
    }

    [Fact]
    public void Generate_Deve_UsarIssuerEAudienceConfigurados()
    {
        var token = Decode(_provider.Generate(NewUser()));

        token.Issuer.Should().Be(Settings.Issuer);
        token.Audiences.Should().Contain(Settings.Audience);
    }

    [Fact]
    public void Generate_Deve_DefinirExpiracaoConformeConfiguracao()
    {
        var token = Decode(_provider.Generate(NewUser()));

        token.ValidTo.Should().BeCloseTo(
            DateTime.UtcNow.AddMinutes(Settings.ExpirationInMinutes),
            TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Generate_Deve_ProduzirTokensDiferentes_ACadaChamada()
    {
        var user = NewUser();

        _provider.Generate(user).Should().NotBe(_provider.Generate(user));
    }

    [Fact]
    public void Generate_Deve_AssinarComHmacSha256()
    {
        var token = Decode(_provider.Generate(NewUser()));

        token.Header.Alg.Should().Be("HS256");
    }
}
