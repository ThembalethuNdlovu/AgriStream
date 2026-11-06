using System.Text.Json;
using AgriStream.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgriStream.Functions.Functions;

public class ProcessIrrigationCommandFunction
{
    private readonly ILogger<ProcessIrrigationCommandFunction> _logger;
    private readonly IConfiguration _config;

    public ProcessIrrigationCommandFunction(ILogger<ProcessIrrigationCommandFunction> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    [Function("ProcessIrrigationCommand")]
    public async Task Run(
        [QueueTrigger("irrigation-commands", Connection = "AzureStorage")] string message)
    {
        var cmd = JsonSerializer.Deserialize<IrrigationCommand>(message, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        if (cmd is null)
        {
            _logger.LogWarning("Could not read queue message: {Message}", message);
            return; // do not throw, a bad message would be retried 5 times
        }

        _logger.LogInformation("Irrigation command {Id} for device {Device} ({Minutes} min)",
            cmd.CommandId, cmd.DeviceId, cmd.DurationMinutes);

        // Adjust table/column names here if your Device model differs
        const string sql = "UPDATE Devices SET Status = @status WHERE Id = @id";

        await using var conn = new SqlConnection(_config["SqlConnection"]);
        await conn.OpenAsync();
        await using var sqlCmd = new SqlCommand(sql, conn);
        sqlCmd.Parameters.AddWithValue("@status", "Irrigating");
        sqlCmd.Parameters.AddWithValue("@id", cmd.DeviceId);

        var rows = await sqlCmd.ExecuteNonQueryAsync();
        if (rows == 0)
            _logger.LogWarning("No device with Id {Id} found", cmd.DeviceId);
        else
            _logger.LogInformation("Device {Id} status set to Irrigating", cmd.DeviceId);
    }
}