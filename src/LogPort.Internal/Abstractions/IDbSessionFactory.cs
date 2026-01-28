namespace LogPort.Internal.Abstractions;

public interface IDbSessionFactory
{
    IDbSession Create();
}