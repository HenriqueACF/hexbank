using BankingBackend.Core.Common;

namespace BankingBackend.Core.Users;

public sealed record UserCreatedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid UserId) : DomainEvent(Id, OccurredOnUtc);