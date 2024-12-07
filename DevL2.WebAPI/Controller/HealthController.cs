using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DevL2.WebAPI.Controller;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }
    
    [HttpGet("health")]
    public async Task<IActionResult> GetHealth()
    {
        var healthReport = await _healthCheckService.CheckHealthAsync();

        var response = new
        {
            status = healthReport.Status.ToString(),
            results = healthReport.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    details = entry.Value.Data
                })
        };

        return Ok(response);
    }
}