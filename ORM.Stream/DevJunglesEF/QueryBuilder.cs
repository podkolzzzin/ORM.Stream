using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace ORM.Stream.DevJunglesEF;

public class QueryBuilder : ExpressionVisitor
{
  private readonly IDevJunglesContext _context;
  private Expression
    selectList,
    whereExpression;

  public QueryBuilder(IDevJunglesContext context)
  {
    _context = context;
  }
  
  protected override Expression VisitMethodCall(MethodCallExpression node)
  {
    if (node.Method.IsGenericMethod)
    {
      var genericMethod = node.Method.GetGenericMethodDefinition();
      if (genericMethod == QueryableMethods.Select)
      {
        VisitSelect(node);
      }
      else if (genericMethod == QueryableMethods.Where)
      {
        VisitWhere(node);
      }
    }
    return base.VisitMethodCall(node);
  }
  private void VisitWhere(MethodCallExpression node)
  {
    whereExpression = ((UnaryExpression)node.Arguments[1]).Operand;
  }
  private void VisitSelect(MethodCallExpression node)
  {
    selectList = ((UnaryExpression)node.Arguments[1]).Operand;
  }
  public FormattableString Compile(Expression expression)
  {
    Visit(expression);
    var whereVisitor = new WhereVisitor();
    whereVisitor.Visit(whereExpression);
    var selectVisitor = new SelectVisitor();
    selectVisitor.Visit(selectList);
    var whereClause = whereVisitor.Result;
    var selectListClause = selectVisitor.Result;
    var tableName = _context.ResolveTableName(expression.Type);
    var sql = $"""
SELECT 
   {selectListClause}
FROM
    {tableName}
WHERE
    {whereClause}
""";
    return FormattableStringFactory.Create(sql);
  }
}

internal class StringExpression : Expression
{
  public string String { get; }
  public StringExpression(string @string, ExpressionType nodeType, Type type)
  {
    String = @string;
    NodeType = nodeType;
    Type = type;
  }
  public override ExpressionType NodeType { get; }
  public override Type Type { get; }
}

internal class WhereVisitor : ExpressionVisitor
{
  protected override Expression VisitBinary(BinaryExpression node)
  {
    var @operator = node.NodeType switch {
      ExpressionType.GreaterThan => ">",
      ExpressionType.Equal => "=",
      ExpressionType.OrElse => "OR",
      ExpressionType.AndAlso => "AND",
      ExpressionType.LessThan => "<"
    };

    var left = ToString(node.Left);
    var right = ToString(node.Right);
    Result = $"{left} {@operator} {right}";
    return base.VisitBinary(node);
  }

  public string Result { get; set; }

  private string ToString(Expression exp)
  {
    if (exp is ConstantExpression cs)
      return cs.Value.ToString();
    return $"\"{((MemberExpression)exp).Member.Name}\"";
  }
}

internal class SelectVisitor : ExpressionVisitor
{
  protected override Expression VisitMemberInit(MemberInitExpression node)
  {
    var nodes = node.Bindings.Cast<MemberAssignment>().Select(x => ToString(x.Expression));
    Result = string.Join(", ", nodes);
    return base.VisitMemberInit(node);
  }

  public string Result { get; set; }
  
  private string ToString(Expression exp)
  {
    if (exp is ConstantExpression cs)
      return cs.Value.ToString();
    return $"\"{((MemberExpression)exp).Member.Name}\"";
  }
}