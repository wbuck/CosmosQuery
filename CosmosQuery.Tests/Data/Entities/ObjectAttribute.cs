namespace CosmosQuery.Tests.Data.Entities;

public sealed record ObjectAttribute
{
    public string Name { get; init; } = default!;
    public string Value { get; init; } = default!;
}
