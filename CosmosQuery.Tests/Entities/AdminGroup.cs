namespace CosmosQuery.Tests.Entities;

public sealed record AdminGroup
{
    public ICollection<UserObject> UserObjects { get; init; } 
        = new List<UserObject>();
}


