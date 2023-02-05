namespace CosmosQuery.Tests.Data.Entities;

public abstract record EntityBase
{
    public Guid Id { get; init; }
    public Guid ForestId { get; init; }
}
