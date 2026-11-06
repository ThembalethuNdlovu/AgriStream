using System.Text.Json;
using AgriStream.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AgriStream.Functions.Functions;

public class TriggerIrrigationOutput
{
    [QueueOutput("irrigation-commands", Connection = "AzureStorage")]
    public string? QueueMessage { get; set; }

    [HttpResult]
    public IActionResult HttpResponse { get; set; } = null!;
}

public class TriggerIrrigationFunction
{
    [Function("TriggerIrrigation")]
    public async Task<TriggerIrrigationOutput> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "irrigation")] HttpRequest req)
    {
        IrrigationCommand? cmd = null;
        try { cmd = await req.ReadFromJsonAsync<IrrigationCommand>(); }
        catch (JsonException) { }

        if (cmd is null || cmd.DeviceId <= 0 || string.IsNullOrWhiteSpace(cmd.FieldName))
        {
            return new TriggerIrrigationOutput
            {
                HttpResponse = new BadRequestObjectResult("DeviceId and FieldName are required.")
            };
        }

        cmd.CommandId = Guid.NewGuid().ToString();
        cmd.RequestedUtc = DateTime.UtcNow;
        cmd.DurationMinutes = Math.Clamp(cmd.DurationMinutes, 1, 120);

        return new TriggerIrrigationOutput
        {
            QueueMessage = JsonSerializer.Serialize(cmd),
            HttpResponse = new ObjectResult(new { commandId = cmd.CommandId, status = "Queued" }) { StatusCode = 202 }
        };
    }
}