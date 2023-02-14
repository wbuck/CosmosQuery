using AutoMapper;
using CosmosQuery.Benchmarks.Data.Entities;
using CosmosQuery.Benchmarks.Data.Models;

namespace CosmosQuery.Benchmarks.Data.Mappings;

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
