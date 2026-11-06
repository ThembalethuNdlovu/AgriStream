using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace AgriStream.Web.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _container;

    public BlobStorageService(BlobServiceClient serviceClient, IConfiguration config)
    {
        var name = config["AzureStorage:DroneImagesContainer"] ?? "drone-images";
        _container = serviceClient.GetBlobContainerClient(name);
        _container.CreateIfNotExists(PublicAccessType.None);
    }

    public async Task<string> UploadAsync(IFormFile file, string uploadedBy, string fieldName)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var blobName = $"{DateTime.UtcNow:yyyyMMdd}/{Guid.NewGuid()}{ext}";
        var blob = _container.GetBlobClient(blobName);

        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType },
            Metadata = new Dictionary<string, string>
            {
                ["uploadedBy"] = uploadedBy,
                ["fieldName"] = string.IsNullOrWhiteSpace(fieldName) ? "unspecified" : fieldName,
                ["originalName"] = Uri.EscapeDataString(file.FileName)
            }
        };

        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, options);
        return blobName;
    }

    public async Task<List<DroneImageInfo>> ListAsync()
    {
        var result = new List<DroneImageInfo>();
        await foreach (var item in _container.GetBlobsAsync(new GetBlobsOptions { Traits = BlobTraits.Metadata }))
        {
            var sas = _container.GetBlobClient(item.Name)
                .GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(30));

            item.Metadata.TryGetValue("uploadedBy", out var by);
            item.Metadata.TryGetValue("fieldName", out var field);

            result.Add(new DroneImageInfo
            {
                Name = item.Name,
                Url = sas.ToString(),
                SizeBytes = item.Properties.ContentLength ?? 0,
                UploadedOn = item.Properties.CreatedOn,
                UploadedBy = by ?? "",
                FieldName = field ?? ""
            });
        }
        return result.OrderByDescending(r => r.UploadedOn).ToList();
    }
}