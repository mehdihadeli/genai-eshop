using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Carts.EndToEndTests;

[CollectionDefinition(Name)]
public sealed class EndToEndTestCollection : ICollectionFixture<SharedFixtureWithRedis<Program>>
{
    public const string Name = "Carts end-to-end tests";
}
