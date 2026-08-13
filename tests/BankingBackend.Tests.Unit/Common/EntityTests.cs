using BankingBackend.Core.Common;
using FluentAssertions;

namespace BankingBackend.Tests.Unit.Common;

public class EntityTests
{
    // ── auxiliares: existem só para este arquivo ──
    private sealed class TestEntity : Entity
    {
        public TestEntity(Guid id) : base(id) { }
        public void Raise(IDomainEvent e) => RaiseDomainEvent(e);
    }

    private sealed class OtherEntity : Entity
    {
        public OtherEntity(Guid id) : base(id) { }
    }

    private sealed record TestEvent(Guid Id, DateTime OccurredOnUtc)
        : DomainEvent(Id, OccurredOnUtc);

    private static TestEvent NewEvent() => new(Guid.NewGuid(), DateTime.UtcNow);

    // ── testes ──
    [Fact]
    public void Entidades_Deve_SeremIguais_Quando_IdForOMesmo()
    {
        var id = Guid.NewGuid();

        var a = new TestEntity(id);
        var b = new TestEntity(id);

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Entidades_Deve_SeremDiferentes_Quando_IdForDiferente()
    {
        var a = new TestEntity(Guid.NewGuid());
        var b = new TestEntity(Guid.NewGuid());

        a.Should().NotBe(b);
    }

    [Fact]
    public void Entidades_Deve_SeremDiferentes_Quando_TipoForDiferente()
    {
        var id = Guid.NewGuid();

        var a = new TestEntity(id);
        var b = new OtherEntity(id);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_Deve_RetornarFalso_Quando_ComparadoComNull()
    {
        var a = new TestEntity(Guid.NewGuid());

        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Operadores_Deve_SeremCoerentesComEquals()
    {
        var id = Guid.NewGuid();

        var a = new TestEntity(id);
        var b = new TestEntity(id);
        var c = new TestEntity(Guid.NewGuid());

        (a == b).Should().BeTrue();
        (a != c).Should().BeTrue();
        ((TestEntity?)null == (TestEntity?)null).Should().BeTrue();
    }

    [Fact]
    public void RaiseDomainEvent_Deve_RegistrarOEvento()
    {
        var entity = new TestEntity(Guid.NewGuid());
        var domainEvent = NewEvent();

        entity.Raise(domainEvent);

        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().Be(domainEvent);
    }

    [Fact]
    public void ClearDomainEvents_Deve_EsvaziarALista()
    {
        var entity = new TestEntity(Guid.NewGuid());
        entity.Raise(NewEvent());

        entity.ClearDomainEvents();

        entity.DomainEvents.Should().BeEmpty();
    }
}