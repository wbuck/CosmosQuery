using AgileObjects.ReadableExpressions;
using CosmosQuery;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using QueryExpression.Tests.Data;
using QueryExpression.Tests.Data.Entities;
using QueryExpression.Tests.Data.Mappings;
using QueryExpression.Tests.Data.Models;
using QueryExpression.Tests.Infrastructure;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace QueryExpression.Tests;

internal static class QueryableExtensions
{
    public static string GetString<TModel>(this IQueryable<TModel> queryable) where TModel : class
        => queryable.Expression.ToReadableString()
            .Replace("\r\n", string.Empty)
            .Replace(" ", string.Empty);
}

public sealed class QueryTests
{
    private readonly IServiceProvider serviceProvider;
    private readonly IQueryable<Forest> _queryable = DatabaseSeeder.GenerateData().AsQueryable();

    public QueryTests()
    {
        IServiceCollection services = new ServiceCollection();
        IMvcCoreBuilder builder = new TestMvcCoreBuilder
        {
            Services = services
        };

        builder.AddOData();
        services
            .AddSingleton<IConfigurationProvider>(new MapperConfiguration(cfg => cfg.AddMaps(typeof(IAssemablyMarker).Assembly)))
            .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<IConfigurationProvider>(), sp.GetService))
            .AddTransient<IApplicationBuilder>(sp => new ApplicationBuilder(sp))
            .AddRouting()
            .AddLogging();

        serviceProvider = services.BuildServiceProvider();
    }

    [Theory]
    [InlineData("/forest?$select=Values($skip=3;$top=2)", "Values=dtoForest.Values.OrderBy(p=>p).Skip(3).Take(2).ToList()")]
    [InlineData("/forest?$select=Values($skip=3)", "Values=dtoForest.Values.OrderBy(p=>p).Skip(3).ToList()")]
    [InlineData("/forest?$select=Values($top=2)", "Values=dtoForest.Values.OrderBy(p=>p).Take(2).ToList()")]
    [InlineData("/forest?$select=Values($orderby=$this)", "Values=dtoForest.Values.OrderBy(p=>p).ToList()")]
    [InlineData("/forest?$select=Values($orderby=$this desc)", "Values=dtoForest.Values.OrderByDescending(p=>p).ToList()")]
    [InlineData("/forest?$select=Values($orderby=$this desc;$top=1;$skip=1)", "Values=dtoForest.Values.OrderByDescending(p=>p).Skip(1).Take(1).ToList()")]
    public void QueryMethods_PrimitiveCollection_ShouldProduceCorrectExpressions(string query, string expected)
    {
        string actual = Get<Forest, ForestModel>(_queryable, query).GetString();
        Assert.Contains(expected, actual);
    }

    [Theory]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($orderby=FullyQualifiedDomainName)",
        @"DomainControllers=dtoForest\.DomainControllers\.Select\(.*?\)\.OrderBy\(it=>it\.Entry\.Dc\.FullyQualifiedDomainName\)\.ToList\(\)"
    )]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($orderby=FullyQualifiedDomainName;$skip=1;$top=2)",
        @"DomainControllers=dtoForest\.DomainControllers\.Select\(.*?\)\.OrderBy\(it=>it\.Entry\.Dc\.FullyQualifiedDomainName\)\.Skip\(1\)\.Take\(2\)\.ToList\(\)"
    )]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($skip=1;$top=2)",
        @"DomainControllers=dtoForest\.DomainControllers\.Select\(.*?\)\.OrderBy\(a=>a\.DateAdded\)\.Skip\(1\)\.Take\(2\)\.ToList\(\)"
    )]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($skip=1)",
        @"DomainControllers=dtoForest\.DomainControllers\.Select\(.*?\)\.OrderBy\(a=>a\.DateAdded\)\.Skip\(1\)\.ToList\(\)"
    )]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($top=2)",
        @"DomainControllers=dtoForest\.DomainControllers\.Select\(.*?\)\.OrderBy\(a=>a\.DateAdded\)\.Take\(2\)\.ToList\(\)"
    )]
    public void QueryMethods_NestedEntityCollection_ShouldProduceCorrectExpression(string query, string pattern)
    {
        string actual = Get<Forest, ForestModel>(_queryable, query).GetString();
        Assert.Matches(pattern, actual);
    }

    private IQueryable<TModel> Get<TEntity, TModel>(IQueryable<TEntity> queryable, string query, ODataQueryOptions<TModel>? options = null, QuerySettings? querySettings = null)
        where TEntity : class
        where TModel : class        
    {
        return
        (
            DoGet
            (
                queryable,
                serviceProvider.GetRequiredService<IMapper>()
            )
        );

        IQueryable<TModel> DoGet(IQueryable<TEntity> dataQueryable, IMapper mapper)
        {
            return dataQueryable.GetQuery
            (
                mapper,
                options ?? GetODataQueryOptions<TModel>(query),
                querySettings!
            );
        }
    }

    private ODataQueryOptions<TModel> GetODataQueryOptions<TModel>(string query, string? customNamespace = null)
        where TModel : class
    {
        return ODataHelpers.GetODataQueryOptions<TModel>
        (
            query,
            this.serviceProvider,
            customNamespace
        );
    }
}