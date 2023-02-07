namespace QueryExpression.Tests.Data.Models;

public sealed record UserObjectModel
{
    public UserModel User { get; init; } = default!;
}
