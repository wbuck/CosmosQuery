using AutoMapper;
using CosmosQuery.Benchmarks.Data.Entities;
using CosmosQuery.Benchmarks.Data.Models;

namespace CosmosQuery.Benchmarks.Data.Mappings;

internal sealed class ObjectAttributeMapping : Profile
{
	public ObjectAttributeMapping()
	{
		CreateMap<ObjectAttribute, ObjectAttributeModel>()
			.ForAllMembers(opts => opts.ExplicitExpansion());
	}
}
