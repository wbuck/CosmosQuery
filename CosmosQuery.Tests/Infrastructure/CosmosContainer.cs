using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CosmosQuery.Tests.Infrastructure;

public sealed class CosmosContainer : IAsyncLifetime
{
    private const string CosmosKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
    private const string DatabaseId = "ForestDatabase";
    private const string ContainerId = "ForestContainer";
    private const string PartitionKey = "/forestId";

    private readonly CosmosDbTestcontainer? testContainer;
    private readonly CosmosDbTestcontainerConfiguration? configuration;

    private CosmosClient? client;

    public CosmosContainer()
    {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        if (!IsGithubAction())
        {
            configuration = new();
            testContainer = new TestcontainersBuilder<CosmosDbTestcontainer>()
                .WithDatabase(configuration)
                .WithCleanUp(true)
                .Build();
        }
    }

    //localhost:49162/_explorer/index.html
    public async Task InitializeAsync()
    {
        if (IsGithubAction())
        {
            var client = CreateCosmosClient
            (
                "https://localhost:8081",
                CosmosKey
            );

            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            this.client = await InitializeCosmosClientAsync(client, cts.Token).ConfigureAwait(false);
        }
        else
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            await testContainer!.StartAsync(cts.Token).ConfigureAwait(false);

            var client = CreateCosmosClient(testContainer.ConnectionString);
            this.client = await InitializeCosmosClientAsync(client, cts.Token).ConfigureAwait(false);
        }
    }

    public Container GetContainer() =>
        client?.GetContainer(DatabaseId, ContainerId)
            ?? throw new InvalidOperationException($"{nameof(InitializeAsync)} must be called");

    public async Task DisposeAsync()
    {
        if (!IsGithubAction())
        {
            await testContainer!.DisposeAsync().ConfigureAwait(false);
            configuration!.Dispose();
        }
        client?.Dispose();
    }

    private static CosmosClient CreateCosmosClient(string endpoint, string key) =>
        new(endpoint, key, new()
        {
            AllowBulkExecution = true,
            SerializerOptions = new() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase }
        });

    private CosmosClient CreateCosmosClient(string connectionString) =>
        new(connectionString, new()
        {
            ConnectionMode = ConnectionMode.Gateway,
            HttpClientFactory = () => testContainer!.HttpClient,
            AllowBulkExecution = true,
            SerializerOptions = new() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase }
        });


    private static async Task<CosmosClient> InitializeCosmosClientAsync(CosmosClient client, CancellationToken cancellationToken = default)
    {
        try
        {
            var databaseResponse = await client
                .CreateDatabaseIfNotExistsAsync(DatabaseId, 4000, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var containerResponse = await databaseResponse.Database.DefineContainer(ContainerId, PartitionKey)
                .WithIndexingPolicy()
                    .WithIncludedPaths()
                        .Path("/*")
                    .Attach()
                    .WithCompositeIndex()
                        .Path("/name")
                        .Path("/createdDate")
                    .Attach()
                .Attach()
                .CreateIfNotExistsAsync(4000, cancellationToken).ConfigureAwait(false);

            await containerResponse.Container.SeedAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            client.Dispose();
            throw;
        }
        return client;
    }

    private static bool IsGithubAction() =>
        Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null;
}
