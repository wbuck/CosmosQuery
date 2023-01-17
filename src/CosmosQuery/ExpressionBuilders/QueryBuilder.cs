using CosmosQuery.Extensions;
using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System.Reflection;

namespace CosmosQuery.ExpressionBuilders;

internal static class QueryBuilder
{
    public static Expression<Func<IQueryable<T>, IQueryable<T>>>? GetQueryableExpression<T>(this ODataQueryOptions<T> options, ODataSettings? settings = null)
    {
        if (options.NoQueryableMethod(settings))
            return null;

        ParameterExpression param = Expression.Parameter(typeof(IQueryable<T>), "q");

        return param.GetOrderByMethod(options, settings) switch
        {
            Expression body => Expression.Lambda<Func<IQueryable<T>, IQueryable<T>>>(body, param),
            _ => null
        };
    }

    public static Expression? GetQueryableMethod(this Expression expression,
        ODataQueryContext context, OrderByClause? orderByClause, Type type, int? skip, int? top)
    {
        if (orderByClause is null && skip is null && top is null)
            return null;

        if (orderByClause is null && (skip is not null || top is not null))
        {
            var orderBySettings = context.FindSortableProperties(type);

            if (orderBySettings is null)
                return null;

            return expression
                .GetDefaultOrderByCall(orderBySettings)
                .GetSkipCall(skip)
                .GetTakeCall(top);
        }

        return expression
            .GetOrderByCall(orderByClause!, context)
            .GetSkipCall(skip)
            .GetTakeCall(top);
    }

    public static Expression? GetOrderByMethod<T>(this Expression expression,
            ODataQueryOptions<T> options, ODataSettings? settings = null)
    {
        if (options.NoQueryableMethod(settings))
            return null;

        return expression.GetQueryableMethod
        (
            options.Context,
            options.OrderBy?.OrderByClause,
            typeof(T),
            options.Skip?.Value,
            GetPageSize()
        );

        int? GetPageSize() => (settings?.PageSize, options.Top) switch
        {
            (null, null) => null,
            (int size, null) => size,
            (null, TopQueryOption top) => top.Value,
            (int size, TopQueryOption top) => top.Value < size ? top.Value : size,
        };
    }

    private static Expression GetDefaultOrderByCall(this Expression expression, OrderBySetting settings)
    {
        return settings.ThenBy is null
            ? GetMethodCall()
            : GetMethodCall().GetDefaultThenByCall(settings.ThenBy);

        Expression GetMethodCall() =>
            expression.GetOrderByCall(settings.Name, nameof(Queryable.OrderBy));
    }

    private static Expression GetDefaultThenByCall(this Expression expression, OrderBySetting settings)
    {
        return settings.ThenBy is null
            ? GetMethodCall()
            : GetMethodCall().GetDefaultThenByCall(settings.ThenBy);

        Expression GetMethodCall() =>
            expression.GetOrderByCall(settings.Name, nameof(Queryable.ThenBy));
    }

    private static Expression GetOrderByCall(this Expression expression, string memberFullName, string methodName, string selectorParameterName = "a")
    {
        Type sourceType = expression.GetUnderlyingElementType();
        MemberInfo memberInfo = sourceType.GetMemberInfoFromFullName(memberFullName);
        return Expression.Call
        (
            expression.Type.IsIQueryable() ? typeof(Queryable) : typeof(Enumerable),
            methodName,
            new Type[] { sourceType, memberInfo.GetMemberType() },
            expression,
            memberFullName.GetTypedSelector(sourceType, selectorParameterName)
        );
    }

    private static Expression GetOrderByCountCall(this Expression expression, CountNode countNode, string methodName, ODataQueryContext context, string selectorParameterName = "a")
    {
        Type sourceType = expression.GetUnderlyingElementType();
        ParameterExpression param = Expression.Parameter(sourceType, selectorParameterName);

        Expression countSelector;

        if (countNode.FilterClause is not null)
        {
            string memberFullName = countNode.GetPropertyPath();
            Type filterType = sourceType.GetMemberInfoFromFullName(memberFullName).GetMemberType().GetUnderlyingElementType();
            LambdaExpression filterExpression = countNode.FilterClause.GetFilterExpression(filterType, context);
            countSelector = param.MakeSelector(memberFullName).GetCountCall(filterExpression);
        }
        else
        {
            countSelector = param.MakeSelector(countNode.GetPropertyPath()).GetCountCall();
        }

        return Expression.Call
        (
            expression.Type.IsIQueryable() ? typeof(Queryable) : typeof(Enumerable),
            methodName,
            new Type[] { sourceType, countSelector.Type },
            expression,
            param.MakeLambdaExpression
            (
                countSelector
            )
        );
    }

    private static Expression GetThenByCall(this Expression expression, OrderByClause orderByClause, ODataQueryContext context)
    {
        const string ThenBy = "ThenBy";
        const string ThenByDescending = "ThenByDescending";

        return orderByClause.ThenBy == null
            ? GetMethodCall()
            : GetMethodCall().GetThenByCall(orderByClause.ThenBy, context);

        Expression GetMethodCall()
        {
            return orderByClause.Expression switch
            {
                CountNode countNode => expression.GetOrderByCountCall
                (
                    countNode,
                    orderByClause.Direction == OrderByDirection.Ascending
                        ? ThenBy
                        : ThenByDescending,
                    context
                ),
                SingleValuePropertyAccessNode propertyNode => expression.GetOrderByCall
                (
                    propertyNode.GetPropertyPath(),
                    orderByClause.Direction == OrderByDirection.Ascending
                        ? ThenBy
                        : ThenByDescending
                ),
                _ => throw new ArgumentException($"Unsupported SingleValueNode value: {orderByClause.Expression.GetType()}"),
            };
        }
    }

    private static Expression GetOrderByCall(this Expression expression, OrderByClause orderByClause, ODataQueryContext context)
    {
        const string OrderBy = "OrderBy";
        const string OrderByDescending = "OrderByDescending";

        return orderByClause.ThenBy == null
            ? GetMethodCall()
            : GetMethodCall().GetThenByCall(orderByClause.ThenBy, context);

        Expression GetMethodCall()
        {
            SingleValueNode orderByNode = orderByClause.Expression;
            switch (orderByNode)
            {
                case CountNode countNode:
                    return expression.GetOrderByCountCall
                    (
                        countNode,
                        orderByClause.Direction == OrderByDirection.Ascending
                            ? OrderBy
                            : OrderByDescending,
                        context,
                        orderByClause.RangeVariable.Name
                    );
                default:
                    SingleValuePropertyAccessNode propertyNode = (SingleValuePropertyAccessNode)orderByNode;
                    return expression.GetOrderByCall
                    (
                        propertyNode.GetPropertyPath(),
                        orderByClause.Direction == OrderByDirection.Ascending
                            ? OrderBy
                            : OrderByDescending,
                        orderByClause.RangeVariable.Name
                    );
            }
        }
    }

    private static Expression GetSkipCall(this Expression expression, int? skip)
    {
        if (skip == null) return expression;

        return Expression.Call
        (
            expression.Type.IsIQueryable() ? typeof(Queryable) : typeof(Enumerable),
            "Skip",
            new[] { expression.GetUnderlyingElementType() },
            expression,
            Expression.Constant(skip.Value)
        );
    }

    private static Expression GetTakeCall(this Expression expression, int? top)
    {
        if (top == null) return expression;

        return Expression.Call
        (
            expression.Type.IsIQueryable() ? typeof(Queryable) : typeof(Enumerable),
            "Take",
            new[] { expression.GetUnderlyingElementType() },
            expression,
            Expression.Constant(top.Value)
        );
    }

    public static bool NoQueryableMethod(this ODataQueryOptions options, ODataSettings? settings)
        => options.OrderBy is null
        && options.Top is null
        && options.Skip is null
        && settings?.PageSize is null;

    public static OrderBySetting? FindSortableProperties(this ODataQueryContext context, Type type)
    {
        context = context ?? throw new ArgumentNullException(nameof(context));

        var entity = GetEntity();
        return entity is not null
            ? FindProperties(entity)
            : throw new InvalidOperationException($"The type '{type.FullName}' has not been declared in the entity data model.");

        IEdmEntityType? GetEntity()
        {
            List<IEdmEntityType> entities = context.Model.SchemaElements.OfType<IEdmEntityType>().Where(e => e.Name == type.Name).ToList();
            if (entities.Count == 1)
                return entities[0];

            return null;
        }

        static OrderBySetting? FindProperties(IEdmEntityType entity)
        {
            var propertyNames = entity.Key().Any() switch
            {
                true => entity.Key().Select(k => k.Name),
                false => entity.StructuralProperties()
                    .Where(p => p.Type.IsPrimitive() && !p.Type.IsStream())
                    .Select(p => p.Name)
                    .OrderBy(n => n)
                    .Take(1)
            };
            var orderBySettings = new OrderBySetting();
            propertyNames.Aggregate(orderBySettings, (settings, name) =>
            {
                if (settings.Name is null)
                {
                    settings.Name = name;
                    return settings;
                }
                settings.ThenBy = new() { Name = name };
                return settings.ThenBy;
            });
            return orderBySettings.Name is null ? null : orderBySettings;
        }

    }
}
