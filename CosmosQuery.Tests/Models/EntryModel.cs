namespace CosmosQuery.Tests.Models;

public sealed record EntryModel
{
    public DomainControllerModel Dc { get; init; } = default!;
}
