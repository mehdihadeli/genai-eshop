namespace ConsoleApp.Dtos;

public sealed record AnalyzeProductReviewsResponse(Guid ProductId, string Analysis, DateTime GeneratedAt);