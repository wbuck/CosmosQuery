using CosmosQuery.Cache;
using CosmosQuery.Extensions;
using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser;
using System.Linq.Expressions;

namespace CosmosQuery.Visitors
{
    internal sealed class MemberCollectionFilterAppender : VisitorBase
    {
        private MemberCollectionFilterAppender(List<PathSegment> pathSegments, ODataQueryContext context) 
            : base(pathSegments, context)
        {}

        public static Expression AppendFilters(Expression expression, List<PathSegment> filterPath, ODataQueryContext context) =>
            new MemberCollectionFilterAppender(filterPath, context).Visit(expression);        

        protected override Expression MatchedExpression(PathSegment pathSegment, MemberInitExpression node, MemberAssignment binding)
        {
            if (pathSegment.FilterOptions?.FilterClause is not FilterClause clause)
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
            Type memberType = memberAssignment.Member.GetMemberType();
            Type elementType = memberType.GetCurrentType();

            LambdaExpression lambdaExpression = clause.GenerateFilterExpression(elementType, this.context);

            return memberAssignment.Expression.NodeType switch
            {
                ExpressionType.Convert => GetConvertedCallExpression
                (
                    lambdaExpression, 
                    (UnaryExpression)memberAssignment.Expression
                ),
                _ => GetCallExpression()
            };

            Expression GetCallExpression()
            {
                MethodCallExpression callExpression = GetWhereCallExpression
                (
                    elementType, 
                    lambdaExpression, 
                    memberAssignment.Expression
                );
                return memberType.IsArray
                    ? callExpression.ToArrayCall(elementType)
                    : callExpression.ToListCall(elementType);
            }
        }

        // If the member assignment is a convert expression (E.g., Convert(Collection, CollectionModel[]))
        // then we need to rearrange the expression as the Cosmos DB provider throws an error
        // if we attempt to append a where clause after the conversion (E.g., Convert(Collection, CollectionModel[]).Where(...)).
        // Instead rearrage the conversion to the following: 
        // Convert(Collection.Where(it => (Convert(it, Model) == Value).ToArray(), CollectionModel[]).
        private static Expression GetConvertedCallExpression(LambdaExpression lambdaExpression, UnaryExpression unaryExpression)
        {
            Type destinationType = unaryExpression.Type.GetCurrentType();
            Type sourceType = unaryExpression.Operand.Type.GetCurrentType();

            ParameterExpression originalParameter = lambdaExpression.Parameters[0];

            ParameterExpression newParameter = Expression.Parameter(sourceType, originalParameter.Name);
            Expression body = lambdaExpression.Body.ReplaceParameter
            (
                originalParameter,
                Expression.Convert(newParameter, destinationType)
            );

            lambdaExpression = Expression.Lambda(body, newParameter);

            MethodCallExpression callExpression = GetWhereCallExpression
            (
                sourceType,
                lambdaExpression,
                unaryExpression.Operand                
            );

            callExpression = unaryExpression.Type.IsArray
                ? callExpression.ToArrayCall(sourceType)
                : callExpression.ToListCall(sourceType);

            return Expression.Convert(callExpression, unaryExpression.Type);
        }

        private static MethodCallExpression GetWhereCallExpression(Type elementType, LambdaExpression lambdaExpression, Expression expression)
            => Expression.Call
               (
                   LinqMethods.EnumerableWhereMethod.MakeGenericMethod(elementType),
                   expression,
                   lambdaExpression
               );
    }
}
