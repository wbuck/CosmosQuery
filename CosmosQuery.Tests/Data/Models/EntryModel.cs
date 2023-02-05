namespace CosmosQuery.Tests.Data.Models;

public sealed record EntryModel
{
    public DomainControllerModel Dc { get; init; } = default!;
}
