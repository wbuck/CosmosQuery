using AutoMapper.Internal;
using CosmosQuery.Cache;
using CosmosQuery.Visitors;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser;
using System.Linq.Expressions;

namespace CosmosQuery.Extensions;
internal static class ExpressionExt
{
    public static Expression GetQueryableExpression(this Expression expression, IReadOnlyList<PathSegment> pathSegments, QueryOptions options, ODataQueryContext context)
        => QueryMethodInserter.Insert(pathSegments, options, context, expression);

    public static LambdaExpression GetSelector(this OrderByClause clause, IReadOnlyList<PathSegment> pathSegments)
    {
        Type elementType = pathSegments[0].ElementType;
        ParameterExpression parameter = Expression.Parameter(elementType, clause.RangeVariable.Name.Replace("$", string.Empty));

        Expression memberExpression = parameter.GetMemeberAccessExpression(clause.Expression, pathSegments);

        return Expression.Lambda
        (
            memberExpression,
            parameter
        );
    }

    public static Expression GetMemeberAccessExpression(this ParameterExpression parameter, SingleValueNode node, IReadOnlyList<PathSegment> pathSegments)
    {
        Expression memberExpression = pathSegments.Skip(1).Aggregate((Expression)parameter, (expression, next)
            => Expression.MakeMemberAccess(expression, next.Member));

        string[] properties = node.GetPropertyPath().Split('.');

        memberExpression = properties.Aggregate(memberExpression, (expression, next)
            => Expression.MakeMemberAccess(expression, expression.Type.GetFieldOrProperty(next)));

        return memberExpression;
    }

    public static Expression Unquote(this Expression exp)
        => exp.NodeType == ExpressionType.Quote
            ? ((UnaryExpression)exp).Operand.Unquote()
            : exp;

    public static MethodCallExpression ToListCall(this Expression expression, Type elementType)
        => Expression.Call
           (
               LinqMethods.EnumerableToListMethod.MakeGenericMethod(elementType),
               expression
           );


    public static MethodCallExpression ToArrayCall(this Expression expression, Type elementType)
        => Expression.Call
           (
               LinqMethods.EnumerableToArrayMethod.MakeGenericMethod(elementType),
               expression
           );

    private static string GetPropertyPath(this SingleValueNode node) => node switch
    {
        CountNode countNode => countNode.GetPropertyPath(),
        _ => ((SingleValuePropertyAccessNode)node).GetPropertyPath()
    };
}
