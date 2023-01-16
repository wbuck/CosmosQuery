namespace CosmosQuery.Visitors;

internal sealed class ParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression _source;
    private readonly Expression _target;

    public ParameterReplacer(ParameterExpression source, Expression target)
    {
        _source = source;
        _target = target;
    }

    /// <inheritdoc cref="VisitParameter(ParameterExpression)"/>
    protected override Expression VisitParameter(ParameterExpression node) =>    
        node == _source ? _target : base.VisitParameter(node);    
}
