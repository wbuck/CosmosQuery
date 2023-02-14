namespace CosmosQuery.Benchmarks.Data.Models;

public sealed record EntryModel
{
    public DomainControllerModel Dc { get; init; } = default!;
}
