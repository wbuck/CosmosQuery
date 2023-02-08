﻿using QueryExpression.Tests.Data.Entities;
using QueryExpression.Tests.Data.Models;

namespace QueryExpression.Tests.Data.Mappings;

internal sealed class UserMapping : Profile
{
	public UserMapping()
	{
        CreateMap<AdminGroup, AdminGroupModel>()
            .ForAllMembers(opts => opts.ExplicitExpansion());

        CreateMap<UserObject, UserObjectModel>()
            .ForAllMembers(opts => opts.ExplicitExpansion());

        CreateMap<User, UserModel>()
            .ForAllMembers(opts => opts.ExplicitExpansion());
    }
}
