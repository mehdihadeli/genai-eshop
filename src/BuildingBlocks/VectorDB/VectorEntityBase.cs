using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;

namespace BuildingBlocks.VectorDB;

// https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/defining-your-data-model?pivots=programming-language-csharp
// https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/schema-with-record-definition
// https://learn.microsoft.com/en-us/semantic-kernel/concepts/text-search/text-search-vector-stores?pivots=programming-language-csharp
public abstract class VectorEntityBase
{
    [VectorStoreKey]
    public required Guid Id { get; init; }

    // https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/hybrid-search?pivots=programming-language-csharp
    // https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/defining-your-data-model?pivots=programming-language-csharp#vectorstoredataattribute
    // contains merge fields to search like: `{product.Name} {product.Price:C} {product.Description}` for doing full-text-search in hybrid-search
    [TextSearchResultValue]
    [VectorStoreData(IsFullTextIndexed = true, IsIndexed = true)]
    public string? Description { get; init; }

    [VectorStoreVector(1536, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; init; }
}
