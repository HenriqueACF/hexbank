namespace BankingBackend.Core.Common;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOnUtc { get; }
}

public abstract record DomainEvent(Guid Id, DateTime OccurredOnUtc): IDomainEvent;