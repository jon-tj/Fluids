using FluidSim.Core;
using FluidSim.Solvers;

namespace FluidSim.WebApp;

public static class SolverRegistry
{
    public static readonly List<IFluidSolver> All =
    [
        new Solvers.Verlet.StableFluidsSolver(),
    ];
}
