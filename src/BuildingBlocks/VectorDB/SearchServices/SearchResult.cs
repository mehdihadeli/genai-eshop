namespace BuildingBlocks.VectorDB.SearchServices;

public class VectorSearchResult<TEntity>
{
    public string Search { get; set; } = default!;
    public List<TEntity> Data { get; set; } = new();
    public string AIExplanationMessage { get; set; } = default!;
}
