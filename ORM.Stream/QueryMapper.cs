using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace ORM.Stream;

public interface IDevJunglesConnection
{
  IDevJunglesCommand CreateCommand(FormattableString sql);
}

public interface IDevJunglesCommand : IAsyncDisposable
{
  Task<IDataReader> ExecuteReaderAsync(CancellationToken cancellationToken);
}

static class QueryMapper
{
  private static readonly MethodInfo
    GetStringMethod = typeof(DataReaderExtensions).GetMethod("GetString")!,
    GetInt32Method = typeof(DataReaderExtensions).GetMethod("GetInt32")!;

  private static ConcurrentDictionary<Type, Delegate> _mapperFuncs = new();

  public static async Task<List<T>> QueryAsync<T>(this IDevJunglesConnection connection, FormattableString sql, CancellationToken cancellationToken)
  {
    await using var command = connection.CreateCommand(sql);
    using var reader = await command.ExecuteReaderAsync(cancellationToken);

    var list = new List<T>();
    Func<IDataReader, T> func = (Func<IDataReader, T>)_mapperFuncs.GetOrAdd(typeof(T), x => Build<T>());
    
    while (reader.Read())
    {
      list.Add(func(reader));
    }
    return list;
  }

  public static async Task<IList> QueryAsyncType(this IDevJunglesConnection connection, FormattableString sql, Type entityType, CancellationToken cancellationToken)
  {
    var result = (Task)typeof(QueryMapper).GetMethod("QueryAsync")
      .MakeGenericMethod(entityType)
      .Invoke(null, new Object[] { connection, sql, cancellationToken });
    await result;
    dynamic r = result;
    return r.Result;
  }
 

  private static Func<IDataReader, T> Build<T>()
  {
    var readerParam = Expression.Parameter(typeof(IDataReader));

    var newExp = Expression.New(typeof(T));
    var memberInit = Expression.MemberInit(newExp, typeof(T).GetProperties()
      .Select(x => Expression.Bind(x, BuildReadColumnExpression(readerParam, x))));
    
    return Expression.Lambda<Func<IDataReader, T>>(memberInit, readerParam).Compile();
  }
  private static Expression BuildReadColumnExpression(Expression reader, PropertyInfo prop)
  {
    if (prop.PropertyType == typeof(string))
      return Expression.Call(null, GetStringMethod, reader, Expression.Constant(prop.Name));
    else if (prop.PropertyType == typeof(int))
      return Expression.Call(null, GetInt32Method, reader, Expression.Constant(prop.Name));
    throw new InvalidOperationException();
  }
}