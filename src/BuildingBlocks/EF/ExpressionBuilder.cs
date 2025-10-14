using System.Linq.Expressions;

namespace BuildingBlocks.EF;

public static class ExpressionBuilder
{
    public static Expression<Func<T, bool>> CombineWithOr<T>(
        Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2
    )
    {
        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
        var left = leftVisitor.Visit(expr1.Body);

        var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
        var right = rightVisitor.Visit(expr2.Body);

        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(left!, right!), parameter);
    }

    public class ReplaceExpressionVisitor(Expression oldValue, Expression newValue) : ExpressionVisitor
    {
        public override Expression? Visit(Expression? node)
        {
            if (node == oldValue)
                return newValue;
            return base.Visit(node);
        }
    }
}
