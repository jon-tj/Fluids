using FluidSim.Core;
using FluidSim.Core.Units;

namespace FluidSim.Solvers.DensityField;

/// <summary>
/// SPH (Smoothed Particle Hydrodynamics) based fluid solver that uses
/// density field estimation to compute pressure forces between particles.
/// </summary>
public class DensityFieldSolver : IFluidSolver
{
    public string Id => "DensityField";
    public SolverMetadata Metadata => md;
    public SolverMetadata md =
        new(
            displayName: "Density Field (SPH)",
            description: """
            Smoothed Particle Hydrodynamics solver that estimates local density
            at each particle position and computes pressure and viscosity forces.
            The smoothing radius determines the neighborhood size for density estimation.
            """,
            parameters: new()
            {
                ["Substeps"] = new DisplayParameter(4, new Interval(1, 20), ParameterDomain.Integer),
                ["Particles"] = new DisplayParameter(200, new Interval(1, 1500), ParameterDomain.Integer),
                ["Time Step"] = new DisplayParameter(0.016f, new Interval(0.001f, 0.1f), ParameterDomain.Decimal, Units.Second),
                ["Smoothing Radius"] = new DisplayParameter(0.5f, new Interval(0.1f, 2f), ParameterDomain.Decimal, Units.Meter),
                ["Rest Density"] = new DisplayParameter(1000f, new Interval(100f, 5000f), ParameterDomain.Decimal),
                ["Gas Constant"] = new DisplayParameter(500f, new Interval(10f, 5000f), ParameterDomain.Decimal),
                ["Viscosity"] = new DisplayParameter(50f, new Interval(0f, 500f), ParameterDomain.Decimal),
                ["Gravity"] = new DisplayParameter(9.81f, new Interval(0, 30), ParameterDomain.Decimal, Units.MetersPerSecond2),
                ["Particle Radius"] = new DisplayParameter(0.15f, new Interval(0.05f, 0.5f), ParameterDomain.Decimal, Units.Meter),
            }
        );

    // Precomputed kernel coefficients (set based on smoothing radius)
    private float poly6Coeff;
    private float spikyGradCoeff;
    private float viscosityLapCoeff;

    public FluidState Step(FluidState state)
    {
        // Handle particle count changes
        if (md.Parameters["Particles"].ValueChanged() is ValueChangedEventArgs particleChangedEvent)
        {
            int targetCount = (int)particleChangedEvent.NewValue;
            int currentCount = state.Particles.Count;

            if (targetCount > currentCount)
            {
                int toAdd = targetCount - currentCount;
                Random rand = new Random();
                for (int i = 0; i < toAdd; i++)
                {
                    Particle p = state.Particles[0].Clone();
                    p.Position = new Vector3(
                        (float)(rand.NextDouble() * state.Width * 0.8f + state.Width * 0.1f),
                        (float)(rand.NextDouble() * state.Height * 0.8f + state.Height * 0.1f),
                        (float)(rand.NextDouble() * state.Depth * 0.8f + state.Depth * 0.1f)
                    );
                    p.Velocity = Vector3.Zero;
                    state.Particles.Add(p);
                }
            }
            else if (targetCount < currentCount)
            {
                int toRemove = currentCount - targetCount;
                state.Particles.RemoveRange(state.Particles.Count - toRemove, toRemove);
            }
        }

        int substeps = (int)md.Parameters["Substeps"].Value;
        float dt = md.Parameters["Time Step"].Value / substeps;
        float h = md.Parameters["Smoothing Radius"].Value;
        float restDensity = md.Parameters["Rest Density"].Value;
        float gasConstant = md.Parameters["Gas Constant"].Value;
        float viscosity = md.Parameters["Viscosity"].Value;
        float gravity = md.Parameters["Gravity"].Value;
        float particleRadius = md.Parameters["Particle Radius"].Value;

        // Precompute kernel coefficients
        float h2 = h * h;
        float h3 = h2 * h;
        float h6 = h3 * h3;
        float h9 = h6 * h3;
        poly6Coeff = 315f / (64f * MathF.PI * h9);
        spikyGradCoeff = -45f / (MathF.PI * h6);
        viscosityLapCoeff = 45f / (MathF.PI * h6);

        int n = state.Particles.Count;
        float[] densities = new float[n];
        float[] pressures = new float[n];
        Vector3[] forces = new Vector3[n];

        for (int step = 0; step < substeps; step++)
        {
            // Build spatial hash for neighbor searching
            var grid = BuildSpatialHash(state.Particles, h);

            // --- Compute density for each particle ---
            for (int i = 0; i < n; i++)
            {
                densities[i] = ComputeDensity(state.Particles, i, grid, h, h2);
            }

            // --- Compute pressure from density ---
            for (int i = 0; i < n; i++)
            {
                // Equation of state: p = k * (rho - rho0)
                pressures[i] = gasConstant * (densities[i] - restDensity);
                if (pressures[i] < 0) pressures[i] = 0; // Clamp negative pressure
            }

            // --- Compute forces (pressure + viscosity + gravity) ---
            for (int i = 0; i < n; i++)
            {
                forces[i] = ComputeForces(state.Particles, i, densities, pressures, grid, h, h2, viscosity);
                // Add gravity
                forces[i] += Vector3.Down * gravity * densities[i];
            }

            // --- Integration (symplectic Euler) ---
            for (int i = 0; i < n; i++)
            {
                Particle p = state.Particles[i];
                
                // Acceleration = force / density
                Vector3 acceleration = forces[i] / MathF.Max(densities[i], 0.001f);
                
                // Update velocity
                p.Velocity += acceleration * dt;
                
                // Damping for stability
                p.Velocity *= 0.999f;
                
                // Update position
                p.PreviousPosition = p.Position;
                p.Position += p.Velocity * dt;

                state.Particles[i] = p;
            }

            // --- Boundary conditions ---
            for (int i = 0; i < n; i++)
            {
                Particle p = state.Particles[i];
                float damping = 0.3f;

                // Floor
                if (p.Position.y < particleRadius)
                {
                    p.Position.y = particleRadius;
                    p.Velocity.y *= -damping;
                }
                // Ceiling
                if (p.Position.y > state.Height - particleRadius)
                {
                    p.Position.y = state.Height - particleRadius;
                    p.Velocity.y *= -damping;
                }
                // Left wall
                if (p.Position.x < particleRadius)
                {
                    p.Position.x = particleRadius;
                    p.Velocity.x *= -damping;
                }
                // Right wall
                if (p.Position.x > state.Width - particleRadius)
                {
                    p.Position.x = state.Width - particleRadius;
                    p.Velocity.x *= -damping;
                }
                // Front wall
                if (p.Position.z < particleRadius)
                {
                    p.Position.z = particleRadius;
                    p.Velocity.z *= -damping;
                }
                // Back wall
                if (p.Position.z > state.Depth - particleRadius)
                {
                    p.Position.z = state.Depth - particleRadius;
                    p.Velocity.z *= -damping;
                }

                state.Particles[i] = p;
            }
        }

        return state;
    }

    /// <summary>
    /// Poly6 smoothing kernel for density estimation
    /// </summary>
    private float Poly6Kernel(float r2, float h2)
    {
        if (r2 >= h2) return 0f;
        float diff = h2 - r2;
        return poly6Coeff * diff * diff * diff;
    }

    /// <summary>
    /// Spiky kernel gradient for pressure force
    /// </summary>
    private Vector3 SpikyGradient(Vector3 r, float rMag, float h)
    {
        if (rMag >= h || rMag < 0.0001f) return Vector3.Zero;
        float diff = h - rMag;
        float coeff = spikyGradCoeff * diff * diff / rMag;
        return r * coeff;
    }

    /// <summary>
    /// Viscosity kernel Laplacian for viscosity force
    /// </summary>
    private float ViscosityLaplacian(float r, float h)
    {
        if (r >= h) return 0f;
        return viscosityLapCoeff * (h - r);
    }

    /// <summary>
    /// Compute density at particle i using SPH interpolation
    /// </summary>
    private float ComputeDensity(List<Particle> particles, int i, Dictionary<(int, int, int), List<int>> grid, float h, float h2)
    {
        float density = 0f;
        Particle pi = particles[i];
        var neighbors = GetNeighborIndices(pi.Position, grid, h);

        foreach (int j in neighbors)
        {
            Particle pj = particles[j];
            Vector3 r = pi.Position - pj.Position;
            float r2 = r.x * r.x + r.y * r.y + r.z * r.z;
            density += pj.Mass * Poly6Kernel(r2, h2);
        }

        return MathF.Max(density, 0.001f);
    }

    /// <summary>
    /// Compute pressure and viscosity forces on particle i
    /// </summary>
    private Vector3 ComputeForces(List<Particle> particles, int i, float[] densities, float[] pressures,
        Dictionary<(int, int, int), List<int>> grid, float h, float h2, float viscosity)
    {
        Vector3 pressureForce = Vector3.Zero;
        Vector3 viscosityForce = Vector3.Zero;

        Particle pi = particles[i];
        var neighbors = GetNeighborIndices(pi.Position, grid, h);

        foreach (int j in neighbors)
        {
            if (i == j) continue;

            Particle pj = particles[j];
            Vector3 r = pi.Position - pj.Position;
            float rMag = r.Magnitude();

            if (rMag > h || rMag < 0.0001f) continue;

            // Pressure force (symmetric formulation)
            float pressureTerm = (pressures[i] + pressures[j]) / (2f * densities[j]);
            pressureForce -= SpikyGradient(r, rMag, h) * (pj.Mass * pressureTerm);

            // Viscosity force
            Vector3 vDiff = pj.Velocity - pi.Velocity;
            viscosityForce += vDiff * (viscosity * pj.Mass * ViscosityLaplacian(rMag, h) / densities[j]);
        }

        return pressureForce + viscosityForce;
    }

    /// <summary>
    /// Build spatial hash grid for efficient neighbor searching
    /// </summary>
    private Dictionary<(int, int, int), List<int>> BuildSpatialHash(List<Particle> particles, float cellSize)
    {
        var grid = new Dictionary<(int, int, int), List<int>>();

        for (int i = 0; i < particles.Count; i++)
        {
            var key = GetCellKey(particles[i].Position, cellSize);
            if (!grid.ContainsKey(key))
                grid[key] = new List<int>();
            grid[key].Add(i);
        }

        return grid;
    }

    /// <summary>
    /// Get cell key for a position
    /// </summary>
    private (int, int, int) GetCellKey(Vector3 pos, float cellSize)
    {
        return (
            (int)MathF.Floor(pos.x / cellSize),
            (int)MathF.Floor(pos.y / cellSize),
            (int)MathF.Floor(pos.z / cellSize)
        );
    }

    /// <summary>
    /// Get all particle indices in neighboring cells (including current cell)
    /// </summary>
    private IEnumerable<int> GetNeighborIndices(Vector3 pos, Dictionary<(int, int, int), List<int>> grid, float cellSize)
    {
        var center = GetCellKey(pos, cellSize);
        
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    var key = (center.Item1 + dx, center.Item2 + dy, center.Item3 + dz);
                    if (grid.TryGetValue(key, out var indices))
                    {
                        foreach (int idx in indices)
                            yield return idx;
                    }
                }
            }
        }
    }
}
