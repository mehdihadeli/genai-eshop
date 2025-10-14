namespace BuildingBlocks.Types;

public abstract class Entity
{
    public Guid Id { get; set; }
}

public abstract class Entity<TId> : Entity
{
    public new TId Id { get; set; } = default!;
}
