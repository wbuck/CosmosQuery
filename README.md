# CosmosQuery

[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/CosmosQuery?color=blue&style=for-the-badge)](https://www.nuget.org/packages/CosmosQuery)

Creates LINQ expressions from `ODataQueryOptions` and executes the query.
This library uses [AutoMapper][AutoMapper]'s [Queryable Extentions](https://docs.automapper.org/en/stable/Queryable-Extensions.html) to project from your `DTO` (Data Transfer Object) to your entities.

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

And thats it, you're done. The query is mapped to your entitiy type before query execution.
<br>
The query and results are correctly mapped to and from your `DTO` type(s) and your `Entity` type(s). This keeps your entities from beging publically exposed via the API.

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

