namespace CosmosQuery;

internal sealed class OrderBySetting
{
    public string Name { get; set; } = default!;
    public OrderBySetting? ThenBy { get; set; }
}
