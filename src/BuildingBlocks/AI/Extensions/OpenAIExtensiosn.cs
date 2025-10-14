using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OpenAI;

namespace BuildingBlocks.AI.Extensions;

// https://github.com/dotnet/ai-samples/blob/main/src/microsoft-extensions-ai/openai/OpenAIExamples/DependencyInjection.cs
// https://github.com/dotnet/ai-samples/blob/main/src/microsoft-extensions-ai/openai/OpenAIExamples/Middleware.cs
// https://github.com/dotnet/ai-samples/blob/main/src/microsoft-extensions-ai/openai/OpenAIExamples/ToolCalling.cs
// https://learn.microsoft.com/en-us/dotnet/ai/dotnet-ai-ecosystem
// https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/SemanticKernelMigration
public static class OpenAIExtensions
{
    private static readonly string DefaultExtensionsAISourceName = "Experimental.Microsoft.Extensions.AI";

    public static OpenAIApiClientBuilder AddOpenAIClient(
        this IHostApplicationBuilder builder,
        OpenAIApiClientSettings settings,
        object? serviceKey = null
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(settings.ApiKey, nameof(settings.ApiKey));

        // supports both `openai` and `openrouter` endpoints
        string openaiEndpoint = settings.Endpoint ?? "https://api.openai.com/v1";
        var openAiClient = new OpenAIClient(
            new ApiKeyCredential(settings.ApiKey),
            new OpenAIClientOptions { Endpoint = new Uri(openaiEndpoint) }
        );

        if (serviceKey is not null)
        {
            builder.Services.AddKeyedSingleton(serviceKey, openAiClient);
        }
        else
        {
            builder.Services.TryAddSingleton(openAiClient);
        }

        return new OpenAIApiClientBuilder(builder, serviceKey, settings.DisableTracing);
    }

    public static OpenAIApiClientBuilder AddOpenAIChatClient(
        this OpenAIApiClientBuilder builder,
        string deploymentName,
        string? model = null,
        Action<ChatClientBuilder>? chatClientBuilderConfigs = null,
        Action<OpenTelemetryChatClient>? configureOpenTelemetry = null,
        string? openTelemetrySourceName = null,
        bool useCache = false
    )
    {
        var chatClientBuilder = builder.HostBuilder.Services.AddChatClient(sp =>
        {
            var openAiClient = sp.GetRequiredKeyedService<OpenAIClient>(builder.ServiceKey);
            // `OpenAIChatClient` implements `IChatClient`
            // https://github.com/dotnet/extensions/blob/1eb963e2a9ec2a6501af082f058c3420ea5004a8/src/Libraries/Microsoft.Extensions.AI.OpenAI/OpenAIClientExtensions.cs#L110
            IChatClient chatClient = openAiClient.GetChatClient(deploymentName).AsIChatClient();

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

    public static ChatClientBuilder AddOpenAIChatClient(
        IServiceCollection services,
        OpenAIClient openAiClient,
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
            IChatClient chatClient = openAiClient.GetChatClient(deploymentName).AsIChatClient();

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

    public static ChatClientBuilder AddOpenAIChatClient(
        IServiceCollection services,
        string openAIClientName,
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
            var openAIClient = sp.GetRequiredKeyedService<OpenAIClient>(openAIClientName);
            // `OpenAIChatClient` implements `IChatClient`
            // https://github.com/dotnet/extensions/blob/1eb963e2a9ec2a6501af082f058c3420ea5004a8/src/Libraries/Microsoft.Extensions.AI.OpenAI/OpenAIClientExtensions.cs#L110
            IChatClient chatClient = openAIClient.GetChatClient(deploymentName).AsIChatClient();

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

    public static EmbeddingGeneratorBuilder<string, Embedding<float>> AddOpenAIEmbeddingGenerator(
        this OpenAIApiClientBuilder openAIApiClientBuilder,
        string deploymentName,
        string? model = null,
        Action<OpenTelemetryEmbeddingGenerator<string, Embedding<float>>>? configureOpenTelemetry = null,
        string? openTelemetrySourceName = null,
        bool useCache = false
    )
    {
        var embeddingGeneratorBuilder = openAIApiClientBuilder.HostBuilder.Services.AddEmbeddingGenerator(sp =>
        {
            var openAiClient = sp.GetRequiredKeyedService<OpenAIClient>(openAIApiClientBuilder.ServiceKey);
            // `OpenAIEmbeddingGenerator` is created by `AsIEmbeddingGenerator` and implements `IEmbeddingGenerator<string, Embedding<float>>`.
            // https://github.com/dotnet/extensions/blob/1eb963e2a9ec2a6501af082f058c3420ea5004a8/src/Libraries/Microsoft.Extensions.AI.OpenAI/OpenAIClientExtensions.cs#L110
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = openAiClient
                .GetEmbeddingClient(deploymentName)
                .AsIEmbeddingGenerator();

            return embeddingGenerator;
        });

        if (!openAIApiClientBuilder.DisableTracing)
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

    public static EmbeddingGeneratorBuilder<string, Embedding<float>> AddOpenAIEmbeddingGenerator(
        IServiceCollection services,
        OpenAIClient openAIClient,
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
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = openAIClient
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

    public static EmbeddingGeneratorBuilder<string, Embedding<float>> AddOpenAIEmbeddingGenerator(
        IServiceCollection services,
        string openAIClientName,
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
            var openAIClient = sp.GetRequiredKeyedService<OpenAIClient>(openAIClientName);
            // `OpenAIEmbeddingGenerator` is created by `AsIEmbeddingGenerator` and implements `IEmbeddingGenerator<string, Embedding<float>>`.
            // https://github.com/dotnet/extensions/blob/1eb963e2a9ec2a6501af082f058c3420ea5004a8/src/Libraries/Microsoft.Extensions.AI.OpenAI/OpenAIClientExtensions.cs#L110
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = openAIClient
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

public class OpenAIApiClientBuilder(IHostApplicationBuilder hostBuilder, object? serviceKey, bool disableTracing)
{
    /// <summary>
    /// The host application builder used to configure the application.
    /// </summary>
    public IHostApplicationBuilder HostBuilder { get; } = hostBuilder;

    /// <summary>
    /// Gets the service key used to register the <see cref="OpenAIClient"/> service, if any.
    /// </summary>
    public object? ServiceKey { get; } = serviceKey;

    /// <summary>
    /// Gets a flag indicating whether tracing should be disabled.
    /// </summary>
    public bool DisableTracing { get; } = disableTracing;
}

public class OpenAIApiClientSettings
{
    public string? Endpoint { get; set; }
    public required string ApiKey { get; set; } = default!;
    public bool DisableTracing { get; set; } = false;
}
