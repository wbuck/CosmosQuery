namespace CosmosQuery.Benchmarks.Data.Models;

public sealed record BackupLocationModel
{
    public CredentialsModel? Credentials { get; init; }
    public NetworkInformationModel? NetworkInformation { get; init; }
}


