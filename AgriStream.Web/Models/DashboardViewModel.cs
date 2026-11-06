using System.Text.Json;

namespace AgriStream.Web.Models;

public class DashboardViewModel
{
    public string? Field { get; set; }
    public List<Dictionary<string, JsonElement>> Readings { get; set; } = new();
}