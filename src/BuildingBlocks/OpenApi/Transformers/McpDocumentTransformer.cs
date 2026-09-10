using System.Net.Http;
using System.Net.Mime;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using ModelContextProtocol.Protocol;

namespace BuildingBlocks.OpenApi.Transformers;

public sealed class McpDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var pathItem = new OpenApiPathItem();

        // OpenAPI.NET v2 uses IOpenApiSchema, JsonSchemaType, HttpMethod, and typed references.
        // https://github.com/microsoft/OpenAPI.NET/blob/main/docs/upgrade-guide.md
        // Ensure components and required schemas exist
        document.Components ??= new OpenApiComponents();
        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();

        if (!document.Components.Schemas.ContainsKey(nameof(JsonRpcRequest)))
        {
            document.Components.Schemas[nameof(JsonRpcRequest)] = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Description = "JSON-RPC request payload",
                AdditionalPropertiesAllowed = true,
            };
        }

        if (!document.Components.Schemas.ContainsKey(nameof(JsonRpcResponse)))
        {
            document.Components.Schemas[nameof(JsonRpcResponse)] = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Description = "JSON-RPC response payload",
                AdditionalPropertiesAllowed = true,
            };
        }

        pathItem.AddOperation(
            HttpMethod.Post,
            new OpenApiOperation
            {
                Summary = "Get MCP Components",
                Extensions = new Dictionary<string, IOpenApiExtension>
                {
                    ["x-ms-agentic-protocol"] = new JsonNodeExtension(JsonValue.Create("mcp-streamable-1.0")),
                },
                OperationId = "InvokeMCP",
                Responses = new()
                {
                    [$"{StatusCodes.Status200OK}"] = new OpenApiResponse
                    {
                        Description = "Success",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            [MediaTypeNames.Application.Json] = new()
                            {
                                Schema = new OpenApiSchemaReference(nameof(JsonRpcResponse)),
                            },
                        },
                    },
                    [$"{StatusCodes.Status400BadRequest}"] = new OpenApiResponse { Description = "Bad Request" },
                    [$"{StatusCodes.Status406NotAcceptable}"] = new OpenApiResponse { Description = "Not Acceptable" },
                },
                RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        [MediaTypeNames.Application.Json] = new()
                        {
                            Schema = new OpenApiSchemaReference(nameof(JsonRpcRequest)),
                        },
                    },
                },
            }
        );

        document.Paths.Add("/mcp", pathItem);

        return Task.CompletedTask;
    }
}
