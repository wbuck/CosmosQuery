namespace CosmosQuery.Tests.Models;

public sealed record AdminGroupModel
{
    public ICollection<UserObjectModel> UserObjects { get; init; }
        = new List<UserObjectModel>();
}
