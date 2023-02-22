using CosmosQuery.Tests.Data;
using Microsoft.Azure.Cosmos;

namespace CosmosQuery.Tests.Infrastructure;

internal static class ContainerExtensions
{
    public static async Task SeedAsync(this Container dbContainer, CancellationToken cancellationToken = default)
    {
        var data = DatabaseSeeder.GenerateData();
        foreach (var forest in data)
        {
            var key = new PartitionKey(forest.ForestId.ToString());
            await dbContainer.CreateItemAsync(forest, key, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
