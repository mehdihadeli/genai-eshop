using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Extensions;

public static class HostEnvironmentExtensions
{
    public static bool IsTest(this IHostEnvironment env) => env.IsEnvironment(Types.Environments.Test);

    public static bool IsAspireRun(this IHostEnvironment env) => env.IsEnvironment(Types.Environments.Aspire);

    public static bool IsDependencyTest(this IHostEnvironment env) =>
        env.IsEnvironment(Types.Environments.DependencyTest);

    public static bool IsDocker(this IHostEnvironment env) => env.IsEnvironment(Types.Environments.Docker);

    public static bool IsBuild(this IHostEnvironment hostEnvironment)
    {
        // Check if the environment is "Build" or the entry assembly is "GetDocument.Insider"
        // to account for scenarios where app is launching via OpenAPI build-time generation
        // via the GetDocument.Insider tool.
        return hostEnvironment.IsEnvironment("Build")
            || Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
    }
}
