using LogPort.Core;
using LogPort.Internal;
using LogPort.Internal.Configuration;
using LogPort.Internal.DSL;
using LogPort.Internal.Metrics;
using LogPort.Internal.Services;

var tokenizer = new Tokenizer();

var tokens = tokenizer.Tokenize("level >= 3 and message contains \"timeout or something\"");

foreach (var token in tokens) Console.WriteLine(token);

var parser = new Parser(tokens);
var expr = parser.Parse();

Console.WriteLine(expr);

var sqlBuilder = new SqlWhereBuilder();
var (whereClause, parameters) = sqlBuilder.Build(expr);

Console.WriteLine();
Console.WriteLine("WHERE " + whereClause);

foreach (var p in parameters)
    Console.WriteLine($"{p.Key} = {p.Value}");