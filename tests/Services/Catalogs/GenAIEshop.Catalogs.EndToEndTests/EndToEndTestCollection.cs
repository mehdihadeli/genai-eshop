using GenAIEshop.Catalogs.Shared.Data;
using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Catalogs.EndToEndTests;

[CollectionDefinition(Name)]
public sealed class EndToEndTestCollection : ICollectionFixture<SharedFixtureWithEfCore<Program, CatalogsDbContext>>
{
    public const string Name = "Catalogs end-to-end tests";
}
