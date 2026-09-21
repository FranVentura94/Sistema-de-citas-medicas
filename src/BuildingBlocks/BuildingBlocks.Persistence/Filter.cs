using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace BuildingBlocks.Persistence;

public static class Filter
{
    public static Expression<Func<TModel, bool>> FromStringExpression<TModel>(string query, string parameter = "x")
    {
        try
        {
            var parameterExpression = Expression.Parameter(typeof(TModel), parameter);
            return (Expression<Func<TModel, bool>>)DynamicExpressionParser.ParseLambda(
                new[] { parameterExpression }, null, query);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"La expresión de filtro '{query}' no es válida.", ex);
        }
    }
}