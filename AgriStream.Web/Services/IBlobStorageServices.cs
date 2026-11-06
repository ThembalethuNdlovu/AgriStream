namespace AgriStream.Web.Services;

public class DroneImageInfo
{
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
    public long SizeBytes { get; set; }
    public DateTimeOffset? UploadedOn { get; set; }
    public string UploadedBy { get; set; } = "";
    public string FieldName { get; set; } = "";
}

public interface IBlobStorageService
{
    Task<string> UploadAsync(IFormFile file, string uploadedBy, string fieldName);
    Task<List<DroneImageInfo>> ListAsync();
}