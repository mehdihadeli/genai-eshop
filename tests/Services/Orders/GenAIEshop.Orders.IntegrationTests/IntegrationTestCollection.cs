using GenAIEshop.Orders.Shared.Data;
using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Orders.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<SharedFixtureWithEfCore<Program, OrdersDbContext>>
{
    public const string Name = "Orders integration tests";
}
