using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace NgLoader.Queries.Utils
{
    internal class ExpressionModifier : ExpressionVisitor
    {
        public static Expression BindLambdaBody(LambdaExpression expression, Expression parameter)
        {
            if (expression.Parameters.Count != 1)
            {
                throw new ArgumentException("Only supporting LambdaExpression with one parameter.");
            }

            return Replace(expression.Body, expression.Parameters[0], parameter);
        }

        public static Expression Replace(Expression expression, Expression from, Expression to)
        {
            return new ExpressionModifier(from, to).Visit(expression);
        }

        private readonly Expression from;
        private readonly Expression to;

        private ExpressionModifier(Expression from, Expression to)
        {
            this.from = from;
            this.to = to;
        }

        [return: NotNullIfNotNull("node")]
        public override Expression? Visit(Expression? node)
        {
            return from == node ? to : base.Visit(node);
        }
    }
}
