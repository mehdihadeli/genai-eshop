using System.Net.Http.Json;
using System.Text.Json;
using GenAIEshop.Tests.Shared;
using McpServer.Shared.Clients;
using McpServer.Shared.Contracts;
using McpServer.Shared.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GenAIEshop.McpServer.IntegrationTests;

public sealed class McpServerIntegrationTests : IAsyncLifetime
{
    private CustomWebApplicationFactory<Program> factory = null!;
    private HttpClient client = null!;

    public ValueTask InitializeAsync()
    {
        factory = new CustomWebApplicationFactory<Program>().WithTestServices(services =>
        {
            services.RemoveAll<ICatalogServiceClient>();
            services.AddSingleton<ICatalogServiceClient, FakeCatalogServiceClient>();
        });
        client = factory.CreateTestClient();
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        client.Dispose();
        await factory.DisposeAsync();
    }

    [Fact]
    public async Task Mcp_endpoint_initializes_and_lists_product_tools()
    {
        await SendMcpRequestAsync(
            "initialize",
            1,
            new
            {
                protocolVersion = "2025-06-18",
                capabilities = new { },
                clientInfo = new { name = "integration-tests", version = "1.0.0" },
            }
        );

        using var response = await SendMcpRequestAsync("tools/list", 2);
        var tools = response.RootElement.GetProperty("result").GetProperty("tools");

        tools
            .EnumerateArray()
            .Select(tool => tool.GetProperty("name").GetString())
            .ShouldContain("HybridSearchProducts");
    }

    [Fact]
    public async Task Mcp_tool_call_binds_arguments_and_returns_catalog_data()
    {
        await SendMcpRequestAsync(
            "initialize",
            1,
            new
            {
                protocolVersion = "2025-06-18",
                capabilities = new { },
                clientInfo = new { name = "integration-tests", version = "1.0.0" },
            }
        );

        using var response = await SendMcpRequestAsync(
            "tools/call",
            2,
            new { name = "HybridSearchProducts", arguments = new { query = "laptop", keywords = new[] { "portable" } } }
        );

        response.RootElement.GetProperty("result").GetProperty("content").GetArrayLength().ShouldBeGreaterThan(0);
        response.RootElement.ToString().ShouldContain("Test laptop");
    }

    private async Task<JsonDocument> SendMcpRequestAsync(string method, int id, object? parameters = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/mcp");
        request.Headers.Add("Accept", "application/json, text/event-stream");
        request.Headers.Add("MCP-Protocol-Version", "2025-06-18");
        request.Content = JsonContent.Create(
            new
            {
                jsonrpc = "2.0",
                id,
                method,
                @params = parameters,
            }
        );

        using var response = await client.SendAsync(request);
        response.IsSuccessStatusCode.ShouldBeTrue(await response.Content.ReadAsStringAsync());

        var body = await response.Content.ReadAsStringAsync();
        var dataLine = body.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(line => line.StartsWith("data:", StringComparison.Ordinal));
        var json = dataLine is null ? body : dataLine["data:".Length..].Trim();

        return JsonDocument.Parse(json);
    }

    private sealed class FakeCatalogServiceClient : ICatalogServiceClient
    {
        public Task<ProductDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default) =>
            Task.FromResult<ProductDto?>(null);

        public Task<IReadOnlyList<ProductDto>> GetProductsByIdAsync(
            IEnumerable<Guid> productIds,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<IReadOnlyList<ProductDto>>([]);

        public Task<SearchProductsResponse> SearchProductsAsync(
            string searchTerm,
            double threshold,
            SearchType searchType = SearchType.Regular,
            string[]? keywords = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default
        ) =>
            Task.FromResult(
                new SearchProductsResponse(
                    [new ProductDto(Guid.NewGuid(), "Test laptop", "Integration test product", 999m, true, "")],
                    "Test result",
                    searchType,
                    1,
                    1,
                    1
                )
            );
    }
}
