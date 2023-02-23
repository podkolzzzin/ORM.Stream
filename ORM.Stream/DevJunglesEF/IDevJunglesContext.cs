namespace ORM.Stream.DevJunglesEF;

public interface IDevJunglesContext
{
  string ResolveTableName(Type entityType);
  TResult QueryAsync<TResult>(FormattableString sql);
}