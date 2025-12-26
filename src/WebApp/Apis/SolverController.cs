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
        return GetSolverById(id)?.Metadata;
    }

    // POST api/solver/{id}/update
    [HttpPost("{id}/update")]
    public string Update(string id, [FromBody] UpdateRequestBody request)
    {
        var solver = GetSolverById(id);
        if (solver is null)
            return "";

        FluidState fs;
        if (string.IsNullOrEmpty(request.State))
        {
            fs = FluidState.UniformRandom(300, 1.0f, 10, 10, 0);
        }
        else
        {
            var bytes = Convert.FromBase64String(request.State ?? "");
            fs = FluidState.Deserialize(bytes);
        }

        FluidState next = solver.Step(fs);
        return Convert.ToBase64String(next.Serialize());
    }

    private IFluidSolver? GetSolverById(string id)
    {
        return SolverRegistry.All.FirstOrDefault(s => s.Id == id);
    }
}

public record UpdateRequestBody(string State);
