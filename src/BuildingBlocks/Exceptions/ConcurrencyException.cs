namespace BuildingBlocks.Exceptions;

public class ConcurrencyException<TId>(TId id)
    : DomainException($"A different version than expected was found in aggregate {id}");
