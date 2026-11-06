using System.Net.Http.Json;
using System.Text.Json;

namespace AgriStream.Web.Services;

public record IrrigationResponse(string CommandId, string Status);

public interface IFunctionsClient
{
    Task<List<Dictionary<string, JsonElement>>> GetLatestTelemetryAsync(string field, int count = 20);
    Task<IrrigationResponse?> TriggerIrrigationAsync(int deviceId, string field, int minutes, string requestedBy);
}

public class FunctionsClient : IFunctionsClient
{
    private readonly HttpClient _http;

    public FunctionsClient(HttpClient http, IConfiguration config)
    {
        var baseUrl = config["Functions:BaseUrl"]
            ?? throw new InvalidOperationException("Functions:BaseUrl is missing.");
        _http = http;
        _http.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        var key = config["Functions:Key"];
        if (!string.IsNullOrWhiteSpace(key))
            _http.DefaultRequestHeaders.Add("x-functions-key", key);
    }

    public async Task<List<Dictionary<string, JsonElement>>> GetLatestTelemetryAsync(string field, int count = 20)
    {
        var response = await _http.GetAsync($"telemetry/{Uri.EscapeDataString(field)}?count={count}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Dictionary<string, JsonElement>>>() ?? new();
    }

    public async Task<IrrigationResponse?> TriggerIrrigationAsync(int deviceId, string field, int minutes, string requestedBy)
    {
        var response = await _http.PostAsJsonAsync("irrigation", new
        {
            deviceId,
            fieldName = field,
            durationMinutes = minutes,
            requestedBy
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IrrigationResponse>(
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}