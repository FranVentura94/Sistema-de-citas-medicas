using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace GenericPersistence.FiltroDinamico;

/// <summary>
/// Filtro dinámico: convierte un texto (por ejemplo x.Nombres == "Ana") en un
/// <see cref="Expression{TDelegate}"/> listo para usarse como filtro de
/// <c>GetPagedAsync</c>. La condición se construye en tiempo de ejecución.
/// </summary>
public static class Filter
{
    public static Expression<Func<TModel, bool>> FromStringExpression<TModel>(string query, string parameter = "x")
    {
        try
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TModel), parameter);
            return (Expression<Func<TModel, bool>>)DynamicExpressionParser.ParseLambda(new ParameterExpression[1] { parameterExpression }, null, query);
        }
        catch
        {
            throw new ValidationException("filter expression invalid");
        }
    }
}
