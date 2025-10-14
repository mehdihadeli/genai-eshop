using BuildingBlocks.VectorDB;
using Microsoft.Extensions.VectorData;

namespace GenAIEshop.Catalogs.Products.Data.VectorModel;

public class ProductVector : VectorEntityBase
{
    // https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/defining-your-data-model?pivots=programming-language-csharp#vectorstoredataattribute
    // these fields are used just for filtering in VectorSearch
    // IsIndexed: Indicates whether the property should be indexed for `filtering` in cases where a database requires opting in to indexing per property
    [VectorStoreData(IsIndexed = true)]
    public string? Name { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public double Price { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public bool IsAvailable { get; set; }
}
