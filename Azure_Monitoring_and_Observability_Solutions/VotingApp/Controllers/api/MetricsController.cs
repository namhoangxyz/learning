using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace VotingApp.Controllers.api;

[Route("api/[controller]")]
[ApiController]
public class MetricsController : Controller
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    private readonly ILogger<MetricsController> _logger;
    private Metric _weatherSummaryMetric;
    
    public MetricsController(ILogger<MetricsController> logger, TelemetryConfiguration configuration)
    {
        _logger = logger;
         _weatherSummaryMetric = new TelemetryClient(configuration).GetMetric("Weather Counters","Summary");      
    }

    [HttpGet]
    public async Task<ActionResult<string>> Get()
    {
          
      var summary = Summaries[new Random().Next(Summaries.Length)];
      _weatherSummaryMetric.TrackValue(1,summary);

      var output = $"Generated Weather Summary: {summary}, Activity.Id: {Activity.Current?.Id}";
      _logger.LogDebug(output);
    
      return new OkObjectResult(output);
    }
        
}