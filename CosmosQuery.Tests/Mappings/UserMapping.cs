using CosmosQuery.Tests.Entities;
using CosmosQuery.Tests.Models;

namespace CosmosQuery.Tests.Mappings;

internal sealed class UserMapping : Profile
{
	public UserMapping()
	{
        CreateMap<AdminGroup, AdminGroupModel>()
            .ForAllMembers(opts => opts.ExplicitExpansion());

        CreateMap<UserObject, UserObjectModel>()
            .ForAllMembers(opts => opts.ExplicitExpansion());

        CreateMap<User, UserModel>()
            .ForAllMembers(opts => opts.ExplicitExpansion());
    }
}
