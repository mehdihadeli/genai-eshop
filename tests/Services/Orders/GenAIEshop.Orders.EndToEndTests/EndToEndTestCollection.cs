using GenAIEshop.Orders.Shared.Data;
using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Orders.EndToEndTests;

[CollectionDefinition(Name)]
public sealed class EndToEndTestCollection
    : ICollectionFixture<SharedFixtureWithEfCoreAndRedis<Program, OrdersDbContext>>
{
    public const string Name = "Orders end-to-end tests";
}
