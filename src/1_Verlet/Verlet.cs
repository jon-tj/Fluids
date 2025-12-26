using FluidSim.Core;
using FluidSim.Core.Units;

namespace FluidSim.Solvers.Verlet;

public class StableFluidsSolver : IFluidSolver
{
    public string Id => "Verlet";

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
                // ["Initial Skew"] = new DisplayParameter(0.05f, new Interval(0, 1), ParameterDomain.Decimal),
            }
        );

    public FluidState Step(FluidState state)
    {
        // apply advection, diffusion, projection, etc
        for (int i = 0; i < (int)Metadata.Parameters["Substeps"].Value; i++)
        {
            // perform a single substep
            float dt = Metadata.Parameters["Time Step"].Value;
            float radius = Metadata.Parameters["Radius"].Value;
            float chunkSize = Metadata.Parameters["Chunk Size"].Value;
            Vector3 acceleration = Vector3.Gravity;
            float dt2 = dt * dt;
            for (int j = 0; j < state.Particles.Length; j++)
            {
                Particle p = state.Particles[j];
                // verlet integration
                p.Position = p.Position + p.Velocity * dt + acceleration * dt2;
                state.Particles[j] = p;
            }
        }
        return state; // updated
    }
}
