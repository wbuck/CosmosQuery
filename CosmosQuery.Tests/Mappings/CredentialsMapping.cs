using CosmosQuery.Tests.Entities;
using CosmosQuery.Tests.Models;

namespace CosmosQuery.Tests.Mappings;

internal sealed class CredentialsMapping : Profile
{
	public CredentialsMapping()
	{
		CreateMap<Credentials, CredentialsModel>()
			.ForAllMembers(opts => opts.ExplicitExpansion());
	}
}
