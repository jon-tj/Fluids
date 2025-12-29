using FluidSim.Core;

namespace FluidSim.WebApp;

public class SolverRegistry
{
    public List<IFluidSolver> All;
    public SolverRegistry()
    {
        All = ResetSolvers();
    }

    public List<IFluidSolver> ResetSolvers()
    {
        All = [
            new Solvers.Verlet.VerletSolver(),
        ];
        return All;
    }
}
