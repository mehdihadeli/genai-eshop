using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GenAIEshop.Tests.Shared;

public static class WebApplicationFactoryExtensions
{
    public static HttpClient CreateTestClient<TEntryPoint>(
        this WebApplicationFactory<TEntryPoint> factory,
        string? bearerToken = null
    )
        where TEntryPoint : class
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        if (!string.IsNullOrWhiteSpace(bearerToken))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        return client;
    }
}
