namespace CosmosQuery.Tests.Models;

public sealed record UserObjectModel
{
    public UserModel User { get; init; } = default!;
}
