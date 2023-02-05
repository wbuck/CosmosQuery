using CosmosQuery.Tests.Data.Entities;
using CosmosQuery.Tests.Data.Models;

namespace CosmosQuery.Tests.Data.Mappings;

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
