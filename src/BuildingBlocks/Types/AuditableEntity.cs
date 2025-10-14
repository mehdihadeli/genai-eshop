namespace BuildingBlocks.Types;

public abstract class AuditableEntity<TId> : AuditableEntity
{
    public new TId Id { get; set; } = default!;
}

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public Guid? LastModifiedBy { get; set; }

    // https://www.npgsql.org/efcore/modeling/concurrency.html?tabs=fluent-api
    // https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations#application-managed-concurrency-tokens
    public uint Version { get; set; }
}
