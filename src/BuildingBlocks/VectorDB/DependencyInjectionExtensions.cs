using BuildingBlocks.Constants;
using BuildingBlocks.Extensions;
using BuildingBlocks.VectorDB.Contracts;
using BuildingBlocks.VectorDB.SearchServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;

namespace BuildingBlocks.VectorDB;

public static class DependencyInjectionExtensions
{
    public static VectorStoreOptions AddVectorDB(
        this IHostApplicationBuilder builder,
        VectorDBType vectorDbType = VectorDBType.Qdrant
    )
    {
        var options = builder.Configuration.BindOptions<VectorStoreOptions>();

        builder.Services.AddConfigurationOptions<VectorStoreOptions>();
        builder.Services.AddSingleton<ISemanticSearch, SemanticSearch>();

        switch (vectorDbType)
        {
            case VectorDBType.Qdrant:
                // https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors/qdrant-connector
                if (builder.Configuration.GetConnectionString(AspireResources.Qdrant) is { } connectionString)
                {
                    // Add qdrant client to resolve for VectorStore (QdrantVectorStore)
                    builder.AddQdrantClient(AspireResources.Qdrant);
                    builder.Services.AddQdrantVectorStore();
                }
                else
                {
                    builder.Services.AddQdrantVectorStore(
                        host: options.Host,
                        port: options.Port,
                        https: options.UseHttps,
                        apiKey: options.ApiKey
                    );
                }

                break;
            case VectorDBType.InMemory:
                // https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors/inmemory-connector?pivots=programming-language-csharp
                builder.Services.AddInMemoryVectorStore();
                break;
        }

        return options;
    }
}
