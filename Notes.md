## Commands

Add a new solver by creating a new project. Since both Core and the WebApp depend on the solvers, we have to link the new project to both.

```bash
solver=Verlet
ord_name=1_$solver
dotnet new classlib -n $solver -o src/$ord_name
dotnet add src/$ord_name/$solver.csproj reference src/Core/FluidSim.Core.csproj
dotnet add src/WebApp/FluidSim.WebApp.csproj reference src/$ord_name/$solver.csproj
dotnet sln FluidSim.sln add src/$ord_name/$solver.csproj
```
