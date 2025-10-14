using System.Reflection;
using Asp.Versioning;
using BuildingBlocks.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace BuildingBlocks.AI.MCP;

// ref: https://devblogs.microsoft.com/dotnet/build-a-model-context-protocol-mcp-server-in-csharp/
// https://learn.microsoft.com/en-us/dotnet/ai/get-started-mcp
// https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/adding-mcp-plugins
// https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/adding-openapi-plugins
// https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/?pivots=programming-language-csharp
// https://devblogs.microsoft.com/semantic-kernel/integrating-model-context-protocol-tools-with-semantic-kernel-a-step-by-step-guide/
// https://devblogs.microsoft.com/semantic-kernel/building-a-model-context-protocol-server-with-semantic-kernel/
// https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/agent-types/chat-completion-agent?pivots=programming-language-csharp
// https://github.com/modelcontextprotocol/csharp-sdk
// https://github.com/modelcontextprotocol/csharp-sdk/blob/main/src/ModelContextProtocol.AspNetCore/README.md
// https://modelcontextprotocol.io/specification/2025-06-18/basic/transports
public static class DependencyInjectionExtensions
{
    private const string ActivitySourceName = "Experimental.ModelContextProtocol";

    /// <summary>
    /// Registers and configures an MCP server in the DI container.
    /// </summary>
    /// <param name="builder">The application builder used to access services and configuration.</param>
    /// <param name="name">Logical server name reported in MCP ServerInfo.</param>
    /// <param name="toolAssembly">Assembly to scan for MCP tools, resources, and prompts. Defaults to calling assembly.</param>
    /// <param name="version">API version to expose. Defaults to 1.0.</param>
    /// <returns>An <see cref="IMcpServerBuilder"/> for further server customization.</returns>
    /// <remarks>
    /// - Configures HTTP transport in stateless mode.
    /// - Registers tools, resources, and prompts discovered in the specified assembly.
    /// - Adds OpenTelemetry meters and tracing sources for diagnostics.
    /// </remarks>
    public static IMcpServerBuilder AddCustomMcpServer(
        this IHostApplicationBuilder builder,
        string name = "McpServer",
        Assembly? toolAssembly = null,
        ApiVersion? version = null
    )
    {
        var mcpServerBuilder = builder
            .Services.AddMcpServer(o =>
            {
                Implementation info =
                    new() { Name = name, Version = version?.ToString() ?? new ApiVersion(1, 0).ToString() };

                o.ServerInfo = info;
            })
            .WithHttpTransport(o => o.Stateless = true)
            .WithToolsFromAssembly(toolAssembly: toolAssembly ?? Assembly.GetCallingAssembly())
            .WithResourcesFromAssembly(resourceAssembly: toolAssembly ?? Assembly.GetCallingAssembly())
            .WithPromptsFromAssembly(promptAssembly: toolAssembly ?? Assembly.GetCallingAssembly());

        // https://github.com/modelcontextprotocol/csharp-sdk/blob/7c66faaef69669deddc58d6b256f6e0ef93a31c4/src/ModelContextProtocol.Core/Diagnostics.cs#L11
        builder
            .Services.AddOpenTelemetry()
            .WithMetrics(m => m.AddMeter(ActivitySourceName))
            .WithTracing(t => t.AddSource(ActivitySourceName));

        return mcpServerBuilder;
    }

    /// <summary>
    /// Registers an MCP client that connects to a remote MCP server over HTTP(S) using Streamable HTTP transport.
    /// </summary>
    /// <param name="builder">The application builder used to access services and configuration.</param>
    /// <param name="mcpClientName">Keyed service name used to resolve the MCP client from DI.</param>
    /// <param name="mcpServerHostUrl">Absolute base URL of the MCP server (for example, "https://host:port/").</param>
    /// <param name="relativePath">mcp relative path</param>
    /// <param name="version">Client-reported version in MCP ClientInfo. Defaults to 1.0.</param>
    /// <remarks>
    /// - Uses HttpClientTransport with TransportMode = StreamableHttp.
    /// - The effective endpoint is new Uri(new Uri(mcpServerUrl), relativePath).
    /// - Reuses a named HttpClient, allowing configuration of authentication headers, proxies, retries, and policies via IHttpClientFactory.
    /// - Throws ArgumentException if mcpServerUrl is not an absolute URI.
    /// - Consider AddSseMcpClient for SSE-based servers and AddStdioMcpClient for process/STDIO servers.
    /// </remarks>
    public static void AddHttpMcpClient(
        this IHostApplicationBuilder builder,
        string mcpClientName,
        string mcpServerHostUrl,
        string relativePath = "mcp",
        ApiVersion? version = null
    )
    {
        var services = builder.Services;

        ArgumentException.ThrowIfNullOrWhiteSpace(mcpClientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(mcpServerHostUrl);

        services.AddKeyedSingleton<McpClient>(
            mcpClientName,
            (sp, key) =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
       
                // because McpClient create its own HttpClient manually and don't use HttpClientFactory and our httpclient service discovery doesn't apply (through AddServiceDiscovery()), we should discover endpoint manually
                var hostEndpoint = sp.GetEndpointAddress(mcpServerHostUrl);
                ArgumentException.ThrowIfNullOrEmpty(hostEndpoint);
                
                var endpoint = new Uri(new Uri(hostEndpoint), relativePath);

                McpClientOptions mcpClientOptions =
                    new()
                    {
                        ClientInfo = new Implementation
                        {
                            Name = mcpClientName,
                            Version = version?.ToString() ?? new ApiVersion(1, 0).ToString(),
                        },
                    };

                // https://modelcontextprotocol.io/specification/2025-06-18/basic/transports#streamable-http
                HttpClientTransportOptions httpClientTransportOptions =
                    new()
                    {
                        Name = $"{mcpClientName}HttpClientTransport",
                        TransportMode = HttpTransportMode.StreamableHttp,
                        Endpoint = endpoint,
                    };

                HttpClientTransport httpClientTransport = new(httpClientTransportOptions, loggerFactory);

                McpClient mcpClient = McpClient
                    .CreateAsync(httpClientTransport, mcpClientOptions, loggerFactory)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                return mcpClient;
            }
        );
    }
    
     /// <summary>
    /// Registers an MCP client that connects to a remote MCP server over HTTP(S) using Server-Sent Events (SSE).
    /// </summary>
    /// <param name="builder">The application builder used to access services and configuration.</param>
    /// <param name="mcpClientName">Keyed service name used to resolve the MCP client from DI.</param>
    /// <param name="mcpServerUrl">Absolute base URL of the MCP server (for example, "https://host:port/").</param>
    /// <param name="version">Client-reported version in MCP ClientInfo. Defaults to 1.0.</param>
    /// <remarks>
    /// - Uses HttpClientTransport with TransportMode = Sse.
    /// - Prefer when the server exposes an SSE endpoint for event-driven streaming from server to client.
    /// - Throws ArgumentException if mcpServerUrl is not an absolute URI.
    /// </remarks>
    public static void AddHttpSseMcpClient(
        this IHostApplicationBuilder builder,
        string mcpClientName,
        string mcpServerUrl,
        ApiVersion? version = null
    )
    {
        var services = builder.Services;

        ArgumentException.ThrowIfNullOrWhiteSpace(mcpClientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(mcpServerUrl);

        services.AddKeyedSingleton<McpClient>(
            mcpClientName,
            (sp, key) =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                var resolver = sp.GetRequiredService<ServiceEndpointResolver>();
                var endpoints = resolver
                    .GetEndpointsAsync(mcpServerUrl, CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                var endpoint = endpoints.Endpoints.FirstOrDefault()?.EndPoint.ToString();
                ArgumentException.ThrowIfNullOrEmpty(endpoint);

                McpClientOptions mcpClientOptions =
                    new()
                    {
                        ClientInfo = new Implementation
                        {
                            Name = mcpClientName,
                            Version = version?.ToString() ?? new ApiVersion(1, 0).ToString(),
                        },
                    };

                // https://modelcontextprotocol.io/specification/2025-06-18/basic/transports#streamable-http
                HttpClientTransportOptions httpClientTransportOptions =
                    new()
                    {
                        Name = $"{mcpClientName}SseClientTransport",
                        TransportMode = HttpTransportMode.Sse,
                        Endpoint = new Uri(endpoint),
                    };

                HttpClientTransport sseTransport = new(httpClientTransportOptions, loggerFactory);

                McpClient mcpClient = McpClient
                    .CreateAsync(sseTransport, mcpClientOptions, loggerFactory)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                return mcpClient;
            }
        );
    }

    /// <summary>
    /// Registers an MCP client that launches and connects to an MCP server over json/rpc STDIO.
    /// </summary>
    /// <param name="builder">The application builder used to access services and configuration.</param>
    /// <param name="mcpClientName">Keyed service name for resolving the <see cref="IMcpClient"/>.</param>
    /// <param name="command">Executable or shell command to start the MCP server process (e.g., "npx", "dotnet").</param>
    /// <param name="arguments">Optional command-line arguments passed to <paramref name="command"/>.</param>
    /// <param name="workingDirectory">Optional working directory for the launched process.</param>
    /// <param name="version">Client-reported version in MCP ClientInfo. Defaults to 1.0.</param>
    /// <remarks>
    /// - Uses <see cref="StdioClientTransport"/> to spawn a process and communicate via standard input/output.
    /// - Registers a keyed singleton <see cref="McpClient"/> under <paramref name="mcpClientName"/>.
    /// - Suitable for MCP servers distributed as executables, .NET tools, or package runners.
    /// </remarks>
    public static void AddStdioMcpClient(
        this IHostApplicationBuilder builder,
        string mcpClientName,
        string command,
        string[]? arguments = null,
        string? workingDirectory = null,
        ApiVersion? version = null
    )
    {
        var services = builder.Services;

        ArgumentException.ThrowIfNullOrWhiteSpace(mcpClientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(command);

        services.AddKeyedSingleton<McpClient>(
            mcpClientName,
            (sp, key) =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                McpClientOptions mcpClientOptions =
                    new()
                    {
                        ClientInfo = new Implementation
                        {
                            Name = mcpClientName,
                            Version = version?.ToString() ?? new ApiVersion(1, 0).ToString(),
                        },
                    };

                // https://modelcontextprotocol.io/specification/2025-06-18/basic/transports#stdio
                StdioClientTransportOptions stdioOptions =
                    new()
                    {
                        Name = $"{mcpClientName}StdioClient",
                        Command = command,
                        Arguments = arguments ?? [],
                        WorkingDirectory = workingDirectory,
                    };

                // use JSON-RPC 2.0 over stdio
                StdioClientTransport stdioTransport = new(stdioOptions, loggerFactory);

                McpClient mcpClient = McpClient
                    .CreateAsync(stdioTransport, mcpClientOptions, loggerFactory)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                return mcpClient;
            }
        );
    }
}
