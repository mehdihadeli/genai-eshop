using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Recommendation.EndToEndTests;

[CollectionDefinition(Name)]
public sealed class EndToEndTestCollection : ICollectionFixture<SharedFixture<Program>>
{
    public const string Name = "Recommendation end-to-end tests";
}
