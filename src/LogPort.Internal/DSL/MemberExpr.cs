namespace LogPort.Internal.DSL;

public sealed record MemberExpr(Expr Target, string Member) : Expr;