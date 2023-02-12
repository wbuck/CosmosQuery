# CosmosQuery

![CI](https://github.com/wbuck/cosmosquery/actions/workflows/ci.yml/badge.svg)
[![Nuget (with prereleases)](https://img.shields.io/nuget/v/CosmosQuery)](https://www.nuget.org/packages/CosmosQuery)

CosmosQuery generates an `Expression` tree from the supplied `ODataQueryOptions`.
This library uses [AutoMapper][AutoMapper]'s [Queryable Extentions](https://docs.automapper.org/en/stable/Queryable-Extensions.html) in conjunction with custom `Expression` builders to generate an `Expression` tree which can be parsed by the Cosmos DB LINQ provider. Furthermore, because [AutoMapper][AutoMapper] is used for query projection you do not have to expose your entity types and can instead use DTO’s (Data Transfer Object) in your public facing API.

<br>

Where this library excels is how it deals with complex types. Currently `OData` does not provide a means of `$expand`ing complex types. The consequence of this when using something like `EFCore` is that your complex data members will be `null` after performing a query unless the consumer of your API explicitly `$select`’s said data members. This can quickly become cumbersome when dealing with complex documents.

<br>

What CosmosQuery does instead is treat complex types as just another property of your entity type. In other words, all complex types are automatically expanded and pulled from the database. The data being pulled from the DB can still be controlled using the `$select` operator.

## Supported Operations

Currently CosmosQuery supports the following OData operations in both a query and subquery:
1.	`$select`
2.	`$filter`
3.	`$orderby`
4.	`$top`
5.	`$skip`

Although this library currently supports the use of `$orderby`, `$top`, and `$skip` within a subquery Cosmos DB does **not**.

## Usage

### <b>Step1</b>
<hr>

Set up your `DTO` to `Entity` mappings for [AutoMapper][AutoMapper] so that it can correctly project the incoming `OData` query from your `DTO` type(s) to your `Entity` type(s).

<br>

If you only want to include properties explicitly (E.g., the consumer of the API has to explicitly `$select` which properties they want included in the results) then make sure to enable [Explicit expansion](https://docs.automapper.org/en/stable/Queryable-Extensions.html?highlight=explicitexpansions#explicit-expansion) on your properties.


```c#
    public class Mappings : AutoMapper.Profile
    {
        public Mappings()
        {
            CreateMap<Entity, DTO>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.FullName))                
                .ForAllMembers(o => o.ExplicitExpansion());
        }
    }
```

### <b>Step2</b>
<hr>

Set up the actions on your controller(s) to accept a `ODataQueryOptions`.
<br>
**Do not** decorate your actions with the `[EnableQuery]` attribute as this will result in some operations being applied more than once.

```c#
public class MyController : ODataController
{
    private readonly CosmosClient _client;
    private readonly IMapper _mapper;

    public MyController(CosmosClient client, IMapper mapper)
    {
        _client = client;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Get(ODataQueryOptions<DTO> options)
    {
        var container = _client.GetContainer("DatabaseID", "ContainerID");
        var query = container.GetItemLinqQueryable<Entity>();
        return Ok(await query.GetQueryAsync(_mapper, options));
    }
}
```

And thats it, you're done!
<br>
The query and results are correctly mapped to and from your `DTO` and `Entity` type(s). This keeps your entities from being publically exposed via the API.

## Functions

There are four main functions you can use depending on your usecase.
-   `Get` - Executes the query aginst the database synchronously
-   `GetAsync` - Executes the query against the database asynchronously
-   `GetQuery` - Synchronously builds the `IQueryable` but does not execute it against the database
-   `GetQueryAsync` - Asynchronously builds the `IQueryable` but does not execute it against the database

<b>If you plan on using the synchronous versions of the functions above make sure you've enabled synchronous execution:</b>

```c#
var container = _client.GetContainer("DatabaseID", "ContainerID");
var query = container.GetItemLinqQueryable<Entity>(allowSynchronousQueryExecution: true);
```

### Function Signatures

```c#
public static ICollection<TModel> Get<TModel, TData>(
    this IQueryable<TData> query, 
    IMapper mapper, 
    ODataQueryOptions<TModel> options, 
    QuerySettings? querySettings = null);

public static IQueryable<TModel> GetQuery<TModel, TData>(
    this IQueryable<TData> query,
    IMapper mapper,
    ODataQueryOptions<TModel> options,
    QuerySettings? querySettings = null);

public static async Task<ICollection<TModel>> GetAsync<TModel, TData>(
    this IQueryable<TData> query, 
    IMapper mapper, 
    ODataQueryOptions<TModel> options, 
    QuerySettings? querySettings = null);

public static async Task<IQueryable<TModel>> GetQueryAsync<TModel, TData>(
    this IQueryable<TData> query, 
    IMapper mapper, 
    ODataQueryOptions<TModel> options, 
    QuerySettings? querySettings = null);
```

[AutoMapper]: https://github.com/AutoMapper/AutoMapper

