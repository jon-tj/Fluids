using FluidSim.Core;
using FluidSim.Core.Units;

namespace FluidSim.Solvers.Verlet;

public class StableFluidsSolver : IFluidSolver
{
    public string Id => "StableFluids";

    public SolverMetadata Metadata =>
        new(
            displayName: "Verlet",
            description: "Solve a particle simulation with high density using chunking.",
            parameters: new()
            {
                ["Substeps"] = new DisplayParameter(1, new Interval(1, 10), ParameterDomain.Integer),
                ["Time Step"] = new DisplayParameter(0.05f, Intervals.Unit, ParameterDomain.Decimal, Units.Second),
                ["Radius"] = new DisplayParameter(0.05f, Intervals.Unit, ParameterDomain.Decimal, Units.Meter),
                ["Chunk Size"] = new DisplayParameter(0.05f, new Interval(0, 1), ParameterDomain.Decimal, Units.Meter),
                ["Initial Skew"] = new DisplayParameter(0.05f, new Interval(0, 1), ParameterDomain.Decimal),
            }
        );

    public FluidState Initialize()
    {
        // allocate grid
        int w = 128;
        int h = 128;
        return new FluidState(new float[w * h], w, h);
    }

    public FluidState Step(FluidState state, float dt)
    {
        // apply advection, diffusion, projection, etc
        return state; // updated
    }
}
