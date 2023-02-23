using System.Linq.Expressions;

namespace ORM.Stream.DevJunglesEF;

public class DevJunglesQueryProvider : IQueryProvider
{
  private readonly IDevJunglesContext _context;
  public object? Execute(Expression expression) => Execute<object>(expression);
  public IQueryable CreateQuery(Expression expression) => CreateQuery<object>(expression);

  public DevJunglesQueryProvider(IDevJunglesContext context)
  {
    _context = context;

  }
  
  public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
  {
    return new DevJunglesQueryable<TElement>(expression, this);
  }
  public TResult Execute<TResult>(Expression expression)
  {
    var result = new QueryBuilder(_context);
    var sql = result.Compile(expression);

    return _context.QueryAsync<TResult>(sql);
  }
}