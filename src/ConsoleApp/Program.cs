using LogPort.Internal.DSL;

var compiler = new QueryCompiler();

var (where, parameters) =
    compiler.Compile("level >= 3 and message contains \"timeout\"");

Console.WriteLine(where);