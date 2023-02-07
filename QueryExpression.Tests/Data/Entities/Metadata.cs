namespace QueryExpression.Tests.Data.Entities;

public sealed record Metadata
{
    public string MetadataType { get; init; } = default!;
    public ICollection<MetadataKeyValue> MetadataKeyValuePairs { get; init; } 
        = new List<MetadataKeyValue>();
}
