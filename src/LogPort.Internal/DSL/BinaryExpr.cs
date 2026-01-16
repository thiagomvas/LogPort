namespace LogPort.Internal.DSL;

public sealed record BinaryExpr(
    Expr Left,
    string Operator,
    Expr Right
) : Expr;