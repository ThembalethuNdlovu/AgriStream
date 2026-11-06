using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgriStream.Functions.Functions;

public class ProcessDroneImageFunction
{
    private readonly ILogger<ProcessDroneImageFunction> _logger;
    private readonly IConfiguration _config;

    public ProcessDroneImageFunction(ILogger<ProcessDroneImageFunction> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    [Function("ProcessDroneImage")]
    public async Task Run(
        [BlobTrigger("drone-images/{name}", Connection = "AzureStorage")] BlobClient blob,
        string name)
    {
        var props = (await blob.GetPropertiesAsync()).Value;
        _logger.LogInformation("Drone image uploaded: {Name}, {Size} bytes", name, props.ContentLength);

        var table = new TableClient(_config["AzureStorage"], "ImageAudit");
        await table.CreateIfNotExistsAsync();

        var rowKey = $"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks:D19}_{Guid.NewGuid():N}";
        var entity = new TableEntity("DroneImages", rowKey)
        {
            ["BlobName"] = name,
            ["SizeBytes"] = props.ContentLength,
            ["ContentType"] = props.ContentType,
            ["UploadedUtc"] = props.CreatedOn.UtcDateTime,
            ["ProcessedUtc"] = DateTime.UtcNow,
            ["Status"] = "Processed"
        };
        await table.AddEntityAsync(entity);
    }
}