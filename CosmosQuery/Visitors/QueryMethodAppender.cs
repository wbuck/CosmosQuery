using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Query;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace CosmosQuery.Visitors
{
    internal sealed class QueryMethodAppender : VisitorBase
    {
        private readonly PathSegment collectionSegment;

        private QueryMethodAppender(List<PathSegment> pathSegments, ODataQueryContext context) 
            : base(pathSegments, context)
        {
            this.collectionSegment = this.pathSegments.Last(e => e.MemberType.IsList());
        }

        public static Expression AppendFilters(Expression expression, List<PathSegment> pathSegments, ODataQueryContext context) =>
            new QueryMethodAppender(pathSegments, context).Visit(expression);

        protected override Expression MatchedExpression(PathSegment pathSegment, MemberInitExpression node, MemberAssignment binding)
        {
            if (pathSegment != this.collectionSegment)
                return base.VisitMemberInit(node);

            return Expression.MemberInit
            (
                Expression.New(node.Type),
                node.Bindings.OfType<MemberAssignment>().Select(UpdateBinding)
            );

            MemberAssignment UpdateBinding(MemberAssignment assignment)
            {
                if (assignment != binding)
                    return assignment;

                return assignment.Update(GetBindingExpression(binding));
            }
        }

        private Expression GetBindingExpression(MemberAssignment binding)
        {
            Span<PathSegment> segments = CollectionsMarshal.AsSpan(this.pathSegments);
            ref PathSegment lastSegment = ref segments[^1];

            Debug.Assert(lastSegment.QueryOptions is not null);

            Type elementType = this.collectionSegment.ElementType;

            Expression expression = binding.Expression.NodeType == ExpressionType.Call
                ? GetCallExpression(binding.Expression, lastSegment.QueryOptions!)
                : GetMemberAccessExpression(binding.Expression, lastSegment.QueryOptions!);            

            return expression;            

            Expression GetCallExpression(Expression expression, QueryOptions options) =>
                expression.GetQueryableExpression
                (
                    this.pathSegments, 
                    options, 
                    this.context
                );

            Expression GetMemberAccessExpression(Expression expression, QueryOptions options)
            {
                Expression queryExpression = expression.GetQueryableMethod
                (
                    this.context,
                    options.OrderByClause,
                    elementType,
                    options.Skip,
                    options.Top
                );

                return expression.Type.IsArray
                    ? queryExpression.ToArrayCall(elementType)
                    : queryExpression.ToListCall(elementType);
            }
                
        }
    }
}
