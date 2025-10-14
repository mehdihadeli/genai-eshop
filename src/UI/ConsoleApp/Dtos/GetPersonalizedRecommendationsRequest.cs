namespace ConsoleApp.Dtos;

public sealed record GetPersonalizedRecommendationsRequest(
    Guid UserId,
    string? Query = null,
    string? Preferences = null,
    string? Category = null
);