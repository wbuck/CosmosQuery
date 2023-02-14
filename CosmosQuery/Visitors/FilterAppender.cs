/*
MIT License

Copyright (c) 2019 AutoMapper

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */
using CosmosQuery.Extensions;
using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Query;
using System.Linq.Expressions;

namespace CosmosQuery.Visitors
{
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
            if (node.Method.Name == nameof(Enumerable.Select)
                && elementType == node.Type.GetUnderlyingElementType()
                && this.expression.ToString().StartsWith(node.ToString()))//makes sure we're not updating some nested "Select"
            {
                return Expression.Call
                (
                    node.Method.DeclaringType!,
                    "Where",
                    new Type[] { node.GetUnderlyingElementType() },
                    node,
                    this.pathSegment.FilterOptions!.FilterClause.GetFilterExpression(elementType, context)
                );
            }

            return base.VisitMethodCall(node);
        }
    }
}
