using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Carts.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<SharedFixture<Program>>
{
    public const string Name = "Carts integration tests";
}
