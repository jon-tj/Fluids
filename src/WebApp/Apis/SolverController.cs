using Microsoft.AspNetCore.Mvc;
using FluidSim.Core;

namespace FluidSim.WebApp.Apis;

[Route("api/[controller]")]
[ApiController]
public class SolverController : ControllerBase
{
    // GET: api/solver
    [HttpGet]
    public IEnumerable<SolverMetadata> Get()
    {
        return SolverRegistry.All.Select(s => s.Metadata);
    }

    // GET api/solver/Verlet
    [HttpGet("{id}")]
    public SolverMetadata? Get(string id)
    {
        return SolverRegistry.All.Where(s => s.Id == id).FirstOrDefault()?.Metadata;
    }
}
