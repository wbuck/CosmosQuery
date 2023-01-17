using CosmosQuery.ExpressionBuilders;
using CosmosQuery.Extensions;
using CosmosQuery.Query;
using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser;

namespace CosmosQuery.Visitors;
internal sealed class MemberCollectionFilterAppender : ProjectionVisitorBase
{
    private MemberCollectionFilterAppender(List<PathSegment> pathSegments, ODataQueryContext context)
        : base(pathSegments, context)
    { }

    public static Expression AppendFilters(Expression expression, List<PathSegment> filterPath, ODataQueryContext context) =>
        new MemberCollectionFilterAppender(filterPath, context).Visit(expression);

    protected override Expression MatchedExpression(PathSegment pathSegment, MemberInitExpression node, MemberAssignment binding)
    {
        if (pathSegment.FilterOptions?.Clause is not FilterClause clause)
            return base.VisitMemberInit(node);

        if (!binding.Member.GetMemberType().IsList())
        {
            throw new NotSupportedException();
        }

        return Expression.MemberInit
        (
            Expression.New(node.Type),
            node.Bindings.OfType<MemberAssignment>().Select(UpdateBinding)
        );

        MemberAssignment UpdateBinding(MemberAssignment assignment)
        {
            if (assignment != binding)
                return assignment;

            return assignment.Expression switch
            {
                MethodCallExpression expr => assignment.Update(GetCallExpression(expr, pathSegment)),
                _ => assignment.Update(GetBindingExpression(assignment, clause))
            };
        }
    }

    private Expression GetCallExpression(MethodCallExpression callExpression, in PathSegment pathSegment) =>
        FilterAppender.AppendFilter(callExpression, pathSegment, this.context);

    private Expression GetBindingExpression(MemberAssignment memberAssignment, FilterClause clause)
    {
        Type elementType = memberAssignment.Expression.Type.GetCurrentType();
        return Expression.Call
        (
            LinqMethods.EnumerableWhereMethod.MakeGenericMethod(elementType),
            memberAssignment.Expression,
            clause.GetFilterExpression(elementType, this.context)
        ).ToListCall(elementType);
    }
}
