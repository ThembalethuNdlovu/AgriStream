namespace AgriStream.Web.Models;

public class TelemetryViewModel
{
    public string SelectedField { get; set; } = "";
    public string[] Fields { get; set; } = Array.Empty<string>();
    public List<SensorReading> Readings { get; set; } = new();
}