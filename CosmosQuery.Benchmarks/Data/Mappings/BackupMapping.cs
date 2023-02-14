using AutoMapper;
using CosmosQuery.Benchmarks.Data.Entities;
using CosmosQuery.Benchmarks.Data.Models;

namespace CosmosQuery.Benchmarks.Data.Mappings;

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
