using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenAIEshop.Tests.Shared;

public sealed class CustomWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    private readonly Dictionary<string, string?> _configuration = [];
    private Action<IServiceCollection>? _configureServices;

    public CustomWebApplicationFactory<TEntryPoint> WithConfiguration(string key, string? value)
    {
        _configuration[key] = value;
        return this;
    }

    public CustomWebApplicationFactory<TEntryPoint> WithTestServices(Action<IServiceCollection> configureServices)
    {
        _configureServices += configureServices;
        return this;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        foreach (var pair in _configuration)
            builder.UseSetting(pair.Key, pair.Value);

        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(_configuration));
        builder.ConfigureTestServices(services => _configureServices?.Invoke(services));
    }
}
