using CosmosQuery.Visitors;
using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser;
using System.Reflection;

namespace CosmosQuery.Extensions;

internal static class ExpressionExt
{
    public static Expression ReplaceParameter(this Expression expression, ParameterExpression source, Expression target) =>
        new ParameterReplacer(source, target).Visit(expression);

    public static Expression UnQuote(this Expression expression) =>
        expression.NodeType switch
        {
            ExpressionType.Quote => ((UnaryExpression)expression).Operand.UnQuote(),
            _ => expression,
        };

    public static MethodCallExpression ToListCall(this Expression expression, Type elementType) =>
        Expression.Call
        (
            LinqMethods.EnumerableToListMethod.MakeGenericMethod(elementType),
            expression
        );

    public static MethodCallExpression ToArrayCall(this Expression expression, Type elementType) =>
        Expression.Call
        (
            LinqMethods.EnumerableToArrayMethod.MakeGenericMethod(elementType),
            expression
        );

    public static LambdaExpression MakeLambdaExpression(this ParameterExpression param, Expression body)
    {
        Type[] typeArgs = new[] { param.Type, body.Type };
        Type delegateType = typeof(Func<,>).MakeGenericType(typeArgs);
        return Expression.Lambda(delegateType, body, param);
    }    
}
