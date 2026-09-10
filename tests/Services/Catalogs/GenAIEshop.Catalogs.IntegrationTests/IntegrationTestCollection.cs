using GenAIEshop.Catalogs.Shared.Data;
using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Catalogs.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<SharedFixtureWithEfCore<Program, CatalogsDbContext>>
{
    public const string Name = "Catalogs integration tests";
}
