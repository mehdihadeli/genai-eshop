using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.ProblemDetails;

public static class DependencyInjectionExtensions
{
    public static IHostApplicationBuilder AddCustomProblemDetails(this IHostApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

        return builder;
    }
}
