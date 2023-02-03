using CosmosQuery.Tests.Entities;
using CosmosQuery.Tests.Models;

namespace CosmosQuery.Tests.Mappings;

internal sealed class NetworkInformationMapping : Profile
{
    public NetworkInformationMapping()
    {
        CreateMap<NetworkInformation, NetworkInformationModel>()
            .ForAllMembers(opts => opts.ExplicitExpansion());
    }
}
