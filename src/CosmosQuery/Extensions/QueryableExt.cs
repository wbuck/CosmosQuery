using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using CosmosQuery.Extensions;
using CosmosQuery.Query;
using CosmosQuery.Visitors;
using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Azure.Cosmos.Linq;

namespace CosmosQuery;

public static class QueryableExt
{
    public static ICollection<TModel> ExecuteQuery<TModel, TEntity>(
        this IQueryable<TEntity> query, 
        IMapper mapper, 
        ODataQueryOptions<TModel> options,
        QuerySettings? settings = null) where TModel : class
    {
        IQueryable<TModel> modelQuery = GetQuery(query, mapper, options, settings);
        return modelQuery.ToArray();
    }    

    public static IQueryable<TModel> GetQuery<TModel, TEntity>(
        this IQueryable<TEntity> query,
        IMapper mapper,
        ODataQueryOptions<TModel> options,
        QuerySettings? settings = null) where TModel : class
    {
        ArgumentNullException.ThrowIfNull(nameof(query));
        ArgumentNullException.ThrowIfNull(nameof(mapper));
        ArgumentNullException.ThrowIfNull(nameof(options));

        Expression<Func<TModel, bool>>? filter = options.ToFilterExpression(
            settings?.ODataSettings?.HandleNullPropagation ?? HandleNullPropagationOption.False,
            settings?.ODataSettings?.TimeZone);

        ApplyOptions(options, settings);

        query.ApplyCountQuery(mapper, filter, options);
        return query.GetQueryable(mapper, options, settings, filter);
    }



    public static async Task<ICollection<TModel>> ExecuteQueryAsync<TModel, TEntity>(
        this IQueryable<TEntity> query,
        IMapper mapper,
        ODataQueryOptions<TModel> options,
        QuerySettings? settings = null,
        CancellationToken cancellationToken = default) where TModel : class
    {
        IQueryable<TModel> modelQuery = await 
            GetQueryAsync(query, mapper, options, settings, cancellationToken).ConfigureAwait(false);

        return await modelQuery.ExecuteQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async Task<IQueryable<TModel>> GetQueryAsync<TModel, TEntity>(
        this IQueryable<TEntity> query,
        IMapper mapper,
        ODataQueryOptions<TModel> options,
        QuerySettings? settings = null,
        CancellationToken cancellationToken = default) where TModel : class
    {
        ArgumentNullException.ThrowIfNull(nameof(query));
        ArgumentNullException.ThrowIfNull(nameof(mapper));
        ArgumentNullException.ThrowIfNull(nameof(options));

        Expression<Func<TModel, bool>>? filter = options.ToFilterExpression(
            settings?.ODataSettings?.HandleNullPropagation ?? HandleNullPropagationOption.False,
            settings?.ODataSettings?.TimeZone);

        ApplyOptions(options, settings);

        await query.ApplyCountQueryAsync(mapper, filter, options, cancellationToken).ConfigureAwait(false);
        return query.GetQueryable(mapper, options, settings, filter);
    }

    private static async Task<ICollection<TModel>> ExecuteQueryAsync<TModel>(
        this IQueryable<TModel> query, CancellationToken cancellationToken = default)
    {
        using var iterator = query.ToFeedIterator();
        return
        (
            await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false)
        ).Resource.ToArray();
    }

    private static IQueryable<TModel> GetQueryable<TModel, TEntity>(
        this IQueryable<TEntity> query,
        IMapper mapper,
        ODataQueryOptions<TModel> options,
        QuerySettings? settings,
        Expression<Func<TModel, bool>>? filter)
        where TModel : class
    {
        var selects = options.GetSelects();
        var expansions = options.GetExpansions();

        throw new NotImplementedException();
    }

    private static IQueryable<TModel> GetQueryable<TModel, TEntity>(
        this IQueryable<TEntity> query,
        IMapper mapper,
        Expression<Func<TModel, bool>>? filter = null,
        Expression<Func<IQueryable<TModel>, IQueryable<TModel>>>? queryFunc = null,
        IEnumerable<Expression<Func<TModel, object>>>? includeProperties = null,
        ProjectionSettings? projectionSettings = null)
    {
        Expression<Func<TEntity, bool>> entityFilter = 
            mapper.MapExpression<Expression<Func<TEntity, bool>>>(filter);

        Func<IQueryable<TEntity>, IQueryable<TEntity>>? mappedQueryFunc =
            mapper.MapExpression<Expression<Func<IQueryable<TEntity>, IQueryable<TEntity>>>>(queryFunc)?.Compile();

        if (filter is not null)
            query = query.Where(entityFilter);

        return mappedQueryFunc is not null
                ? mapper.ProjectTo(mappedQueryFunc(query), projectionSettings?.Parameters, GetIncludes())
                : mapper.ProjectTo(query, projectionSettings?.Parameters, GetIncludes());

        Expression<Func<TModel, object>>[] GetIncludes() =>
            includeProperties?.ToArray() ?? Array.Empty<Expression<Func<TModel, object>>>();
    }

    private static void ApplyOptions<TModel>(ODataQueryOptions<TModel> options, QuerySettings? settings)
    {
        options.AddExpandOptionsResult();
        if (settings?.ODataSettings?.PageSize.HasValue == true)
            options.AddNextLinkOptionsResult(settings.ODataSettings.PageSize.Value);
    }

    private static void ApplyCountQuery<TModel, TEntity>(
        this IQueryable<TEntity> query,
        IMapper mapper, 
        Expression<Func<TModel, bool>>? filter, 
        ODataQueryOptions<TModel> options)
    {
        if (options.Count?.Value == true)
        {
            options.AddCountOptionsResult
            (
                query.QueryCount(mapper, filter)
            );
        }
    }

    private static int QueryCount<TModel, TEntity>(
        this IQueryable<TEntity> query,
        IMapper mapper, 
        Expression<Func<TModel, bool>>? filter)
    {
        if (filter is not null)
        {
            query = query.Where
            (
                mapper.MapExpression<Expression<Func<TEntity, bool>>>(filter)
            );
        }
        return query.Count();
    }

    private static async Task ApplyCountQueryAsync<TModel, TEntity>(
        this IQueryable<TEntity> query,
        IMapper mapper,
        Expression<Func<TModel, bool>>? filter,
        ODataQueryOptions<TModel> options,
        CancellationToken cancellationToken)
    {
        if (options.Count?.Value == true)
        {
            options.AddCountOptionsResult
            (
                await query.QueryCountAsync(mapper, filter, cancellationToken)
                    .ConfigureAwait(false)
            );
        }
    }

    private static async Task<int> QueryCountAsync<TModel, TEntity>(
        this IQueryable<TEntity> query,
        IMapper mapper,
        Expression<Func<TModel, bool>>? filter,
        CancellationToken cancellationToken)
    {
        if (filter is not null)
        {
            query = query.Where
            (
                mapper.MapExpression<Expression<Func<TEntity, bool>>>(filter)
            );
        }
        return (await query.CountAsync(cancellationToken).ConfigureAwait(false)).Resource;
    }

    private static IQueryable<TModel> ApplyFilters<TModel>(
            this IQueryable<TModel> query, List<List<PathSegment>> selects, ODataQueryContext context)
    {
        List<List<PathSegment>> memberFilters = GetMemberFilters();
        List<List<PathSegment>> collectionFilters = GetMemberCollectionFilters();

        //List<List<ODataExpansionOptions>> methods = new();// GetQueryMethods();

        if (!memberFilters.Any() && !collectionFilters.Any() /*&& !methods.Any()*/)
            return query;

        Expression expression = query.Expression;

        //if (methods.Any())
        //    expression = UpdateProjectionMethodExpression(expression);

        memberFilters.ForEach
        (
            segments => expression = MemberFilterAppender.AppendFilters(expression, segments, context)
        );

        collectionFilters.ForEach
        (
            segments => expression = MemberCollectionFilterAppender.AppendFilters(expression, segments, context)
        );

        return query.Provider.CreateQuery<TModel>(expression);

        List<List<PathSegment>> GetMemberFilters() =>
            selects.Where(s => !s.Last().MemberType.IsList()).ToList();

        List<List<PathSegment>> GetMemberCollectionFilters() =>
            selects.Where(s => s.Last().MemberType.IsList()).ToList();
    }
}
