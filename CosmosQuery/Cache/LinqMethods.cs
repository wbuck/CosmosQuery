using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace CosmosQuery.Cache;
internal class LinqMethods
{
    private static readonly MethodInfo _queryableWhereMethod =
        GetGenericMethod(_ => default(IQueryable<int>)!.Where(default(Expression<Func<int, bool>>)!));

    private static readonly MethodInfo _enumerableWhereMethod =
        GetGenericMethod(_ => default(IEnumerable<bool>)!.Where(i => i));

    private static readonly MethodInfo _queryableSelectMethod =
        GetGenericMethod(_ => default(IQueryable<int>)!.Select(i => i));

    private static readonly MethodInfo _enumerableSelectMethod =
        GetGenericMethod(_ => default(IEnumerable<int>)!.Select(i => i));

    private static readonly MethodInfo _enumerableNonEmptyAnyMethod =
        GetGenericMethod(_ => default(IEnumerable<int>)!.Any(default!));

    private static readonly MethodInfo _enumerableEmptyAnyMethod =
        GetGenericMethod(_ => default(IEnumerable<int>)!.Any());

    private static readonly MethodInfo _queryableTakeMethod =
        GetGenericMethod(_ => default(IQueryable<int>)!.Take(0));

    private static readonly MethodInfo _queryableSkipMethod =
       GetGenericMethod(_ => default(IQueryable<int>)!.Skip(0));

    private static readonly MethodInfo _enumerableOrderByMethod =
        GetGenericMethod(_ => Enumerable.OrderBy(default(IQueryable<int>)!, i => i));

    private static readonly MethodInfo _enumerableOrderByDescendingMethod =
        GetGenericMethod(_ => Enumerable.OrderByDescending(default(IQueryable<int>)!, i => i));

    private static readonly MethodInfo _enumerableToListMethod =
        GetGenericMethod(_ => default(IEnumerable<int>)!.ToList());

    private static readonly MethodInfo _enumerableToArrayMethod =
        GetGenericMethod(_ => default(IEnumerable<int>)!.ToArray());

    public static MethodInfo EnumerableWhereMethod =>
        _enumerableWhereMethod;
    public static MethodInfo QueryableSkipMethod =>
        _queryableSkipMethod;
    public static MethodInfo QueryableTakeMethod =>
        _queryableTakeMethod;
    public static MethodInfo QueryableWhereMethod =>
        _queryableWhereMethod;
    public static MethodInfo QueryableSelectMethod =>
        _queryableSelectMethod;
    public static MethodInfo EnumerableSelectMethod =>
        _enumerableSelectMethod;
    public static MethodInfo EnumerableEmptyAnyMethod =>
        _enumerableEmptyAnyMethod;
    public static MethodInfo EnumerableNonEmptyAnyMethod =>
        _enumerableNonEmptyAnyMethod;
    public static MethodInfo EnumerableOrderByMethod =>
        _enumerableOrderByMethod;
    public static MethodInfo EnumerableOrderByDescendingMethod =>
        _enumerableOrderByDescendingMethod;
    public static MethodInfo EnumerableToListMethod =>
        _enumerableToListMethod;
    public static MethodInfo EnumerableToArrayMethod =>
        _enumerableToArrayMethod;

    private static MethodInfo GetGenericMethod<TReturn>(Expression<Func<object, TReturn>> expression) =>
        GetGenericMethod(expression as Expression);

    private static MethodInfo GetGenericMethod(Expression expression)
    {
        var lamdaExpression = expression as LambdaExpression;

        Debug.Assert(expression.NodeType == ExpressionType.Lambda);
        Debug.Assert(lamdaExpression is not null);
        Debug.Assert(lamdaExpression.Body.NodeType == ExpressionType.Call);

        return ((MethodCallExpression)lamdaExpression.Body).Method.GetGenericMethodDefinition();
    }
}
