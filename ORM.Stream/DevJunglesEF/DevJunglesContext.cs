using System.Reflection;

namespace ORM.Stream.DevJunglesEF;

public class DevJunglesContext : IDevJunglesContext
{
  private readonly IDevJunglesConnection _connection;
  public DevJunglesContext(IDevJunglesConnection connection)
  {
    _connection = connection;
    var sets =
      GetType().GetProperties()
        .Where(x => x.PropertyType.GetGenericTypeDefinition() == typeof(DevJunglesSet<>));
    foreach (var set in sets)
    {
      set.SetValue(this, CreateSet(set.PropertyType.GetGenericArguments()[0]));
    }
      
  }
  private object? CreateSet(Type setPropertyType)
  {
    return typeof(DevJunglesContext).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy)
      .Single(x => x.Name == "CreateSetInternal")
      .MakeGenericMethod(setPropertyType)
      .Invoke(this, Array.Empty<object>());
  }

  private object CreateSetInternal<T>() => new DevJunglesSet<T>(new DevJunglesQueryProvider(this));
  public string ResolveTableName(Type entityType) => "\"Documents\"";
  
  public TResult QueryAsync<TResult>(FormattableString sql)
  {
    if (typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(IEnumerable<>))
    {
      var entityType = typeof(TResult).GetGenericArguments()[0];
      var task = (Task)QueryMapper.QueryAsyncType(_connection, sql, entityType, default);
      task.Wait();
      dynamic d = task;
      return d.Result;
    }
    throw new InvalidOperationException();
  }
}