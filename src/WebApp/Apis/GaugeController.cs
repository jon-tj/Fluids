using Microsoft.AspNetCore.Mvc;
using FluidSim.Core;
using FluidSim.Core.Gauges;

namespace FluidSim.WebApp.Apis;

[Route("api/[controller]")]
[ApiController]
public class GaugeController(ILogger<GaugeController> logger, GaugeRegistry gauges) : ControllerBase
{
    private readonly ILogger<GaugeController> logger = logger;
    private readonly GaugeRegistry gauges = gauges;
    // GET: api/gauge
    [HttpGet]
    public IEnumerable<ParticleGaugeMetadata> Get()
    {
        return gauges.All.Select(s => s.Metadata);
    }

    // GET api/gauge/Velocity
    [HttpPost("{id}")]
    public ParticleGaugeResult[] Gauge(string id, [FromBody] UpdateRequestBody request)
    {
        var gauge = gauges.All.FirstOrDefault(s => s.Metadata.Id == id);

        var bytes = Convert.FromBase64String(request.State ?? "");
        var fs = FluidState.Deserialize(bytes);

        return gauge?.Gauge(fs) ?? [];
    }
}

public record GaugeRequestBody(string State);