using GenAIEshop.Reviews.Shared.Data;
using GenAIEshop.Tests.Shared;

namespace GenAIEshop.Reviews.EndToEndTests;

[CollectionDefinition(Name)]
public sealed class EndToEndTestCollection : ICollectionFixture<SharedFixtureWithEfCore<Program, ReviewsDbContext>>
{
    public const string Name = "Reviews end-to-end tests";
}
