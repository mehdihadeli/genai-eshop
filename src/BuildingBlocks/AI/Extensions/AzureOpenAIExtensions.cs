using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.AI.Extensions;

// https://github.com/dotnet/ai-samples/blob/main/src/microsoft-extensions-ai/azure-openai/AzureOpenAIExamples/ToolCalling.cs
// https://github.com/dotnet/ai-samples/blob/main/src/microsoft-extensions-ai/azure-openai/AzureOpenAIExamples/DependencyInjection.cs
// https://github.com/dotnet/ai-samples/blob/main/src/microsoft-extensions-ai/azure-openai/AzureOpenAIExamples/Middleware.cs
// https://learn.microsoft.com/en-us/dotnet/ai/dotnet-ai-ecosystem
// https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/SemanticKernelMigration
public static class AzureOpenAIExtensions
{
    private static readonly string DefaultExtensionsAISourceName = "Experimental.Microsoft.Extensions.AI";

    public static AzureOpenAIApiClientBuilder AddAzureOpenAIClient(
        this IHostApplicationBuilder builder,
        AzureOpenAIApiClientSettings settings,
        object? serviceKey = null
    )
    {
        // Use the main endpoint if provided, otherwise fall back to chat endpoint
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoint, nameof(settings.Endpoint));
        var endpoint = new Uri(settings.Endpoint);

        ArgumentException.ThrowIfNullOrEmpty(settings.ApiKey, nameof(settings.ApiKey));
        var credential = new ApiKeyCredential(settings.ApiKey);

        var azureOpenAIClient = new AzureOpenAIClient(endpoint: endpoint, credential: credential);
        if (serviceKey is not null)
        {
            builder.Services.AddKeyedSingleton(serviceKey, azureOpenAIClient);
        }
        else
        {
            builder.Services.TryAddSingleton(azureOpenAIClient);
        }

        return new AzureOpenAIApiClientBuilder(builder, serviceKey, settings.DisableTracing);
    }

    public static AzureOpenAIApiClientBuilder AddAzureOpenAIChatClient(
        this AzureOpenAIApiClientBuilder azureOpenAiApiClientBuilder,
        string deploymentName,
        string? model = null,
        Action<ChatClientBuilder>? chatClientBuilderConfigs = null,
        Action<OpenTelemetryChatClient>? configureOpenTelemetry = null,
        string? openTelemetrySourceName = null,
        bool useCache = false
    )
    {
        var chatClientBuilder = azureOpenAiApiClientBuilder.HostBuilder.Services.AddChatClient(sp =>
        {
            var azureOpenAiClient = sp.GetRequiredKeyedService<AzureOpenAIClient>(
                azureOpenAiApiClientBuilder.ServiceKey
            );
            // `OpenAIChatClient` implements `IChatClient`
            // https://github.com/dotnet/extensions/blob/1eb963e2a9ec2a6501af082f058c3420ea5004a8/src/Libraries/Microsoft.Extensions.AI.OpenAI/OpenAIClientExtensions.cs#L110
            IChatClient chatClient = azureOpenAiClient.GetChatClient(deploymentName).AsIChatClient();

            return chatClient;
        });

        if (!azureOpenAiApiClientBuilder.DisableTracing)
        {
            chatClientBuilder.UseOpenTelemetry(
                sourceName: openTelemetrySourceName ?? DefaultExtensionsAISourceName,
                configure: configureOpenTelemetry
            );
        }

        if (useCache)
        {
            chatClientBuilder.UseDistributedCache();
        }

        // because we don't have a kernel instead of `UseKernelFunctionInvocation` we should use `UseFunctionInvocation`
        chatClientBuilder = chatClientBuilder.UseLogging().UseFunctionInvocation();

        chatClientBuilderConfigs?.Invoke(chatClientBuilder);

        return azureOpenAiApiClientBuilder;
    }

    public static ChatClientBuilder AddAzureOpenAIChatClient(
        IServiceCollection services,
        AzureOpenAIClient azureOpenAiClient,
        string deploymentName,
        string? model = null,
        bool disableTracing = false,
        Action<OpenTelemetryChatClient>? configureOpenTelemetry = null,
        string? openTelemetrySourceName = null,
        bool useCache = false
    )
    {
        var chatClientBuilder = services.AddChatClient(sp =>
        {
            // `OpenAIChatClient` implements `IChatClient`
            // https://github.com/dotnet/extensions/blob/1eb963e2a9ec2a6501af082f058c3420ea5004a8/src/Libraries/Microsoft.Extensions.AI.OpenAI/OpenAIClientExtensions.cs#L110
            IChatClient chatClient = azureOpenAiClient.GetChatClient(deploymentName).AsIChatClient();

            return chatClient;
        });

        if (!disableTracing)
        {
            chatClientBuilder.UseOpenTelemetry(
                sourceName: openTelemetrySourceName ?? DefaultExtensionsAISourceName,
                configure: configureOpenTelemetry
            );
        }

        if (useCache)
        {
            chatClientBuilder.UseDistributedCache();
        }

        return chatClientBuilder.UseLogging().UseFunctionInvocation();
    }

    public static ChatClientBuilder AddAzureOpenAIChatClient(
        IServiceCollection services,
        string azureOpenAIClientName,
        string deploymentName,
        string? model = null,
        bool disableTracing = false,
        Action<OpenTelemetryChatClient>? configureOpenTelemetry = null,
        string? openTelemetrySourceName = null,
        bool useCache = false
    )
    {
        var chatClientBuilder = services.AddChatClient(sp =>
        {
            var azureOpenAIClient = sp.GetRequiredKeyedService<AzureOpenAIClient>(azureOpenAIClientName);
            // the implementor of `IOllamaApiClient` also implements `IChatClient` like `OllamaApiClient`
            // `OpenAIChatClient` implements `IChatClient`
            // https://github.com/dotnet/extensions/blob/1eb963e2a9ec2a6501af082f058c3420ea5004a8/src/Libraries/Microsoft.Extensions.AI.OpenAI/OpenAIClientExtensions.cs#L110
            IChatClient chatClient = azureOpenAIClient.GetChatClient(deploymentName).AsIChatClient();

            return chatClient;
        });

        if (!disableTracing)
        {
            chatClientBuilder.UseOpenTelemetry(
                sourceName: openTelemetrySourceName ?? DefaultExtensionsAISourceName,
                configure: configureOpenTelemetry
            );
        }

        if (useCache)
        {
            chatClientBuilder.UseDistributedCache();
        }

        return chatClientBuilder.UseLogging().UseFunctionInvocation();
    }

    public static EmbeddingGeneratorBuilder<string, Embedding<float>> AddAzureOpenAIEmbeddingGenerator(
        this AzureOpenAIApiClientBuilder azureOpenAIApiClientBuilder,
        string deploymentName,
        string? model = null,
        Action<OpenTelemetryEmbeddingGenerator<string, Embedding<float>>>? configureOpenTelemetry = null,
        string? openTelemetrySourceName = null,
        bool useCache = false
    )
    {
        var embeddingGeneratorBuilder = azureOpenAIApiClientBuilder.HostBuilder.Services.AddEmbeddingGenerator(sp =>
        {
            var azureOpenAiClient = sp.GetRequiredKeyedService<AzureOpenAIClient>(
                azureOpenAIApiClientBuilder.ServiceKey
            );
            // `OpenAIEmbeddingGenerator` is created by `AsIEmbeddingGenerator` and implements `IEmbeddingGenerator<string, Embedding<float>>`.
            // https://github.com/dotnet/extensions/blob/1eb963e2a9ec2a6501af082f058c3420ea5004a8/src/Libraries/Microsoft.Extensions.AI.OpenAI/OpenAIClientExtensions.cs#L110
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = azureOpenAiClient
                .GetEmbeddingClient(deploymentName)
                .AsIEmbeddingGenerator();

            return embeddingGenerator;
        });

        if (!azureOpenAIApiClientBuilder.DisableTracing)
        {
            embeddingGeneratorBuilder.UseOpenTelemetry(
                sourceName: openTelemetrySourceName ?? DefaultExtensionsAISourceName,
                configure: configureOpenTelemetry
            );
        }

        if (useCache)
        {
            embeddingGeneratorBuilder.UseDistributedCache();
        }

        return embeddingGeneratorBuilder.UseLogging();
    }

    public static EmbeddingGeneratorBuilder<string, Embedding<float>> AddAzureOpenAIEmbeddingGenerator(
        IServiceCollection services,
        AzureOpenAIClient azureOpenAIClient,
        string deploymentName,
        string? model = null,
        bool disableTracing = false,
        Action<OpenTelemetryEmbeddingGenerator<string, Embedding<float>>>? configureOpenTelemetry = null,
        string? openTelemetrySourceName = null,
        bool useCache = false
    )
    {
        var embeddingGeneratorBuilder = services.AddEmbeddingGenerator(sp =>
        {
            // `OpenAIEmbeddingGenerator` is created by `AsIEmbeddingGenerator` and implements `IEmbeddingGenerator<string, Embedding<float>>`.
            // https://github.com/dotnet/extensions/blob/1eb963e2a9ec2a6501af082f058c3420ea5004a8/src/Libraries/Microsoft.Extensions.AI.OpenAI/OpenAIClientExtensions.cs#L110
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = azureOpenAIClient
                .GetEmbeddingClient(deploymentName)
                .AsIEmbeddingGenerator();

            return embeddingGenerator;
        });

        if (!disableTracing)
        {
            embeddingGeneratorBuilder.UseOpenTelemetry(
                sourceName: openTelemetrySourceName ?? DefaultExtensionsAISourceName,
                configure: configureOpenTelemetry
            );
        }

        if (useCache)
        {
            embeddingGeneratorBuilder.UseDistributedCache();
        }

        return embeddingGeneratorBuilder.UseLogging();
    }

    public static EmbeddingGeneratorBuilder<string, Embedding<float>> AddAzureOpenAIEmbeddingGenerator(
        IServiceCollection services,
        string azureOpenAIClientName,
        string deploymentName,
        string? model = null,
        bool disableTracing = false,
        Action<OpenTelemetryEmbeddingGenerator<string, Embedding<float>>>? configureOpenTelemetry = null,
        string? openTelemetrySourceName = null,
        bool useCache = false
    )
    {
        var embeddingGeneratorBuilder = services.AddEmbeddingGenerator(sp =>
        {
            var azureOpenAIClient = sp.GetRequiredKeyedService<AzureOpenAIClient>(azureOpenAIClientName);
            // `OpenAIEmbeddingGenerator` is created by `AsIEmbeddingGenerator` and implements `IEmbeddingGenerator<string, Embedding<float>>`.
            // https://github.com/dotnet/extensions/blob/1eb963e2a9ec2a6501af082f058c3420ea5004a8/src/Libraries/Microsoft.Extensions.AI.OpenAI/OpenAIClientExtensions.cs#L110
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = azureOpenAIClient
                .GetEmbeddingClient(deploymentName)
                .AsIEmbeddingGenerator();

            return embeddingGenerator;
        });

        if (!disableTracing)
        {
            embeddingGeneratorBuilder.UseOpenTelemetry(
                sourceName: openTelemetrySourceName ?? DefaultExtensionsAISourceName,
                configure: configureOpenTelemetry
            );
        }

        if (useCache)
        {
            embeddingGeneratorBuilder.UseDistributedCache();
        }

        return embeddingGeneratorBuilder.UseLogging();
    }
}

public class AzureOpenAIApiClientBuilder(IHostApplicationBuilder hostBuilder, object? serviceKey, bool disableTracing)
{
    /// <summary>
    /// The host application builder used to configure the application.
    /// </summary>
    public IHostApplicationBuilder HostBuilder { get; } = hostBuilder;

    /// <summary>
    /// Gets the service key used to register the <see cref="AzureOpenAIClient"/> service, if any.
    /// </summary>
    public object? ServiceKey { get; } = serviceKey;

    /// <summary>
    /// Gets a flag indicating whether tracing should be disabled.
    /// </summary>
    public bool DisableTracing { get; } = disableTracing;
}

public class AzureOpenAIApiClientSettings
{
    public required string Endpoint { get; set; } = default!;
    public required string ApiKey { get; set; } = default!;
    public bool DisableTracing { get; set; }
}
