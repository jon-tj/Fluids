using FluidSim.Core;

namespace FluidSim.WebApp;

public class SolverRegistry
{
    public List<IFluidSolver> All = [
            new Solvers.Verlet.VerletSolver(),
        ];

    public void ResetSolvers()
    {
        foreach (var solver in All)
            foreach (var param in solver.Metadata.Parameters.Values)
                param.Reset();
    }
}
