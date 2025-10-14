using BuildingBlocks.Extensions;
using BuildingBlocks.VectorDB.Contracts;
using GenAIEshop.Catalogs.Products.Data.VectorModel;
using GenAIEshop.Catalogs.Products.Models;
using Humanizer;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace GenAIEshop.Catalogs.Products.Data;

public class ProductVectorIngestor(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStore vectorStore,
    ILogger<ProductVectorIngestor> logger
) : IDataIngestor<Product>
{
    public async Task IngestDataAsync(Product product, CancellationToken cancellationToken = default)
    {
        product.NotBeNull();
        product.Name.NotBeNullOrWhiteSpace();
        product.Description.NotBeNullOrWhiteSpace();

        var productsCollection = vectorStore.GetCollection<Guid, ProductVector>(nameof(ProductVector).Underscore());
        await productsCollection.EnsureCollectionExistsAsync(cancellationToken);

        var vectorSearchContent =
            $"Product Name: {product.Name}. Price: {product.Price:C}. Description: {product.Description}";
        // use this for using in full-text search in hybrid search
        var fullTextSearchContent = $"{product.Name} {product.Price:C} {product.Description}";

        // use this to store semantic meaning of the product in the type of vector in the vector store
        var embeddingsVector = await embeddingGenerator.GenerateVectorAsync(
            value: vectorSearchContent,
            cancellationToken: cancellationToken
        );

        var vectorEntity = new ProductVector
        {
            Id = product.Id,
            // use for full-text search
            Description = fullTextSearchContent,
            Vector = embeddingsVector,
            // these fields are used just for filtering in VectorSearch
            Name = product.Name,
            Price = (double)product.Price,
            IsAvailable = product.IsAvailable,
        };

        // update or insert vector data
        await productsCollection.UpsertAsync(vectorEntity, cancellationToken);

        logger.LogInformation("Product {ProductId} has been ingested.", product.Id);
    }

    public async Task IngestDataAsync(IList<Product> entities, CancellationToken cancellationToken = default)
    {
        entities.NotBeNull();

        foreach (var product in entities)
        {
            await IngestDataAsync(product, cancellationToken);
        }
    }

    public async Task DeleteDataAsync(Product product, CancellationToken cancellationToken = default)
    {
        product.NotBeNull();

        var productsCollection = vectorStore.GetCollection<Guid, ProductVector>(nameof(ProductVector).Underscore());
        await productsCollection.EnsureCollectionExistsAsync(cancellationToken);

        // delete vector data
        await productsCollection.DeleteAsync(product.Id, cancellationToken);
    }
}
