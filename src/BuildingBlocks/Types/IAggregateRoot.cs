namespace BuildingBlocks.Types;

public interface IAggregateRoot
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }
    void RemoveDomainEvent(DomainEvent domainEvent);
    void ClearDomainEvents();
}
