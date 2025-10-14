using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OllamaSharp;

namespace BuildingBlocks.AI.Extensions;

// https://github.com/dotnet/ai-samples/blob/main/src/microsoft-extensions-ai/ollama/OllamaExamples/DependencyInjection.cs
// https://github.com/dotnet/ai-samples/blob/main/src/microsoft-extensions-ai/ollama/OllamaExamples/Middleware.cs
// https://github.com/dotnet/ai-samples/blob/main/src/microsoft-extensions-ai/ollama/OllamaExamples/ToolCalling.cs
// https://learn.microsoft.com/en-us/dotnet/ai/dotnet-ai-ecosystem
// https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/SemanticKernelMigration
public static class OllamaExtensions
{
    private static readonly string DefaultExtensionsAISourceName = "Experimental.Microsoft.Extensions.AI";

    public static AspireOllamaApiClientBuilder AddOllamaChatClient(
        this AspireOllamaApiClientBuilder builder,
        Action<ChatClientBuilder>? chatClientBuilderConfigs = null,
        Action<OpenTelemetryChatClient>? configureOpenTelemetry = null,
        string? openTelemetrySourceName = null,
        bool useCache = false
    )
    {
        var chatClientBuilder = builder.HostBuilder.Services.AddChatClient(sp =>
        {
            var ollamaApiClient = sp.GetRequiredKeyedService<IOllamaApiClient>(builder.ServiceKey);
            // the implementor of `IOllamaApiClient` also implements `IChatClient` like `OllamaApiClient`
            IChatClient chatClient = (IChatClient)ollamaApiClient;

            return chatClient;
        });

        if (!builder.DisableTracing)
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

        return builder;
    }

    public static ChatClientBuilder AddOllamaChatClient(
        IServiceCollection services,
        IOllamaApiClient ollamaApiClient,
        bool disableTracing = false,
        Action<OpenTelemetryChatClient>? configureOpenTelemetry = null,
        string? openTelemetrySourceName = null,
        bool useCache = false
    )
    {
        var chatClientBuilder = services.AddChatClient(sp =>
        {
            // the implementor of `IOllamaApiClient` also implements `IChatClient` like `OllamaApiClient`
            IChatClient chatClient = (IChatClient)ollamaApiClient;

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

    public static ChatClientBuilder AddOllamaChatClient(
        IServiceCollection services,
        string ollamaApiClientName,
        bool disableTracing = false,
        Action<OpenTelemetryChatClient>? configureOpenTelemetry = null,
        string? openTelemetrySourceName = null,
        bool useCache = false
    )
    {
        var chatClientBuilder = services.AddChatClient(sp =>
        {
            var ollamaApiClient = sp.GetRequiredKeyedService<IOllamaApiClient>(ollamaApiClientName);
            // the implementor of `IOllamaApiClient` also implements `IChatClient` like `OllamaApiClient`
            IChatClient chatClient = (IChatClient)ollamaApiClient;

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

    public static EmbeddingGeneratorBuilder<string, Embedding<float>> AddOllamaEmbeddingGenerator(
        this AspireOllamaApiClientBuilder aspireOllamaApiClientBuilder,
        Action<OpenTelemetryEmbeddingGenerator<string, Embedding<float>>>? configureOpenTelemetry = null,
        string? openTelemetrySourceName = null,
        bool useCache = false
    )
    {
        var embeddingGeneratorBuilder = aspireOllamaApiClientBuilder.HostBuilder.Services.AddEmbeddingGenerator(sp =>
        {
            var ollamaApiClient = sp.GetRequiredKeyedService<IOllamaApiClient>(aspireOllamaApiClientBuilder.ServiceKey);

            // the implementor of `IOllamaApiClient` also implements `IEmbeddingGenerator<string, Embedding<float>` like `OllamaApiClient`
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
                (IEmbeddingGenerator<string, Embedding<float>>)ollamaApiClient;

            return embeddingGenerator;
        });

        if (!aspireOllamaApiClientBuilder.DisableTracing)
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

    public static EmbeddingGeneratorBuilder<string, Embedding<float>> AddOllamaEmbeddingGenerator(
        IServiceCollection services,
        IOllamaApiClient ollamaApiClient,
        bool disableTracing = false,
        Action<OpenTelemetryEmbeddingGenerator<string, Embedding<float>>>? configureOpenTelemetry = null,
        string? openTelemetrySourceName = null,
        bool useCache = false
    )
    {
        var embeddingGeneratorBuilder = services.AddEmbeddingGenerator(sp =>
        {
            // the implementor of `IOllamaApiClient` also implements `IEmbeddingGenerator<string, Embedding<float>` like `OllamaApiClient`
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
                (IEmbeddingGenerator<string, Embedding<float>>)ollamaApiClient;

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

    public static EmbeddingGeneratorBuilder<string, Embedding<float>> AddOllamaEmbeddingGenerator(
        IServiceCollection services,
        string ollamaApiClientName,
        bool disableTracing = false,
        Action<OpenTelemetryEmbeddingGenerator<string, Embedding<float>>>? configureOpenTelemetry = null,
        string? openTelemetrySourceName = null,
        bool useCache = false
    )
    {
        var embeddingGeneratorBuilder = services.AddEmbeddingGenerator(sp =>
        {
            var ollamaApiClient = sp.GetRequiredKeyedService<IOllamaApiClient>(ollamaApiClientName);
            // the implementor of `IOllamaApiClient` also implements `IEmbeddingGenerator<string, Embedding<float>` like `OllamaApiClient`
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
                (IEmbeddingGenerator<string, Embedding<float>>)ollamaApiClient;

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
