namespace QueryExpression.Tests.Data.Entities;

public sealed record BackupLocation
{
    public Credentials? Credentials { get; init; }
    public NetworkInformation? NetworkInformation { get; init; }
}


