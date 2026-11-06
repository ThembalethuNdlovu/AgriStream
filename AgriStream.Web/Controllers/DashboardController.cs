using AgriStream.Web.Models;
using AgriStream.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriStream.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IFunctionsClient _functions;

    public DashboardController(IFunctionsClient functions) => _functions = functions;

    public async Task<IActionResult> Index(string? field)
    {
        var model = new DashboardViewModel { Field = field };
        if (!string.IsNullOrWhiteSpace(field))
        {
            try { model.Readings = await _functions.GetLatestTelemetryAsync(field); }
            catch (HttpRequestException) { TempData["Error"] = "Could not reach the telemetry function."; }
        }
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Irrigate(int deviceId, string field, int minutes = 15)
    {
        try
        {
            var result = await _functions.TriggerIrrigationAsync(deviceId, field, minutes, User.Identity?.Name ?? "unknown");
            TempData["Message"] = $"Irrigation command queued (ID {result?.CommandId}).";
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Could not queue the irrigation command.";
        }
        return RedirectToAction(nameof(Index), new { field });
    }
}