using Azure;
using Azure.Data.Tables;

namespace AgriStream.Web.Models;

public class SensorReading : ITableEntity
{
    // PartitionKey = field name, RowKey = reverse-ticks so the newest reading sorts first
    public string PartitionKey { get; set; } = "";
    public string RowKey { get; set; } = "";
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string DeviceId { get; set; } = "";
    public double SoilMoisturePercent { get; set; }
    public double SoilTemperatureC { get; set; }
    public double HumidityPercent { get; set; }
    public double Ph { get; set; }
    public DateTimeOffset ReadingTimeUtc { get; set; }

    public static SensorReading Create(string field, string deviceId, double moisture,
        double temp, double humidity, double ph, DateTimeOffset readingTime)
    {
        var reverseTicks = (DateTimeOffset.MaxValue.UtcTicks - readingTime.UtcTicks).ToString("D19");
        return new SensorReading
        {
            PartitionKey = field,
            RowKey = $"{reverseTicks}_{Guid.NewGuid():N}".Substring(0, 32),
            DeviceId = deviceId,
            SoilMoisturePercent = moisture,
            SoilTemperatureC = temp,
            HumidityPercent = humidity,
            Ph = ph,
            ReadingTimeUtc = readingTime
        };
    }
}