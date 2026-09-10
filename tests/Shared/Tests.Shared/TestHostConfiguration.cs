using Xunit;

namespace GenAIEshop.Tests.Shared;

public static class TestHostConfiguration
{
    private static readonly string[] ServiceNames = ["CARTS", "CATALOGS", "ORDERS", "RECOMMENDATION", "REVIEWS"];

    public static void SetEnvironmentVariables(IReadOnlyDictionary<string, string?> values)
    {
        foreach (var pair in values)
            Environment.SetEnvironmentVariable(pair.Key, pair.Value);
    }

    public static HttpClient CreateServiceClient(string serviceName)
    {
        LoadDotEnv();

        var normalizedName = serviceName.Trim().ToUpperInvariant();
        if (!ServiceNames.Contains(normalizedName, StringComparer.Ordinal))
            throw new ArgumentException($"Unknown service name: {serviceName}", nameof(serviceName));

        var baseAddress = Environment.GetEnvironmentVariable($"GENAI_TEST_{normalizedName}_BASE_URL");
        if (!Uri.TryCreate(baseAddress, UriKind.Absolute, out var uri))
            Assert.Skip($"Set GENAI_TEST_{normalizedName}_BASE_URL in .env to run this test.");

        return new HttpClient { BaseAddress = uri };
    }

    public static void LoadDotEnv()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var envFile = Path.Combine(directory.FullName, ".env");
            if (File.Exists(envFile))
            {
                BuildingBlocks.Env.DotEnv.Load(envFile);
                return;
            }

            directory = directory.Parent;
        }
    }
}
