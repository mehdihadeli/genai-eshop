using System.Net.Http.Json;
using BuildingBlocks.Serialization;

namespace BuildingBlocks.Extensions;

public static class HttpResponseMessageExtensions
{
    /// <summary>
    /// Helper method to read and deserialize the JSON response content using the default serializer options.
    /// </summary>
    /// <typeparam name="T">The type to which the JSON content should be deserialized.</typeparam>
    /// <param name="response">The HTTP response to read the content from.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An instance of T deserialized from the JSON content.</returns>
    public static async Task<T?> ReadResponseContentAsync<T>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default
    )
    {
        return await response.Content.ReadFromJsonAsync<T>(
            options: SystemTextJsonSerializerOptions.DefaultSerializerOptions,
            cancellationToken: cancellationToken
        );
    }
}
