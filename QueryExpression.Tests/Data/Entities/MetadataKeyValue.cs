namespace QueryExpression.Tests.Data.Entities;

public sealed record MetadataKeyValue
{
    public string Key { get; init; } = default!;
    public int Value { get; init; }
}
