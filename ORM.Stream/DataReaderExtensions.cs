using System.Data;

namespace ORM.Stream;

static class DataReaderExtensions
{
  private static bool TryGetOrdinal(this IDataReader reader, string column, out int order)
  {
    order = -1;
    for (int i = 0; i < reader.FieldCount; i++)
    {
      if (reader.GetName(i) == column)
      {
        order = i;
        return true;
      }
    }
    return false;
  }
  
  public static string GetString(this IDataReader reader, string columnName)
  {
    if (reader.TryGetOrdinal(columnName, out int order))
      return reader.GetString(order);
    return default;
  }
  
  public static int GetInt32(this IDataReader reader, string columnName)
  {
    if (reader.TryGetOrdinal(columnName, out int order))
      return reader.GetInt32(order);
    return default;
  }
}