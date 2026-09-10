using BuildingBlocks.Types;
using GenAIEshop.Catalogs.Products.Features.GettingProducts;
using GenAIEshop.Catalogs.Products.Models;
using GenAIEshop.Catalogs.Shared.Data;
using GenAIEshop.Tests.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenAIEshop.Catalogs.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class ProductsHandlerIntegrationTests(SharedFixtureWithEfCore<Program, CatalogsDbContext> sharedFixture)
    : IntegrationTestBase<Program, CatalogsDbContext>(sharedFixture)
{
    [Fact]
    public async Task Get_products_reads_available_products_from_postgres()
    {
        await using var db = SharedFixture.CreateDbContext();
        await SharedFixture.EnsureDatabaseCreatedAsync();

        db.Products.AddRange(
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Available keyboard",
                Price = 99,
                ImageUrl = "https://example.test/keyboard",
                IsAvailable = true,
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Unavailable keyboard",
                Price = 89,
                ImageUrl = "https://example.test/unavailable",
                IsAvailable = false,
            }
        );
        await db.SaveChangesAsync();

        var result = await new GetProductsHandler(db, NullLogger<GetProductsHandler>.Instance).Handle(
            GetProducts.Of(1, 10),
            CancellationToken.None
        );

        result.Products.Count.ShouldBe(1);
        result.Products.Single().Name.ShouldBe("Available keyboard");
    }
}
