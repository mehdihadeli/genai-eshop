using System.Text.Json;
using BuildingBlocks.EF;
using BuildingBlocks.Serialization;
using BuildingBlocks.VectorDB.Contracts;
using GenAIEshop.Catalogs.Products.Models;
using Microsoft.EntityFrameworkCore;

namespace GenAIEshop.Catalogs.Shared.Data;

public class CatalogsDataSeeder(
    IWebHostEnvironment environment,
    IDataIngestor<Product> productVectorIngestor,
    ILogger<CatalogsDataSeeder> logger
) : IDataSeeder<CatalogsDbContext>
{
    public async Task SeedAsync(CatalogsDbContext context)
    {
        if (await context.Products.AnyAsync())
            return;

        var seedData = await LoadSeedDataAsync();
        if (seedData == null)
        {
            logger.LogError("No seed data found or failed to load JSON data.");
            return;
        }

        await SeedProducts(context, seedData);
    }

    private async Task SeedProducts(CatalogsDbContext context, ProductsSeedData seedData)
    {
        await SeedProductsInEntityFrameworkAsync(context, seedData);
        await SeedProductsInVectorDBAsync(context);
    }

    private async Task SeedProductsInVectorDBAsync(CatalogsDbContext context)
    {
        var products = context.Products.ToList();
        await productVectorIngestor.IngestDataAsync(products);
    }

    private async Task SeedProductsInEntityFrameworkAsync(CatalogsDbContext context, ProductsSeedData seedData)
    {
        try
        {
            await context.Products.AddRangeAsync(
                seedData.Products.Select(x => new Product
                {
                    ImageUrl = x.ImageUrl,
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    IsAvailable = x.IsAvailable,
                    Id = x.Id,
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

    private async Task<ProductsSeedData?> LoadSeedDataAsync()
    {
        var filePath = Path.Combine(environment.ContentRootPath, "Shared", "Data", "Setup", "catalogs-seed-data.json");

        if (!File.Exists(filePath))
        {
            logger.LogError($"Seed data file not found: {filePath}");
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var seedData = JsonSerializer.Deserialize<ProductsSeedData>(
                json,
                SystemTextJsonSerializerOptions.DefaultSerializerOptions
            );

            logger.LogInformation($"Loaded seed data: {seedData?.Products.Count ?? 0} products");
            return seedData;
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to deserialize seed data: {ex.Message}");
            return null;
        }
    }

    private class ProductsSeedData
    {
        public List<ProductSeed> Products { get; set; } = default!;

        public class ProductSeed
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public bool IsAvailable { get; set; }
            public string ImageUrl { get; set; } = string.Empty;
        }
    }
}
