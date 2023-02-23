// See https://aka.ms/new-console-template for more information

using Npgsql;
using ORM.Stream;
using ORM.Stream.DevJunglesEF;
using ORM.Stream.Npgsql;

await using var connection = new NpgsqlConnection("Host=localhost;Database=FullTextGames;User Id=postgres;Password=postgres");
await connection.OpenAsync();

var adaptedConnection = connection.Adapt();

int id = 20;
FormattableString sql = $"""
SELECT "Id" FROM "Documents" WHERE "Id" < {id};
""";

var ctx = new FullTextGamesContext(adaptedConnection);
var result = ctx.Documents.Where(x => x.Id < 20).Select(x => new Document() { Id = x.Id});
var results = result.ToArray();

Console.WriteLine(results.Length);


foreach (var doc in await adaptedConnection.QueryAsync<Document>(sql, default))
{
  Console.WriteLine(doc.Id + " " + doc.Content?.Length);
}

public class Document
{
  public int Id { get; set; }
  public string? Content { get; set; }
}

public class FullTextGamesContext : DevJunglesContext
{
  public DevJunglesSet<Document> Documents { get; set; }
  public FullTextGamesContext(IDevJunglesConnection connection) : base(connection)
  {
  }
}