using AgileObjects.ReadableExpressions;
using CosmosQuery;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using QueryExpression.Tests.Data.Entities;
using QueryExpression.Tests.Data.Mappings;
using QueryExpression.Tests.Data.Models;
using QueryExpression.Tests.Infrastructure;
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
    private readonly IQueryable<Forest> queryable = new List<Forest>().AsQueryable();

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
        string actual = Get<Forest, ForestModel>(this.queryable, query).GetString();
        Assert.Contains(expected, actual);
    }

    [Theory]
    [InlineData("/forest?$select=ValuesArray($skip=3;$top=2)", "ValuesArray=dtoForest.ValuesArray.OrderBy(p=>p).Skip(3).Take(2).ToArray()")]
    [InlineData("/forest?$select=ValuesArray($skip=3)", "ValuesArray=dtoForest.ValuesArray.OrderBy(p=>p).Skip(3).ToArray()")]
    [InlineData("/forest?$select=ValuesArray($top=2)", "ValuesArray=dtoForest.ValuesArray.OrderBy(p=>p).Take(2).ToArray()")]
    [InlineData("/forest?$select=ValuesArray($orderby=$this)", "ValuesArray=dtoForest.ValuesArray.OrderBy(p=>p).ToArray()")]
    [InlineData("/forest?$select=ValuesArray($orderby=$this desc)", "ValuesArray=dtoForest.ValuesArray.OrderByDescending(p=>p).ToArray()")]
    [InlineData("/forest?$select=ValuesArray($orderby=$this desc;$top=1;$skip=1)", "ValuesArray=dtoForest.ValuesArray.OrderByDescending(p=>p).Skip(1).Take(1).ToArray()")]
    public void QueryMethods_PrimitiveArray_ShouldProduceCorrectExpressions(string query, string expected)
    {
        string actual = Get<Forest, ForestModel>(this.queryable, query).GetString();
        Assert.Contains(expected, actual);
    }

    [Theory]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($orderby=FullyQualifiedDomainName desc)",
        @"DomainControllers=dtoForest\.DomainControllers\.Select\(.*?\)\.OrderByDescending\(it=>it\.Entry\.Dc\.FullyQualifiedDomainName\)\.ToList\(\)"
    )]
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
        "/forest?$expand=DomainControllers/Entry/Dc($orderby=FullyQualifiedDomainName desc;$skip=1;$top=2)",
        @"DomainControllers=dtoForest\.DomainControllers\.Select\(.*?\)\.OrderByDescending\(it=>it\.Entry\.Dc\.FullyQualifiedDomainName\)\.Skip\(1\)\.Take\(2\)\.ToList\(\)"
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
        string actual = Get<Forest, ForestModel>(this.queryable, query).GetString();
        Assert.Matches(pattern, actual);
    }

    [Theory]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($expand=Backups($orderby=DateCreated desc))",
        @"Select\(dtoBackup=>.*?\).OrderByDescending\(it=>it\.DateCreated\)\.ToList\(\)"
    )]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($expand=Backups($orderby=DateCreated desc;$top=1;$skip=2))",
        @"Select\(dtoBackup=>.*?\).OrderByDescending\(it=>it\.DateCreated\)\.Skip\(2\)\.Take\(1\)\.ToList\(\)"
    )]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($expand=Backups($orderby=DateCreated desc;$top=1))",
        @"Select\(dtoBackup=>.*?\).OrderByDescending\(it=>it\.DateCreated\)\.Take\(1\)\.ToList\(\)"
    )]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($expand=Backups($orderby=DateCreated desc;$skip=1))",
        @"Select\(dtoBackup=>.*?\).OrderByDescending\(it=>it\.DateCreated\)\.Skip\(1\)\.ToList\(\)"
    )]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($expand=Backups($top=1;$skip=2))",
        @"Select\(dtoBackup=>.*?\).OrderBy\(a=>a\.Id\)\.Skip\(2\)\.Take\(1\)\.ToList\(\)"
    )]
    public void QueryMethods_NestedNestedEntityCollection_ShouldProduceCorrectExpression(string query, string pattern)
    {
        string actual = Get<Forest, ForestModel>(this.queryable, query).GetString();
        Assert.Matches(pattern, actual);
    }

    [Theory]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($expand=Backups($select=StringValuesArray($orderby=$this)))",
        @"StringValuesArray=dtoBackup\.StringValuesArray\.OrderBy\(p=>p\)\.ToArray\(\)"
    )]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($expand=Backups($select=StringValuesArray($orderby=$this;$top=1;$skip=1)))",
        @"StringValuesArray=dtoBackup\.StringValuesArray\.OrderBy\(p=>p\)\.Skip\(1\)\.Take\(1\)\.ToArray\(\)"
    )]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($expand=Backups($select=StringValuesArray($orderby=$this;$top=1)))",
        @"StringValuesArray=dtoBackup\.StringValuesArray\.OrderBy\(p=>p\)\.Take\(1\)\.ToArray\(\)"
    )]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($expand=Backups($select=StringValuesArray($orderby=$this;$skip=1)))",
        @"StringValuesArray=dtoBackup\.StringValuesArray\.OrderBy\(p=>p\)\.Skip\(1\)\.ToArray\(\)"
    )]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($expand=Backups($select=StringValuesArray($top=1;$skip=1)))",
        @"StringValuesArray=dtoBackup\.StringValuesArray\.OrderBy\(p=>p\)\.Skip\(1\)\.Take\(1\)\.ToArray\(\)"
    )]
    public void QueryMethods_NestedNestedPrimitiveCollection_ShouldProduceCorrectExpression(string query, string pattern)
    {
        string actual = Get<Forest, ForestModel>(this.queryable, query).GetString();
        Assert.Matches(pattern, actual);
    }

    [Theory]
    [InlineData
    (
        "/forest?$select=DomainControllers($orderby=DateAdded)",
        @"DomainControllers=dtoForest\.DomainControllers\.Select\(.*?\)\.OrderBy\(it=>it\.DateAdded\)\.ToList\(\)"
    )]
    [InlineData
    (
        "/forest?$select=DomainControllers($orderby=DateAdded desc)",
        @"DomainControllers=dtoForest\.DomainControllers\.Select\(.*?\)\.OrderByDescending\(it=>it\.DateAdded\)\.ToList\(\)"
    )]
    [InlineData
    (
        "/forest?$select=DomainControllers($orderby=DateAdded desc;$skip=1;$top=1)",
        @"DomainControllers=dtoForest\.DomainControllers\.Select\(.*?\)\.OrderByDescending\(it=>it\.DateAdded\)\.Skip\(1\)\.Take\(1\)\.ToList\(\)"
    )]
    [InlineData
    (
        "/forest?$select=DomainControllers($skip=1;$top=1)",
        @"DomainControllers=dtoForest\.DomainControllers\.Select\(.*?\)\.OrderBy\(a=>a\.DateAdded\)\.Skip\(1\)\.Take\(1\)\.ToList\(\)"
    )]
    public void QueryMethods_NestedComplexCollection_ShouldProduceCorrectExpression(string query, string pattern)
    {
        string actual = Get<Forest, ForestModel>(this.queryable, query).GetString();
        Assert.Matches(pattern, actual);
    }

    [Theory]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($select=FsmoRoles($orderby=$this))",
        @"FsmoRoles=\(\(FsmoRoleModel\[]\)dtoDomainControllerEntry\.Entry\.Dc\.FsmoRoles\)\.OrderBy\(p=>p\)\.ToArray\(\)"
    )]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($select=FsmoRoles($orderby=$this;$top=1;$skip=1))",
        @"FsmoRoles=\(\(FsmoRoleModel\[]\)dtoDomainControllerEntry\.Entry\.Dc\.FsmoRoles\)\.OrderBy\(p=>p\)\.Skip\(1\)\.Take\(1\)\.ToArray\(\)"
    )]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($select=FsmoRoles($orderby=$this desc;$top=1;$skip=1))",
        @"FsmoRoles=\(\(FsmoRoleModel\[]\)dtoDomainControllerEntry\.Entry\.Dc\.FsmoRoles\)\.OrderByDescending\(p=>p\)\.Skip\(1\)\.Take\(1\)\.ToArray\(\)"
    )]
    [InlineData
    (
        "/forest?$expand=DomainControllers/Entry/Dc($select=FsmoRoles($top=1;$skip=1))",
        @"FsmoRoles=\(\(FsmoRoleModel\[]\)dtoDomainControllerEntry\.Entry\.Dc\.FsmoRoles\)\.OrderBy\(p=>p\)\.Skip\(1\)\.Take\(1\)\.ToArray\(\)"
    )]
    public void QueryMethod_NestedEnumCollection_ShouldProduceCorrectExpressions(string query, string pattern)
    {
        string actual = Get<Forest, ForestModel>(this.queryable, query).GetString();
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