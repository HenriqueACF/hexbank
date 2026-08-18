using BankingBackend.Core.Common;

namespace BankingBackend.Core.Accounts;

public sealed record AccountOpenedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid AccountId,
    Guid UserId) : DomainEvent(Id, OccurredOnUtc);