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


int index = 0;
await foreach (var batch in store.GetBatchesAsync(batchSize: 1000))
{
    Console.WriteLine($"Batch {index}: {batch.Count} logs");
    index++;
}