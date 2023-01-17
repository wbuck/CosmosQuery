using CosmosQuery.ExpressionBuilders;
using CosmosQuery.Extensions;
using CosmosQuery.Query;
using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser;

namespace CosmosQuery.Visitors;
internal class FilterAppender : ExpressionVisitor
{
    private readonly PathSegment pathSegment;
    private readonly Expression expression;
    private readonly ODataQueryContext context;

    public FilterAppender(Expression expression, PathSegment pathSegment, ODataQueryContext context)
    {
        this.pathSegment = pathSegment;
        this.expression = expression;
        this.context = context;
    }

    public static Expression AppendFilter(Expression expression, PathSegment pathSegment, ODataQueryContext context)
        => new FilterAppender(expression, pathSegment, context).Visit(expression);

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        Type elementType = this.pathSegment.ElementType;

        if (node.Method.Name == nameof(Queryable.Select)
            && this.pathSegment.FilterOptions?.Clause is FilterClause clause
            && elementType == node.Type.GetUnderlyingElementType()
            && this.expression.ToString().StartsWith(node.ToString()))//makes sure we're not updating some nested "Select"
        {
            return Expression.Call
            (
                node.Method.DeclaringType!,
                nameof(Queryable.Where),
                new Type[] { node.GetUnderlyingElementType() },
                node,
                clause.GetFilterExpression(elementType, context)
            );
        }

        return base.VisitMethodCall(node);
    }
}
