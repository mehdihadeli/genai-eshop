using GenAIEshop.Reviews.ProductReviews.Features.AnalyzeProductReviews;
using GenAIEshop.Reviews.ProductReviews.Features.AnalyzeReviewTrends;
using GenAIEshop.Reviews.ProductReviews.Features.CompareProductsByReviews;
using GenAIEshop.Reviews.ProductReviews.Features.CreatingReview;
using GenAIEshop.Reviews.ProductReviews.Features.GetProductQualitySummary;
using GenAIEshop.Reviews.ProductReviews.Features.GettingReviewsByProduct;
using GenAIEshop.Reviews.ProductReviews.Models;
using Humanizer;

namespace GenAIEshop.Reviews.ProductReviews;

public static class ProductReviewsConfig
{
    public static IHostApplicationBuilder AddProductReviewsServices(this IHostApplicationBuilder builder)
    {
        return builder;
    }

    public static IEndpointRouteBuilder MapProductReviewsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var reviews = endpoints.NewVersionedApi(nameof(ProductReview).Pluralize().Kebaberize());
        var reviewsV1 = reviews.MapGroup("/api/v{version:apiVersion}/reviews").HasApiVersion(1.0);
        
        reviewsV1.MapCreateReviewEndpoint();
        reviewsV1.MapGetReviewsByProductEndpoint();
        reviewsV1.MapAnalyzeProductReviewsEndpoint();
        reviewsV1.MapAnalyzeReviewTrendsEndpoint();
        reviewsV1.MapCompareProductsByReviewsEndpoint();
        reviewsV1.MapGetProductQualitySummaryEndpoint();

        return endpoints;
    }
}
