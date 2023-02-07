using CosmosQuery;
using ExpressionBuilder.Tests.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace ExpressionBuilder.Tests;

public sealed class QueryTests
{
    private readonly IServiceProvider serviceProvider;

    public QueryTests()
	{
        IServiceCollection services = new ServiceCollection();
        services.AddTransient<IApplicationBuilder>(sp => new ApplicationBuilder(sp))
            .AddTransient<IRouteBuilder>(sp => new RouteBuilder(sp.GetRequiredService<IApplicationBuilder>()));

        this.serviceProvider = services.BuildServiceProvider();
    }

	[Fact]
	public void Warren_Test()
	{
        GetSelectNestedQuery<Product, int>("AlternateIDs($orderby=$this)");
	}

    private static void GetSelectNestedQuery<TModel, TElement>(string selectString) where TModel : class
        => GetNestedQueryOptions<TModel, TElement, PathSelectItem>
           (
                new Dictionary<string, string> { ["$select"] = selectString }
           );

    private static void GetExpandNestedQuery<TModel, TElement>(string selectString) where TModel : class
        => GetNestedQueryOptions<TModel, TElement, ExpandedNavigationSelectItem>
           (
                new Dictionary<string, string> { ["$expand"] = selectString }
           );

    private static void GetNestedQueryOptions<TModel, TElement, TPath>(IDictionary<string, string> queryOptions)
		where TModel : class
		where TPath : SelectItem
	{
		SelectExpandClause clause = ODataHelpers.GetSelectExpandClause<TModel>(queryOptions);
        QueryOptions option = clause.SelectedItems
            .OfType<TPath>()
            .Select(GetQueryOptions)
            .Last();

        Debugger.Break();

        static QueryOptions GetQueryOptions(SelectItem item) => item switch
		{
			PathSelectItem select => new QueryOptions(select.OrderByOption, (int?)select.TopOption, (int?)select.SkipOption),
			ExpandedNavigationSelectItem expand => new QueryOptions(expand.OrderByOption, (int?)expand.TopOption, (int?)expand.SkipOption),
			_ => throw new NotSupportedException()
		};

	}
}
