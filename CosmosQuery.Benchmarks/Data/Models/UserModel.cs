namespace CosmosQuery.Benchmarks.Data.Models;

public sealed record UserModel
{
    public Guid Id { get; init; }
    public Guid ForestId { get; init; }
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
}
