using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FluidSim.WebApp.Pages;

public class IndexModel(SolverRegistry registry) : PageModel
{
    public SolverRegistry registry { get; init; } = registry;
    public void OnGet()
    {

    }
}
