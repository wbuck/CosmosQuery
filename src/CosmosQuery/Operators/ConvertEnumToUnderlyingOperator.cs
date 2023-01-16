using LogicBuilder.Expressions.Utils.ExpressionBuilder.Operand;
using LogicBuilder.Expressions.Utils.ExpressionBuilder;
using LogicBuilder.Expressions.Utils;

namespace CosmosQuery.Operators;

internal sealed class ConvertEnumToUnderlyingOperator : IExpressionPart
{
    private readonly IExpressionPart expressionPart;

    public ConvertEnumToUnderlyingOperator(IExpressionPart expressionPart)
    {
        this.expressionPart = expressionPart;
    }

    public Expression Build() =>
        Build(expressionPart.Build());    

    private static Expression Build(Expression expression)
    {
        Type underlyingType = expression.Type.ToNullableUnderlyingType();

        if (!underlyingType.IsEnum)
            return expression;

        Type underlyingEnumType = underlyingType.GetEnumUnderlyingType();

        if (expression is ParameterExpression)
        {
            return GetConvertExpression(expression, underlyingEnumType);
        }
        else if (expression is MemberExpression memberExpression)
        {
            if (memberExpression.Expression is ConstantExpression constant)
                return GetConstantExpression(constant, underlyingEnumType);
            else
                return GetConvertExpression(expression, underlyingEnumType);
        }

        return expression;
    }

    private static Expression GetConstantExpression(ConstantExpression constant, Type underlyingEnumType)
    {
        if (constant.Value is null)
            return constant;

        var property = constant.Value.GetType()
            .GetProperty(nameof(ConstantContainer<object>.TypedProperty));

        if (property is not null)
        {
            object value = property.GetValue(constant.Value)!;
            return GetConstant(value, underlyingEnumType);
        }

        return GetConstant(constant.Value, underlyingEnumType);

        static Expression GetConstant(object value, Type type) =>
            Expression.Constant
            (
                Convert.ChangeType(value, type),
                type
            );
    }

    private static Expression GetConvertExpression(Expression expression, Type underlyingEnumType)
    {
        Type conversionType = expression.Type.IsNullableType()
            ? underlyingEnumType.ToNullable()
            : underlyingEnumType;

        return Expression.Convert(expression, conversionType);
    }
}
