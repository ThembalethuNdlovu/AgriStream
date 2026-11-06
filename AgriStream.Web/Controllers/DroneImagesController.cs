using AgriStream.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriStream.Web.Controllers;

[Authorize(Roles = "Farmer,Agronomist,Admin")]
public class DroneImagesController : Controller
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".tif", ".tiff" };
    private const long MaxBytes = 20 * 1024 * 1024; // 20 MB

    private readonly IBlobStorageService _blobs;
    public DroneImagesController(IBlobStorageService blobs) => _blobs = blobs;

    public async Task<IActionResult> Index() => View(await _blobs.ListAsync());

    [HttpGet]
    public IActionResult Upload() => View();

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxBytes)]
    public async Task<IActionResult> Upload(IFormFile? file, string? fieldName)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Please choose an image to upload.");
            return View();
        }
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            ModelState.AddModelError("", "Only JPG, PNG or TIFF images are allowed.");
            return View();
        }
        if (file.Length > MaxBytes)
        {
            ModelState.AddModelError("", "File is too large (max 20 MB).");
            return View();
        }

        await _blobs.UploadAsync(file, User.Identity?.Name ?? "unknown", fieldName ?? "");
        TempData["Success"] = "Drone image uploaded successfully.";
        return RedirectToAction(nameof(Index));
    }
}