using BuildingBlocks.Types;

namespace BuildingBlocks.VectorDB.Contracts;

public interface IDataIngestor<TEntity>
    where TEntity : Entity
{
    Task IngestDataAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task IngestDataAsync(IList<TEntity> entities, CancellationToken cancellationToken = default);
    Task DeleteDataAsync(TEntity entity, CancellationToken cancellationToken = default);
}
