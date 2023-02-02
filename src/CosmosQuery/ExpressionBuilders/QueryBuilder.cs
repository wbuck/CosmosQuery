using AutoMapper.Internal;
using CosmosQuery.Extensions;
using CosmosQuery.Query;
using CosmosQuery.Visitors;
using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System.Reflection;
using AmExt = AutoMapper.Internal.TypeExtensions;
using LbExt = LogicBuilder.Expressions.Utils.TypeExtensions;

namespace CosmosQuery.ExpressionBuilders;

internal static class QueryBuilder
{
    public static Expression? GetQueryableExpression(this Expression expression, IReadOnlyList<PathSegment> pathSegments, QueryOptions options, ODataQueryContext context)
        => QueryMethodInserter.Insert(pathSegments, options, context, expression);

    public static LambdaExpression GetSelector(this OrderByClause clause, IReadOnlyList<PathSegment> pathSegments)
    {
        Type elementType = pathSegments[0].ElementType;
        ParameterExpression parameter = Expression.Parameter(elementType, clause.RangeVariable.Name.Replace("$", string.Empty));

        Expression memberExpression = pathSegments.Skip(1).Aggregate((Expression)parameter, (expression, next)
            => Expression.MakeMemberAccess(expression, next.Member));

        string[] properties = clause.Expression.GetPropertyPath().Split('.');

        memberExpression = properties.Aggregate(memberExpression, (expression, next)
            => Expression.MakeMemberAccess(expression, AmExt.GetFieldOrProperty(expression.Type, next)));

        return Expression.Lambda
        (
            memberExpression,
            parameter
        );
    }

    private static string GetPropertyPath(this SingleValueNode node) => node switch
    {
        CountNode countNode => countNode.GetPropertyPath(),
        _ => ((SingleValuePropertyAccessNode)node).GetPropertyPath()
    };

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
            ODataQueryContext context, OrderByClause orderByClause, Type type, int? skip, int? top)
    {
        if (orderByClause is null && skip is null && top is null)
            return null;

        if (orderByClause is null && (skip is not null || top is not null))
        {
            if (type.IsLiteralType())
            {
                return expression
                    .GetPrimitiveOrderByCall(orderByClause)
                    .GetSkipCall(skip)
                    .GetTakeCall(top);
            }

            var orderBySettings = context.FindSortableProperties(type);

            if (orderBySettings is null)
                return null;

            return expression
                .GetDefaultOrderByCall(orderBySettings)
                .GetSkipCall(skip)
                .GetTakeCall(top);
        }

        return expression
            .GetOrderByCall(orderByClause, context)
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

    public static Expression GetOrderByCall(this Expression expression, string memberFullName, string methodName, string selectorParameterName = "a")
    {
        Type sourceType = expression.GetUnderlyingElementType();
        MemberInfo memberInfo = sourceType.GetMemberInfoFromFullName(memberFullName);
        return Expression.Call
        (
            expression.Type.IsIQueryable() ? typeof(Queryable) : typeof(Enumerable),
            methodName,
            new Type[] { sourceType, LbExt.GetMemberType(memberInfo) },
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
            Type filterType = LbExt.GetMemberType(sourceType.GetMemberInfoFromFullName(memberFullName)).GetUnderlyingElementType();
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
                case NonResourceRangeVariableReferenceNode:
                    return expression.GetPrimitiveOrderByCall
                    (
                        orderByClause.Direction == OrderByDirection.Ascending
                            ? OrderBy
                            : OrderByDescending
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
                NonResourceRangeVariableReferenceNode => expression.GetPrimitiveOrderByCall
                (
                    orderByClause.Direction == OrderByDirection.Ascending
                        ? ThenBy
                        : ThenByDescending
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

    public static Expression GetSkipCall(this Expression expression, int? skip)
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

    public static Expression GetTakeCall(this Expression expression, int? top)
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

    private static Expression GetPrimitiveOrderByCall(this Expression expression, OrderByClause orderByClause = null)
    {
        const string OrderBy = nameof(Enumerable.OrderBy);
        const string OrderByDescending = nameof(Enumerable.OrderByDescending);

        return orderByClause?.ThenBy is null
            ? GetMethodCall()
            : GetMethodCall().GetPrimitiveThenByCall(orderByClause.ThenBy);

        Expression GetMethodCall()
            => expression.GetPrimitiveOrderByCall
            (
                orderByClause is null
                    ? OrderBy
                    : GetDirection(orderByClause.Direction)
            );

        string GetDirection(OrderByDirection direction)
            => direction == OrderByDirection.Ascending ? OrderBy : OrderByDescending;
    }

    private static Expression GetPrimitiveThenByCall(this Expression expression, OrderByClause orderByClause)
    {
        const string ThenBy = nameof(Enumerable.ThenBy);
        const string ThenByDescending = nameof(Enumerable.ThenByDescending);

        return orderByClause.ThenBy is null
            ? GetMethodCall()
            : GetMethodCall().GetPrimitiveThenByCall(orderByClause.ThenBy);

        Expression GetMethodCall()
            => expression.GetPrimitiveOrderByCall
            (
                 orderByClause.Direction == OrderByDirection.Ascending
                    ? ThenBy
                    : ThenByDescending
            );
    }

    public static Expression GetPrimitiveOrderByCall(this Expression expression, string methodName)
    {
        Type elementType = expression.Type.GetUnderlyingElementType();
        ParameterExpression parameter = Expression.Parameter(elementType, "p");

        return Expression.Call
        (
            expression.Type.IsIQueryable() ? typeof(Queryable) : typeof(Enumerable),
            methodName,
            new[] { elementType, elementType },
            expression,
            Expression.Lambda(parameter, parameter)
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

        IEdmSchemaElement? schemaElement = GetSchemaElement();

        if (schemaElement is null)
            throw new InvalidOperationException($"The type '{type.FullName}' has not been declared in the entity data model.");

        return FindProperties(schemaElement);


        IEdmSchemaElement? GetSchemaElement() =>
            context.Model.SchemaElements
                .FirstOrDefault(e => (e is IEdmEntityType || e is IEdmComplexType) && e.Name == type.Name);

        static OrderBySetting? FindProperties(IEdmSchemaElement schemaElement)
        {
            var propertyNames = schemaElement switch
            {
                IEdmEntityType entityType when entityType.ContainsKey() => entityType.Key().Select(p => p.Name),
                IEdmStructuredType structuredType => structuredType.GetSortableProperties().Take(1),
                _ => throw new NotSupportedException("The EDM element type is not supported.")
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

    private static bool ContainsKey(this IEdmEntityType entityType)
        => entityType.Key().Any();

    private static IEnumerable<string> GetSortableProperties(this IEdmStructuredType structuredType)
        => structuredType.StructuralProperties()
            .Where(p => p.Type.IsPrimitive() && !p.Type.IsStream())
            .Select(p => p.Name)
            .OrderBy(n => n);
}
