using QueryExpression.Tests.Data.Entities;
using QueryExpression.Tests.Data.Models;

namespace QueryExpression.Tests.Data.Mappings;

internal sealed class NetworkInformationMapping : Profile
{
    public NetworkInformationMapping()
    {
        CreateMap<NetworkInformation, NetworkInformationModel>()
            .ForAllMembers(opts => opts.ExplicitExpansion());
    }
}
