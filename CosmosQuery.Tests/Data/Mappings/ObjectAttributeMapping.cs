using CosmosQuery.Tests.Data.Entities;
using CosmosQuery.Tests.Data.Models;

namespace CosmosQuery.Tests.Data.Mappings;

internal sealed class ObjectAttributeMapping : Profile
{
	public ObjectAttributeMapping()
	{
		CreateMap<ObjectAttribute, ObjectAttributeModel>()
			.ForAllMembers(opts => opts.ExplicitExpansion());
	}
}
