namespace QueryExpression.Tests.Data.Models;

public sealed record MetadataModel
{
    public string MetadataType { get; init; } = default!;
    public ICollection<MetadataKeyValueModel> MetadataKeyValuePairs { get; init; }
        = new List<MetadataKeyValueModel>();
}
