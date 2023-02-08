﻿using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace QueryExpression.Tests.Infrastructure;

internal sealed class TestMvcCoreBuilder : IMvcCoreBuilder
{
    public ApplicationPartManager PartManager { get; set; } = default!;
    public IServiceCollection Services { get; set; } = default!;
}
