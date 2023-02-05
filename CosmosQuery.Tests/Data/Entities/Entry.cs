namespace CosmosQuery.Tests.Data.Entities;

public sealed record Entry
{
    public DomainController Dc { get; init; } = default!;
}
