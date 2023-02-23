using System.Data;
using System.Text.RegularExpressions;
using Npgsql;

namespace ORM.Stream.Npgsql;

public static class NpgsqlConnectionExtensions
{
  public static IDevJunglesConnection Adapt(this NpgsqlConnection connection)
  {
    return new NpgsqlConnectionAdapter(connection);
  }
}

public class NpgsqlConnectionAdapter : IDevJunglesConnection
{
  private readonly NpgsqlConnection _connection;
  public NpgsqlConnectionAdapter(NpgsqlConnection connection)
  {
    _connection = connection;
  }
  public IDevJunglesCommand CreateCommand(FormattableString sql)
  {
    var command = _connection.CreateCommand();
    command.CommandText = ReplaceParameters(sql.Format);
    for (int i = 0; i < sql.ArgumentCount; i++)
    {
      command.Parameters.AddWithValue($"@p{i}", sql.GetArgument(i));
    }

    return new NpgsqlCommandAdapter(command);
  }
  
  private static string ReplaceParameters(string query)
  {
    var result = Regex.Replace(query, @"\{(\d+)\}", x => $"@p{x.Groups[1].Value}"); // {0} -> @p1
    return result;
  }
}

public class NpgsqlCommandAdapter : IDevJunglesCommand
{
  private readonly NpgsqlCommand _command;
  public NpgsqlCommandAdapter(NpgsqlCommand command)
  {
    _command = command;
  }

  public async Task<IDataReader> ExecuteReaderAsync(CancellationToken cancellationToken)
  {
    return await _command.ExecuteReaderAsync(cancellationToken);
  }
  
  public async ValueTask DisposeAsync()
  { 
    await _command.DisposeAsync();
  }
}