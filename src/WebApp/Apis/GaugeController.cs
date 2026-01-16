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

    // GET api/gauge/
    [HttpPost]
    public IEnumerable<GaugeResponse> Gauge([FromBody] GaugeRequestBody request)
    {
        var gauge = gauges.All.Where(s => request.GaugeIds.Contains(s.Metadata.Id));

        var bytes = Convert.FromBase64String(request.State ?? "");
        var fs = FluidState.Deserialize(bytes);

        return gauge.Select(g => new GaugeResponse(g.Metadata.Id, g.Gauge(fs)));
    }
}

public record GaugeRequestBody(string State, string[] GaugeIds);
public record GaugeResponse(string GaugeId, ParticleGaugeResult[] Result);