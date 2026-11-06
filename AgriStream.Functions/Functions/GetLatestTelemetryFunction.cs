using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;

namespace AgriStream.Functions.Functions;

public class GetLatestTelemetryFunction
{
    private readonly IConfiguration _config;

    public GetLatestTelemetryFunction(IConfiguration config) => _config = config;

    [Function("GetLatestTelemetry")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "telemetry/{field}")] HttpRequest req,
        string field)
    {
        var count = int.TryParse(req.Query["count"], out var c) ? Math.Clamp(c, 1, 100) : 20;
        var table = new TableClient(_config["AzureStorage"], _config["TelemetryTable"] ?? "SensorReadings");

        var results = new List<Dictionary<string, object?>>();
        // RowKey is reverse ticks, so the default ascending order returns newest first
        await foreach (var e in table.QueryAsync<TableEntity>(
            filter: TableClient.CreateQueryFilter($"PartitionKey eq {field}"), maxPerPage: count))
        {
            results.Add(e.Where(kv => kv.Key != "odata.etag")
                         .ToDictionary(kv => kv.Key, kv => (object?)kv.Value));
            if (results.Count >= count) break;
        }
        return new OkObjectResult(results);
    }
}