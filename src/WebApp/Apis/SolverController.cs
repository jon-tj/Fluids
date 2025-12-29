using Microsoft.AspNetCore.Mvc;
using FluidSim.Core;

namespace FluidSim.WebApp.Apis;

[Route("api/[controller]")]
[ApiController]
public class SolverController(ILogger<SolverController> logger) : ControllerBase
{
    private readonly ILogger<SolverController> logger = logger;
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


    // POST api/solver/{id}/parameters
    [HttpPost("{id}/parameters")]
    public void PostParameter(string id, [FromBody] UpdateParameterRequestBody request)
    {
        var solver = GetSolverById(id);
        if (solver is null)
            return;

        logger.LogInformation("Updating parameter {ParameterName} to {Value} for solver {SolverId}", request.ParameterName, request.Value, id);
        if (solver.Metadata.Parameters.ContainsKey(request.ParameterName))
        {
            solver.Metadata.Parameters[request.ParameterName].Value = request.Value;

        }
        foreach (var param in solver.Metadata.Parameters)
        {
            logger.LogInformation("Parameter {ParameterName} is now {Value}", param.Key, param.Value.Value);
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
        return SolverRegistry.All.FirstOrDefault(s => s.Id == id);
    }
}

public record UpdateRequestBody(string State);

public record UpdateParameterRequestBody(string ParameterName, float Value);