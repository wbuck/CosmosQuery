using CosmosQuery.Tests.Entities;
using CosmosQuery.Tests.Models;

namespace CosmosQuery.Tests.Mappings;

internal sealed class DomainControllerMapping : Profile
{
	public DomainControllerMapping()
	{
        CreateMap<DomainControllerEntry, DomainControllerEntryModel>()
            .ForMember(dest => dest.DateAdded, opts => opts.MapFrom(src => src.DateCreated))
            .ForAllMembers(opts => opts.ExplicitExpansion());

        CreateMap<Entry, EntryModel>()
            .ForAllMembers(opts => opts.ExplicitExpansion());

        CreateMap<DomainController, DomainControllerModel>()
			.ForMember(dest => dest.FullyQualifiedDomainName, opts => opts.MapFrom(src => src.Fqdn))
			.ForAllMembers(opts => opts.ExplicitExpansion());
	}
}
