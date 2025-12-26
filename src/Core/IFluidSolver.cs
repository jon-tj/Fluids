namespace FluidSim.Core;

public interface IFluidSolver
{
    string Id { get; }
    SolverMetadata Metadata { get; }

    FluidState Step(FluidState state);
}