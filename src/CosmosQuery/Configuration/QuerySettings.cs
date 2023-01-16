namespace CosmosQuery;

/// <summary>
/// Settings to use during query composition.
/// </summary>
public sealed record QuerySettings
{
    /// <summary>
    /// Settings for configuring OData options on the server.
    /// </summary>
    public ODataSettings? ODataSettings { get; init; }

    /// <summary>
    /// Miscellaneous arguments for IMapper.ProjectTo.
    /// </summary>
    public ProjectionSettings? ProjectionSettings { get; init; }
}
