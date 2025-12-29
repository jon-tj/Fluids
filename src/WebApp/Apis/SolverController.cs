using Microsoft.AspNetCore.Mvc;
using FluidSim.Core;

namespace FluidSim.WebApp.Apis;

[Route("api/[controller]")]
[ApiController]
public class SolverController(ILogger<SolverController> logger, SolverRegistry registry) : ControllerBase
{
    private readonly ILogger<SolverController> logger = logger;
    private readonly SolverRegistry registry = registry;
    // GET: api/solver
    [HttpGet]
    public IEnumerable<SolverMetadata> Get(bool? reset = false)
    {
        if (reset == true)
        {
            registry.ResetSolvers();
            logger.LogInformation("Solver registry reset requested via API.");
        }
        return registry.All.Select(s => s.Metadata);
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


    // POST api/solver/{id}/parameters
    [HttpPost("{id}/parameters")]
    public void PostParameter(string id, [FromBody] UpdateParameterRequestBody request)
    {
        var solver = GetSolverById(id);
        if (solver is null)
            return;

        if (solver.Metadata.Parameters.ContainsKey(request.ParameterName))
        {
            solver.Metadata.Parameters[request.ParameterName].Value = request.Value;
        }
    }

    // GET api/solver/{id}/parameters
    [HttpGet("{id}/parameters")]
    public Dictionary<string, float> GetParameter(string id)
    {
        var solver = GetSolverById(id);
        if (solver is null)
            return new Dictionary<string, float>();

        return solver.Metadata.Parameters.ToDictionary(kv => kv.Key, kv => kv.Value.Value);
    }

    private IFluidSolver? GetSolverById(string id)
    {
        return registry.All.FirstOrDefault(s => s.Id == id);
    }
}

public record UpdateRequestBody(string State);

public record UpdateParameterRequestBody(string ParameterName, float Value);