namespace BankingBackend.Core.Common.Users;

public sealed record UserCreatedDomainEvent(
    Guid Id,
    DateTime OccuredOnUtc,
    Guid UserId) : DomainEvent(Id, OccuredOnUtc);