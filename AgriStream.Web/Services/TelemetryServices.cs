using Azure.Data.Tables;
using AgriStream.Web.Models;

namespace AgriStream.Web.Services;

public interface ITelemetryService
{
    Task AddReadingsAsync(IEnumerable<SensorReading> readings);
    Task<List<SensorReading>> GetLatestAsync(string field, int count = 20);
}

public class TelemetryService : ITelemetryService
{
    public static readonly string[] DemoFields =
        { "North Field 1", "North Field 2", "South Field 1", "East Field 1" };

    private readonly TableClient _table;

    public TelemetryService(TableServiceClient serviceClient, IConfiguration config)
    {
        var name = config["AzureStorage:TelemetryTable"] ?? "SensorReadings";
        _table = serviceClient.GetTableClient(name);
        _table.CreateIfNotExists();
    }

    public async Task AddReadingsAsync(IEnumerable<SensorReading> readings)
    {
        // Batch transactions must share a partition and hold at most 100 entities
        foreach (var group in readings.GroupBy(r => r.PartitionKey))
        {
            foreach (var chunk in group.Chunk(100))
            {
                var actions = chunk.Select(r =>
                    new TableTransactionAction(TableTransactionActionType.Add, r));
                await _table.SubmitTransactionAsync(actions);
            }
        }
    }

    public async Task<List<SensorReading>> GetLatestAsync(string field, int count = 20)
    {
        var list = new List<SensorReading>();
        await foreach (var r in _table.QueryAsync<SensorReading>(x => x.PartitionKey == field, maxPerPage: count))
        {
            list.Add(r);
            if (list.Count >= count) break;
        }
        return list;
    }
}