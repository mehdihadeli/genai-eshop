using System.Text.Json;
using BuildingBlocks.EF;
using BuildingBlocks.Serialization;
using GenAIEshop.Reviews.ProductReviews.Models;
using Microsoft.EntityFrameworkCore;

namespace GenAIEshop.Reviews.Shared.Data;

public class ReviewsDataSeeder(IWebHostEnvironment environment, ILogger<ReviewsDataSeeder> logger)
    : IDataSeeder<ReviewsDbContext>
{
    public async Task SeedAsync(ReviewsDbContext context)
    {
        if (await context.ProductReviews.AnyAsync())
            return;

        var seedData = await LoadSeedDataAsync();
        if (seedData == null)
        {
            logger.LogError("No seed data found or failed to load JSON data.");
            return;
        }

        await SeedReviews(context, seedData);
    }

    private async Task SeedReviews(ReviewsDbContext context, ReviewsSeedData seedData)
    {
        try
        {
            await context.ProductReviews.AddRangeAsync(
                seedData.ProductReviews.Select(x => new ProductReview
                {
                    Id = x.Id,
                    Comment = x.Comment,
                    Rating = x.Rating,
                    ProductId = x.ProductId,
                    UserId = x.UserId,
                })
            );
            var result = await context.SaveChangesAsync();

            logger.LogInformation($"Successfully seeded {result} entities. ");
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to seed catalogs data: {ex.Message}");
            if (ex.InnerException != null)
            {
                logger.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            throw new Exception("Failed to seed catalogs data", ex);
        }
    }

    private async Task<ReviewsSeedData?> LoadSeedDataAsync()
    {
        var filePath = Path.Combine(environment.ContentRootPath, "Shared", "Data", "Setup", "reviews-seed-data.json");

        if (!File.Exists(filePath))
        {
            logger.LogError($"Seed data file not found: {filePath}");
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var seedData = JsonSerializer.Deserialize<ReviewsSeedData>(
                json,
                SystemTextJsonSerializerOptions.DefaultSerializerOptions
            );

            logger.LogInformation($"Loaded seed data: {seedData?.ProductReviews.Count ?? 0} ProductReviews");
            return seedData;
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to deserialize seed data: {ex.Message}");
            return null;
        }
    }

    private class ReviewsSeedData
    {
        public List<ProductReviewSeed> ProductReviews { get; set; } = default!;

        public class ProductReviewSeed
        {
            public Guid Id { get; set; }
            public required Guid ProductId { get; set; }
            public required Guid UserId { get; set; }
            public required int Rating { get; set; }
            public string? Comment { get; set; }
        }
    }
}
