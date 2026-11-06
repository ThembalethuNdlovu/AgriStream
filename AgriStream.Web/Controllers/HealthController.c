using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AgriStream.Web.Controllers;

[AllowAnonymous]
public class HealthController : Controller
{
    private readonly IConfiguration _config;

    public HealthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("/health")]
    public async Task<IActionResult> Index()
    {
        var result = new Dictionary<string, string>();

        // 1) Azure SQL reachable?
        try
        {
            // Change "DefaultConnection" if your connection string uses another name
            await using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("SELECT 1", conn);
            await cmd.ExecuteScalarAsync();
            result["sql"] = "OK";
        }
        catch
        {
            result["sql"] = "FAILED";
        }

        // 2) Function App reachable? (host root, no key needed)
        try
        {
            var baseUrl = _config["Functions:BaseUrl"];
            var host = new Uri(baseUrl!).GetLeftPart(UriPartial.Authority);
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var response = await http.GetAsync(host);
            result["functions"] = response.IsSuccessStatusCode ? "OK" : $"HTTP {(int)response.StatusCode}";
        }
        catch
        {
            result["functions"] = "FAILED";
        }

        var healthy = result.Values.All(v => v == "OK");
        return StatusCode(healthy ? 200 : 503, result);
    }
}