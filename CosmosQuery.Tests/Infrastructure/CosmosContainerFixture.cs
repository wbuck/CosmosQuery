namespace CosmosQuery.Tests.Infrastructure;

[CollectionDefinition(nameof(CosmosContainer))]
public sealed class CosmosContainerFixture : ICollectionFixture<CosmosContainer>
{ }
