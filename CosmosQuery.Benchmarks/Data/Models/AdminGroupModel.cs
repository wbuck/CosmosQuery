namespace CosmosQuery.Benchmarks.Data.Models;

public sealed record AdminGroupModel
{
    public ICollection<UserObjectModel> UserObjects { get; init; }
        = new List<UserObjectModel>();
}
