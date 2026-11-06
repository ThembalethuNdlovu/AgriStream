using AgriStream.Web.Models;
using AgriStream.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriStream.Web.Controllers;

[Authorize(Roles = "Farmer,Agronomist,Admin")]
public class TelemetryController : Controller
{
    private readonly ITelemetryService _telemetry;
    public TelemetryController(ITelemetryService telemetry) => _telemetry = telemetry;

    public async Task<IActionResult> Index(string? field)
    {
        var fields = TelemetryService.DemoFields;
        var selected = fields.Contains(field) ? field! : fields[0];

        return View(new TelemetryViewModel
        {
            SelectedField = selected,
            Fields = fields,
            Readings = await _telemetry.GetLatestAsync(selected, 20)
        });
    }

    // Simulates a burst of sensor readings (stand-in for real devices)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Simulate(string field)
    {
        if (!TelemetryService.DemoFields.Contains(field))
            return RedirectToAction(nameof(Index));

        var rnd = Random.Shared;
        var now = DateTimeOffset.UtcNow;

        var readings = Enumerable.Range(0, 20).Select(i => SensorReading.Create(
            field,
            $"SENSOR-{rnd.Next(1, 6):000}",
            Math.Round(rnd.NextDouble() * 60 + 15, 1),   // moisture 15-75 %
            Math.Round(rnd.NextDouble() * 15 + 12, 1),   // soil temp 12-27 C
            Math.Round(rnd.NextDouble() * 40 + 40, 1),   // humidity 40-80 %
            Math.Round(rnd.NextDouble() * 2 + 5.5, 1),   // pH 5.5-7.5
            now.AddMinutes(-i * 5))).ToList();

        await _telemetry.AddReadingsAsync(readings);
        TempData["Success"] = $"20 simulated readings stored for {field}.";
        return RedirectToAction(nameof(Index), new { field });
    }
}