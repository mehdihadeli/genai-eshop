using BuildingBlocks.Constants;
using GenAIEshop.Shared.Constants;
using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder
    .AddPostgres(AspireResources.Postgres)
    .WithImage("postgres", "latest")
    .WithImagePullPolicy(ImagePullPolicy.Missing);

IResourceBuilder<PostgresDatabaseResource> catalogsPostgres = postgres.AddDatabase(
    AspireApplicationResources.PostgresDatabase.Catalogs,
    nameof(AspireApplicationResources.PostgresDatabase.Catalogs).ToLowerInvariant()
);

IResourceBuilder<PostgresDatabaseResource> ordersPostgres = postgres.AddDatabase(
    AspireApplicationResources.PostgresDatabase.Orders,
    nameof(AspireApplicationResources.PostgresDatabase.Orders).ToLowerInvariant()
);

IResourceBuilder<PostgresDatabaseResource> reviewsPostgres = postgres.AddDatabase(
    AspireApplicationResources.PostgresDatabase.Reviews,
    nameof(AspireApplicationResources.PostgresDatabase.Orders).ToLowerInvariant()
);

IResourceBuilder<RedisResource> redis = builder
    .AddRedis(AspireResources.Redis)
    .WithImagePullPolicy(ImagePullPolicy.Missing)
    .WithIconName("Redis")
    .WithIconName("Memory")
    .WithImage("redis/redis-stack", "latest");

IResourceBuilder<QdrantServerResource> qdrant = builder
    .AddQdrant(AspireResources.Qdrant)
    .WithImagePullPolicy(ImagePullPolicy.Missing)
    .WithDataVolume()
    .WithIconName("DatabaseSearch")
    .WithImage("qdrant/qdrant", "latest");

// var AgentFrameworkOptions = builder.Configuration.BindOptions<AgentFrameworkOptions>();
// var ollama = builder
//     .AddOllama(AspireResources.Ollama, 11434)
//     .WithImage("ollama/ollama", "0.12.1")
//     .WithImagePullPolicy(ImagePullPolicy.Missing)
//     // use existing docker compose volume for preventing download ollama image and models
//     .WithDataVolume("aspire-ollama");
//
// var ollamaChat = ollama.AddModel(AspireResources.OllamaChat, AgentFrameworkOptions.ChatModel);
// var ollamaEmbedding = ollama.AddModel(AspireResources.OllamaEmbedding, AgentFrameworkOptions.EmbeddingModel);

IResourceBuilder<ProjectResource> catalogsApi = builder
    .AddProject<Catalogs>(AspireApplicationResources.Api.CatalogsApi)
    .WithReplicas(builder.ExecutionContext.IsRunMode ? 1 : 2)
    // this service will be healthy in aspire dashboard after this endpoint is available, default is `/`
    .WithHttpHealthCheck("/health")
    // .WithReference(ollamaChat)
    // .WaitFor(ollamaChat)
    // .WithReference(ollamaEmbedding)
    // .WaitFor(ollamaEmbedding)
    .WithReference(catalogsPostgres)
    .WaitFor(catalogsPostgres)
    .WithReference(qdrant)
    .WaitFor(qdrant);

IResourceBuilder<ProjectResource> reviewsApi = builder
    .AddProject<Reviews>(AspireApplicationResources.Api.ReviewsApi)
    .WithReplicas(builder.ExecutionContext.IsRunMode ? 1 : 2)
    // this service will be healthy in aspire dashboard after this endpoint is available, default is `/`
    .WithHttpHealthCheck("/health")
    .WithReference(reviewsPostgres)
    .WaitFor(reviewsPostgres)
    // .WithReference(ollamaChat)
    // .WaitFor(ollamaChat)
    // .WithReference(ollamaEmbedding)
    // .WaitFor(ollamaEmbedding)
    .WaitFor(catalogsApi)
    .WithReference(catalogsApi); // for service discovery

IResourceBuilder<ProjectResource> ordersApi = builder
    .AddProject<Orders>(AspireApplicationResources.Api.OrdersApi)
    .WithReplicas(builder.ExecutionContext.IsRunMode ? 1 : 2)
    // this service will be healthy in aspire dashboard after this endpoint is available, default is `/`
    .WithHttpHealthCheck("/health")
    .WithReference(ordersPostgres)
    .WaitFor(ordersPostgres)
    .WithReference(redis)
    .WaitFor(redis)
    // .WithReference(ollamaChat)
    // .WaitFor(ollamaChat)
    // .WithReference(ollamaEmbedding)
    // .WaitFor(ollamaEmbedding)
    .WaitFor(catalogsApi)
    .WithReference(catalogsApi);

IResourceBuilder<ProjectResource> cartsApi = builder
    .AddProject<Carts>(AspireApplicationResources.Api.CartsApi)
    .WithReplicas(builder.ExecutionContext.IsRunMode ? 1 : 2)
    // this service will be healthy in aspire dashboard after this endpoint is available, default is `/`
    .WithHttpHealthCheck("/health")
    .WithReference(redis)
    .WaitFor(redis)
    // .WithReference(ollamaChat)
    // .WaitFor(ollamaChat)
    // .WithReference(ollamaEmbedding)
    // .WaitFor(ollamaEmbedding)
    .WaitFor(catalogsApi)
    .WithReference(catalogsApi);

IResourceBuilder<ProjectResource> mcpServerApi = builder
    .AddProject<McpServer>(AspireApplicationResources.Api.McpServerApi)
    .WithReplicas(builder.ExecutionContext.IsRunMode ? 1 : 2)
    // this service will be healthy in aspire dashboard after this endpoint is available, default is `/`
    .WithHttpHealthCheck("/health")
    // .WithReference(ollamaChat)
    // .WaitFor(ollamaChat)
    // .WithReference(ollamaEmbedding)
    // .WaitFor(ollamaEmbedding)
    .WaitFor(catalogsApi)
    .WithReference(catalogsApi);

IResourceBuilder<ProjectResource> recommendationApi = builder
    .AddProject<Recommendation>(AspireApplicationResources.Api.RecommendationApi)
    .WithReplicas(builder.ExecutionContext.IsRunMode ? 1 : 2)
    // this service will be healthy in aspire dashboard after this endpoint is available, default is `/`
    .WithHttpHealthCheck("/health")
    // .WithReference(ollamaChat)
    // .WaitFor(ollamaChat)
    // .WithReference(ollamaEmbedding)
    // .WaitFor(ollamaEmbedding)
    .WaitFor(catalogsApi)
    .WithReference(catalogsApi)
    .WaitFor(reviewsApi)
    .WithReference(reviewsApi)
    .WaitFor(mcpServerApi)
    .WithReference(mcpServerApi);

// https://github.com/CommunityToolkit/Aspire/tree/main/src/CommunityToolkit.Aspire.Hosting.McpInspector
// https://github.com/modelcontextprotocol/inspector
// https://modelcontextprotocol.io/docs/tools/inspector
builder
    .AddMcpInspector(AspireResources.McpInspector, options => { })
    .WithMcpServer(mcpServerApi)
    .ExcludeFromManifest();

await builder.Build().RunAsync();
