using CosmosQuery.Tests.Data.Entities;
using CosmosQuery.Tests.Data.Models;

namespace CosmosQuery.Tests.Mappings;

internal sealed class ForestMapping : Profile
{
	public ForestMapping()
	{
        CreateMap<MetadataKeyValue, MetadataKeyValueModel>()
            .ForAllMembers(opts => opts.ExplicitExpansion());

        CreateMap<Metadata, MetadataModel>()
            .ForAllMembers(opts => opts.ExplicitExpansion());

        CreateMap<Forest, ForestModel>()
			.ForMember(dest => dest.ForestName, opts => opts.MapFrom(src => src.Name))			
			.ForAllMembers(opts => opts.ExplicitExpansion());

    }
}
