using CosmosQuery.Tests.Entities;
using CosmosQuery.Tests.Models;

namespace CosmosQuery.Tests.Mappings;

internal sealed class ObjectAttributeMapping : Profile
{
	public ObjectAttributeMapping()
	{
		CreateMap<ObjectAttribute, ObjectAttributeModel>()
			.ForAllMembers(opts => opts.ExplicitExpansion());
	}
}
