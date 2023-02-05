using CosmosQuery.Tests.Data.Entities;
using CosmosQuery.Tests.Data.Models;

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
