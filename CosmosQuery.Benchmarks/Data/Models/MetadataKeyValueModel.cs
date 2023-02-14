namespace CosmosQuery.Benchmarks.Data.Models;

public sealed record MetadataKeyValueModel
{
    public string Key { get; init; } = default!;
    public int Value { get; init; }
}
