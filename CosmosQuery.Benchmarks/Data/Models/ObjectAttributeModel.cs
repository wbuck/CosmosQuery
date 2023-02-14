namespace CosmosQuery.Benchmarks.Data.Models;

public sealed record ObjectAttributeModel
{
    public string Name { get; init; } = default!;
    public string Value { get; init; } = default!;
}
