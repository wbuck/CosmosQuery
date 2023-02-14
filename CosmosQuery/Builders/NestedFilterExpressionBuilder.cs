﻿using CosmosQuery.Extensions;
using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser;
using System.Linq.Expressions;

namespace CosmosQuery;
internal static class NestedFilterExpressionBuilder
{
    public static LambdaExpression GenerateFilterExpression(this FilterClause filterClause, Type type, ODataQueryContext context)
    {
        var parameters = new Dictionary<string, ParameterExpression>();
        return new FilterHelper(parameters, context)
            .GetFilterPart(filterClause.Expression)
            .GetFilter(type, parameters, filterClause.RangeVariable.Name)
            .ReplaceParameter();        
    }

    // Parameters that start with the '$' character are illegal
    // in Cosmos DB.
    private static LambdaExpression ReplaceParameter(this LambdaExpression lambda) =>
        lambda.Parameters.Where(p => p.Name?.StartsWith('$') ?? false).Aggregate
        (
            lambda,
            (expr, param) => (LambdaExpression)expr.ReplaceParameter(param, param.NewParameter())
        );

    private static ParameterExpression NewParameter(this ParameterExpression expression)
    {
        if (expression.Name is null)
            return expression;

        int index = expression.Name!.LastIndexOf('$');
        string name = index < 0 ? expression.Name : expression.Name[(index + 1)..];
        return Expression.Parameter(expression.Type, name);
    }


}
