namespace FluidSim.Core;

public interface IFluidSolver
{
    string Id { get; }
    SolverMetadata Metadata { get; }

    FluidState Initialize();
    FluidState Step(FluidState state, float dt);
}