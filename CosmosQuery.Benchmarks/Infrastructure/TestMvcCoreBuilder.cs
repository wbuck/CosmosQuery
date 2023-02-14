using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace CosmosQuery.Benchmarks.Infrastructure;

internal sealed class TestMvcCoreBuilder : IMvcCoreBuilder
{
    public ApplicationPartManager PartManager { get; set; } = default!;
    public IServiceCollection Services { get; set; } = default!;
}
