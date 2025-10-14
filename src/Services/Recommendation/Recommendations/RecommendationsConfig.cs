using GenAIEshop.Recommendation.Recommendations.Features.ComparingProducts;
using GenAIEshop.Recommendation.Recommendations.Features.GettingPersonalizedRecommendation;
using GenAIEshop.Recommendation.Recommendations.Features.GettingRecommendation;

namespace GenAIEshop.Recommendation.Recommendations;

public static class RecommendationsConfig
{
    public static IHostApplicationBuilder AddRecommendationsServices(this IHostApplicationBuilder builder)
    {
        return builder;
    }

    public static IEndpointRouteBuilder MapRecommendationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var recommendations = endpoints.NewVersionedApi("recommendations");
        var recommendationsV1 = recommendations
            .MapGroup("/api/v{version:apiVersion}/recommendations")
            .HasApiVersion(1.0);

        recommendationsV1.MapGetPersonalizedRecommendationsEndpoint();
        recommendationsV1.MapGetProductRecommendationsEndpoint();
        recommendationsV1.MapCompareProductsEndpoint();

        return endpoints;
    }
}
