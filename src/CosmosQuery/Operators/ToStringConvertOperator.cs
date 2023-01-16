using LogicBuilder.Expressions.Utils.ExpressionBuilder;
using LogicBuilder.Expressions.Utils;

namespace CosmosQuery.Operators;

internal sealed class ToStringConvertOperator : IExpressionPart
{
    private readonly IExpressionPart source;

    public ToStringConvertOperator(IExpressionPart source) =>
        this.source = source;

    public Expression Build() => Build(source.Build());

    private Expression Build(Expression expression) => expression.Type switch
    {
        Type type when type.IsNullableType() => ConvertNullable(expression),
        _ => expression.GetObjectToStringCall()
    };

    private static Expression ConvertNullable(Expression expression)
    {
        Expression memberAccess = expression.MakeValueSelectorAccessIfNullable();
        return Expression.Condition
        (
            expression.MakeHasValueSelector(),
            memberAccess.GetObjectToStringCall(),
            Expression.Constant(null, typeof(string))
        );
    }
}
