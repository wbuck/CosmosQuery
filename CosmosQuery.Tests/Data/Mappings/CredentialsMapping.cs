using CosmosQuery.Tests.Data.Entities;
using CosmosQuery.Tests.Data.Models;

namespace CosmosQuery.Tests.Data.Mappings;

internal sealed class CredentialsMapping : Profile
{
	
	public CredentialsMapping()
	{
		CreateMap<Credentials, CredentialsModel>()
			.ForAllMembers(opts => opts.ExplicitExpansion());
	}
}
