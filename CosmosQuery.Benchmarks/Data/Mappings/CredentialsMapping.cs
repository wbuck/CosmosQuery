using AutoMapper;
using CosmosQuery.Benchmarks.Data.Entities;
using CosmosQuery.Benchmarks.Data.Models;

namespace CosmosQuery.Benchmarks.Data.Mappings;

internal sealed class CredentialsMapping : Profile
{
	
	public CredentialsMapping()
	{
		CreateMap<Credentials, CredentialsModel>()
			.ForAllMembers(opts => opts.ExplicitExpansion());
	}
}
