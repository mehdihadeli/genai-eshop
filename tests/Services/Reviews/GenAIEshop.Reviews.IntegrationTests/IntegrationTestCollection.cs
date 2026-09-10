using GenAIEshop.Reviews.Shared.Data;
using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Reviews.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<SharedFixtureWithEfCore<Program, ReviewsDbContext>>
{
    public const string Name = "Reviews integration tests";
}
