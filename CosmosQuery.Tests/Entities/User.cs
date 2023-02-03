namespace CosmosQuery.Tests.Entities;

public sealed record User : EntityBase
{    
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
}


