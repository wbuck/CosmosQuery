namespace CosmosQuery.Tests.Models;

public sealed record MetadataKeyValueModel
{
    public string Key { get; init; } = default!;
    public int Value { get; init; }
}
