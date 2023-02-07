using QueryExpression.Tests.Data.Entities;
using QueryExpression.Tests.Data.Models;

namespace QueryExpression.Tests.Data.Mappings;

internal sealed class CredentialsMapping : Profile
{
	
	public CredentialsMapping()
	{
		CreateMap<Credentials, CredentialsModel>()
			.ForAllMembers(opts => opts.ExplicitExpansion());
	}
}
