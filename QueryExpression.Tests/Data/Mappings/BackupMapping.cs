using QueryExpression.Tests.Data.Entities;
using QueryExpression.Tests.Data.Models;

namespace QueryExpression.Tests.Data.Mappings;

internal sealed class BackupMapping : Profile
{
	public BackupMapping()
	{
		CreateMap<BackupLocation, BackupLocationModel>()
            .ForAllMembers(opts => opts.ExplicitExpansion());
        CreateMap<Backup, BackupModel>()           
            .ForAllMembers(opts => opts.ExplicitExpansion());
    }
}
