namespace CosmosQuery.Tests.Data.Models;

public sealed record DomainControllerEntryModel
{
    public DateTimeOffset DateAdded { get; init; }
    public EntryModel Entry { get; init; } = default!;
    public CredentialsModel? DcCredentials { get; init; }
    public NetworkInformationModel? DcNetworkInformation { get; init; }
}
