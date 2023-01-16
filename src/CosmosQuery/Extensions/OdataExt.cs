using CosmosQuery.Query;
using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser;

namespace CosmosQuery.Extensions;

internal static class OdataExt
{
    public static List<List<PathSegment>> GetSelects<TModel>(this ODataQueryOptions<TModel> options)
    {
        throw new NotImplementedException();
    }

    public static List<List<PathSegment>> GetExpansions<TModel>(this ODataQueryOptions<TModel> options)
    {
        throw new NotImplementedException();
    }

    public static LambdaExpression GetFilterExpression(this FilterClause clause, Type type, ODataQueryContext context)
    {
        var parameters = new Dictionary<string, ParameterExpression>();
        return new FilterHelper(parameters, context)
            .GetFilterPart(clause.Expression)
            .GetFilter(type, parameters, clause.RangeVariable.Name);
    }

    public static Expression<Func<TModel, bool>>? ToFilterExpression<TModel>(
        this ODataQueryOptions<TModel> options,
        HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default,
        TimeZoneInfo? timeZone = null)
    {
        if (options is null || options.Filter is null && options.Search is null)        
            return null;

        var parameter = Expression.Parameter(typeof(TModel), "$it");

        Expression? filterExpression = null;
        if (options.Filter is not null)
        {
            var lambda = options.Filter.ToFilterExpression<TModel>(handleNullPropagation, timeZone);
            if (lambda is not null)
                filterExpression = lambda.Body.ReplaceParameter(lambda.Parameters[0], parameter);
        }

        Expression? searchExpression = null;
        if (options.Search is not null)
        {
            var lambda = options.Search.ToSearchExpression<TModel>(handleNullPropagation, timeZone);
            if (lambda is not null)
                searchExpression = lambda.Body.ReplaceParameter(lambda.Parameters[0], parameter);
        }

        Expression? finalExpression = null;
        if (filterExpression is not null && searchExpression is not null)        
            finalExpression = Expression.AndAlso(searchExpression, filterExpression);

        finalExpression ??= filterExpression ?? searchExpression;

        if (finalExpression is not null)
            return Expression.Lambda<Func<TModel, bool>>(finalExpression, parameter);

        return null;
    }

    private static Expression<Func<TModel, bool>>? ToFilterExpression<TModel>(
        this FilterQueryOption? filterOption, 
        HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default, 
        TimeZoneInfo? timeZone = null)
    {
        if (filterOption is null)
            return null;

        IQueryable queryable = Enumerable.Empty<TModel>().AsQueryable();

        queryable = filterOption.ApplyTo(queryable, new()
        {
            HandleNullPropagation = handleNullPropagation, 
            TimeZone = timeZone 
        });

        MethodCallExpression whereMethodCallExpression = (MethodCallExpression)queryable.Expression;
        return (Expression<Func<TModel, bool>>?)(whereMethodCallExpression.Arguments[1].UnQuote() as LambdaExpression);
    }

    private static Expression<Func<TModel, bool>>? ToSearchExpression<TModel>(
        this SearchQueryOption? searchOption,
        HandleNullPropagationOption handleNullPropagation = HandleNullPropagationOption.Default,
        TimeZoneInfo? timeZone = null)
    {
        if (searchOption is null)
            return null;

        IQueryable queryable = Enumerable.Empty<TModel>().AsQueryable();
        queryable = searchOption.ApplyTo(queryable, new()
        {
            HandleNullPropagation = handleNullPropagation,
            TimeZone = timeZone
        });

        MethodCallExpression whereMethodCallExpression = (MethodCallExpression)queryable.Expression;
        return (Expression<Func<TModel, bool>>?)(whereMethodCallExpression.Arguments[1].UnQuote() as LambdaExpression);
    }

    public static void AddExpandOptionsResult(this ODataQueryOptions options)
    {
        if (options.SelectExpand is null)
            return;

        options.Request.ODataFeature().SelectExpandClause = options.SelectExpand.SelectExpandClause;
    }

    public static void AddCountOptionsResult(this ODataQueryOptions options, long longCount)
    {
        if (options.Count?.Value != true)
            return;

        options.Request.ODataFeature().TotalCount = longCount;
    }

    public static void AddNextLinkOptionsResult(this ODataQueryOptions options, int pageSize)
    {
        if (options.Request is null)
            return;

        options.Request.ODataFeature().NextLink = options.Request.GetNextPageLink(pageSize, null, null);
    }
}
