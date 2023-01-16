namespace CosmosQuery;

/// <summary>
/// Miscellaneous arguments for IMapper.ProjectTo
/// </summary>
public sealed record ProjectionSettings
{
    public object? Parameters { get; init; }
}