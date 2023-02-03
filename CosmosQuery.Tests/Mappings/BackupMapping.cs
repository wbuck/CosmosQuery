using CosmosQuery.Tests.Entities;
using CosmosQuery.Tests.Models;

namespace CosmosQuery.Tests.Mappings;

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
