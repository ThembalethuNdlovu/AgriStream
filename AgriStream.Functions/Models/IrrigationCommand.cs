namespace AgriStream.Functions.Models;

public class IrrigationCommand
{
    public string CommandId { get; set; } = Guid.NewGuid().ToString();
    public int DeviceId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; } = 15;
    public string RequestedBy { get; set; } = "system";
    public DateTime RequestedUtc { get; set; } = DateTime.UtcNow;
}