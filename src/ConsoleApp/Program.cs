using LogPort.Core.Models;
using LogPort.Data.Postgres;
using LogPort.Internal.Configuration;
using LogPort.Internal.DSL;
using LogPort.SDK;

var fac = new DbSessionFactory("Host=localhost;Port=5432;Database=logport;Username=postgres;Password=postgres;");
var store = new PostgresLogStore(new() {Postgres = new() { PartitionLength = 1}},
    fac,
    new PostgresLogPatternStore(new(), fac),
    new());
    
await store.AddBatchAsync([new()
{
    Message = "Hello World!",
    Level = "Info",
    Timestamp = DateTime.UtcNow,
    ServiceName = "Foobar"
}]);