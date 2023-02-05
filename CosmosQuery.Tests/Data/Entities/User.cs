namespace CosmosQuery.Tests.Data.Entities;

public sealed record User : EntityBase
{    
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
}


