using QueryExpression.Tests.Data.Entities;
using QueryExpression.Tests.Data.Models;

namespace QueryExpression.Tests.Data.Mappings;

internal sealed class ObjectAttributeMapping : Profile
{
	public ObjectAttributeMapping()
	{
		CreateMap<ObjectAttribute, ObjectAttributeModel>()
			.ForAllMembers(opts => opts.ExplicitExpansion());
	}
}
