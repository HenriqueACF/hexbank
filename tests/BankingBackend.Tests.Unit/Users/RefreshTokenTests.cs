using BankingBackend.Core.Users;
using FluentAssertions;

namespace BankingBackend.Tests.Unit.Users;

public class RefreshTokenTests
{
    [Fact]
    public void Create_Deve_GerarTokenValido_ComExpiracaoFutura()
    {
        var result = RefreshToken.Create(Guid.NewGuid(), DateTime.UtcNow.AddDays(7));

        result.IsSuccess.Should().BeTrue();

        var token = result.Value;
        token.Value.Should().NotBe(Guid.Empty);
        token.IsRevoked.Should().BeFalse();
        token.IsExpired(DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void Create_Deve_Falhar_Quando_ExpiracaoForNoPassado()
    {
        var result = RefreshToken.Create(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RefreshTokenErrors.ExpirationInvalid);
    }

    [Fact]
    public void IsExpired_Deve_RetornarTrue_Quando_NowPassaDaExpiracao()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), DateTime.UtcNow.AddDays(7)).Value;

        token.IsExpired(DateTime.UtcNow).Should().BeFalse();
        token.IsExpired(DateTime.UtcNow.AddDays(8)).Should().BeTrue();
    }

    [Fact]
    public void Revoke_Deve_RevogarOToken()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), DateTime.UtcNow.AddDays(7)).Value;

        var result = token.Revoke();

        result.IsSuccess.Should().BeTrue();
        token.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void Revoke_Deve_Falhar_Quando_TokenJaRevogado()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), DateTime.UtcNow.AddDays(7)).Value;
        token.Revoke();

        var result = token.Revoke();

        result.Error.Should().Be(RefreshTokenErrors.AlreadyRevoked);
    }
}
