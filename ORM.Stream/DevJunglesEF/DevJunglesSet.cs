using System.Collections;
using System.Linq.Expressions;

namespace ORM.Stream.DevJunglesEF;

public class DevJunglesSet<T> : DevJunglesQueryable<T>
{
  public DevJunglesSet(IQueryProvider provider) : base(provider)
  {
  }
}

public class DevJunglesQueryable<T> : IQueryable<T>
{
  public DevJunglesQueryable(IQueryProvider provider)
  {
    Expression = Expression.Constant(this);
    Provider = provider;
    ElementType = typeof(T);
  }
  public DevJunglesQueryable(Expression expression, DevJunglesQueryProvider devJunglesQueryProvider)
  {
    Expression = expression;
    Provider = devJunglesQueryProvider;
    ElementType = typeof(T);
  }

  public IEnumerator<T> GetEnumerator()
  {
    return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
  }
  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
  public Type ElementType { get; }
  public Expression Expression { get; }
  public IQueryProvider Provider { get; }
}