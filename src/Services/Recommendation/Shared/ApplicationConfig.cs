using GenAIEshop.Recommendation.Recommendations;

namespace GenAIEshop.Recommendation.Shared;

public static class ApplicationConfig
{
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.AddRecommendationsServices();

        return builder;
    }

    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapRecommendationsEndpoints();

        return endpoints;
    }
}
