using BankingBackend.Infrastructure.Authentication;
using FluentAssertions;

namespace BankingBackend.Tests.Unit.Authentication;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_Deve_GerarValorDiferenteDaSenhaOriginal()
    {
        var hash = _hasher.Hash("senha-secreta");

        hash.Should().NotBe("senha-secreta");
        hash.Should().HaveLength(60);
    }

    [Fact]
    public void Hash_Deve_GerarValoresDiferentes_ParaAMesmaSenha()
    {
        var primeiro = _hasher.Hash("senha-secreta");
        var segundo = _hasher.Hash("senha-secreta");

        primeiro.Should().NotBe(segundo);
    }

    [Fact]
    public void Verify_Deve_AceitarASenhaCorreta()
    {
        var hash = _hasher.Hash("senha-secreta");

        _hasher.Verify("senha-secreta", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_Deve_RejeitarSenhaErrada()
    {
        var hash = _hasher.Hash("senha-secreta");

        _hasher.Verify("senha-errada", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_Deve_RejeitarSenhaComCaixaDiferente()
    {
        var hash = _hasher.Hash("SenhaSecreta");

        _hasher.Verify("senhasecreta", hash).Should().BeFalse();
    }
}
