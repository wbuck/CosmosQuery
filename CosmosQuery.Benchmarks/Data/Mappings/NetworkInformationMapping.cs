using AutoMapper;
using CosmosQuery.Benchmarks.Data.Entities;
using CosmosQuery.Benchmarks.Data.Models;

namespace CosmosQuery.Benchmarks.Data.Mappings;

internal sealed class NetworkInformationMapping : Profile
{
    public NetworkInformationMapping()
    {
        CreateMap<NetworkInformation, NetworkInformationModel>()
            .ForAllMembers(opts => opts.ExplicitExpansion());
    }
}
