using FluidSim.Core;
using FluidSim.Core.Units;

namespace FluidSim.Solvers.GridFlow;

/// <summary>
/// Eulerian grid-based fluid simulation using the stable fluids method.
/// Solves the incompressible Navier-Stokes equations on a fixed grid.
/// Particles are advected through the velocity field for visualization.
/// </summary>
public class GridFlowSolver : IFluidSolver
{
    public string Id => "GridFlow";
    public SolverMetadata Metadata => md;
    public SolverMetadata md =
        new(
            id: "GridFlow",
            displayName: "Grid Flow (Eulerian)",
            description: """
            Eulerian grid-based fluid solver using Jos Stam's stable fluids method.
            Simulates velocity field on a grid with advection, diffusion, and pressure projection.
            Particles are passively advected through the field for visualization.
            """,
            parameters: new()
            {
                ["Grid Size"] = new DisplayParameter(32, new Interval(8, 128), ParameterDomain.Integer),
                ["Particles"] = new DisplayParameter(500, new Interval(1, 2000), ParameterDomain.Integer),
                ["Time Step"] = new DisplayParameter(0.1f, new Interval(0.01f, 0.5f), ParameterDomain.Decimal, Units.Second),
                ["Viscosity"] = new DisplayParameter(0.0001f, new Interval(0f, 0.01f), ParameterDomain.Decimal),
                ["Diffusion"] = new DisplayParameter(0.0001f, new Interval(0f, 0.01f), ParameterDomain.Decimal),
                ["Force Strength"] = new DisplayParameter(100f, new Interval(0f, 500f), ParameterDomain.Decimal),
                ["Iterations"] = new DisplayParameter(20, new Interval(1, 50), ParameterDomain.Integer),
                ["Gravity"] = new DisplayParameter(0f, new Interval(-20, 20), ParameterDomain.Decimal, Units.MetersPerSecond2),
                ["Source X"] = new DisplayParameter(0.5f, new Interval(0f, 1f), ParameterDomain.Decimal),
                ["Source Y"] = new DisplayParameter(0.2f, new Interval(0f, 1f), ParameterDomain.Decimal),
            }
        );

    // Velocity field (MAC grid - staggered)
    private float[]? vx;      // x-velocity
    private float[]? vy;      // y-velocity
    private float[]? vx_prev;
    private float[]? vy_prev;
    private int gridN;
    private int gridSize;

    public FluidState Step(FluidState state)
    {
        int N = (int)md.Parameters["Grid Size"].Value;
        float dt = md.Parameters["Time Step"].Value;
        float visc = md.Parameters["Viscosity"].Value;
        float diff = md.Parameters["Diffusion"].Value;
        float forceStrength = md.Parameters["Force Strength"].Value;
        int iterations = (int)md.Parameters["Iterations"].Value;
        float gravity = md.Parameters["Gravity"].Value;
        float sourceX = md.Parameters["Source X"].Value;
        float sourceY = md.Parameters["Source Y"].Value;

        // Initialize or reinitialize grid if size changed
        if (vx == null || gridN != N)
        {
            InitializeGrid(N);
        }

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
                        (float)(rand.NextDouble() * state.Width),
                        (float)(rand.NextDouble() * state.Height),
                        (float)(rand.NextDouble() * state.Depth)
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

        // Add external forces (gravity + source)
        AddForces(N, dt, forceStrength, gravity, sourceX, sourceY, state.Width, state.Height);

        // Velocity step: diffuse then advect then project
        VelocityStep(N, visc, dt, iterations);

        // Advect particles through the velocity field
        AdvectParticles(state, N, dt);

        return state;
    }

    private void InitializeGrid(int N)
    {
        gridN = N;
        gridSize = (N + 2) * (N + 2); // Include boundary cells

        vx = new float[gridSize];
        vy = new float[gridSize];
        vx_prev = new float[gridSize];
        vy_prev = new float[gridSize];
    }

    private int IX(int i, int j) => i + (gridN + 2) * j;

    private void AddForces(int N, float dt, float forceStrength, float gravity, float srcX, float srcY, float width, float height)
    {
        // Add gravity to entire field
        if (gravity != 0)
        {
            for (int i = 1; i <= N; i++)
            {
                for (int j = 1; j <= N; j++)
                {
                    vy![IX(i, j)] += gravity * dt;
                }
            }
        }

        // Add upward force at source location
        if (forceStrength > 0)
        {
            int srcI = (int)(srcX * N) + 1;
            int srcJ = (int)(srcY * N) + 1;
            srcI = Math.Clamp(srcI, 1, N);
            srcJ = Math.Clamp(srcJ, 1, N);

            // Add force in a small radius
            int radius = Math.Max(1, N / 16);
            for (int di = -radius; di <= radius; di++)
            {
                for (int dj = -radius; dj <= radius; dj++)
                {
                    int i = srcI + di;
                    int j = srcJ + dj;
                    if (i >= 1 && i <= N && j >= 1 && j <= N)
                    {
                        float dist = MathF.Sqrt(di * di + dj * dj);
                        float falloff = MathF.Max(0, 1 - dist / (radius + 1));
                        vy![IX(i, j)] += forceStrength * falloff * dt;
                    }
                }
            }
        }
    }

    private void VelocityStep(int N, float visc, float dt, int iterations)
    {
        // Diffuse velocity
        Diffuse(N, 1, vx_prev!, vx!, visc, dt, iterations);
        Diffuse(N, 2, vy_prev!, vy!, visc, dt, iterations);

        // Project to make velocity field divergence-free
        Project(N, vx_prev!, vy_prev!, vx!, vy!, iterations);

        // Advect velocity field through itself
        Advect(N, 1, vx!, vx_prev!, vx_prev!, vy_prev!, dt);
        Advect(N, 2, vy!, vy_prev!, vx_prev!, vy_prev!, dt);

        // Project again
        Project(N, vx!, vy!, vx_prev!, vy_prev!, iterations);
    }

    private void Diffuse(int N, int b, float[] x, float[] x0, float diff, float dt, int iterations)
    {
        float a = dt * diff * N * N;
        LinearSolve(N, b, x, x0, a, 1 + 4 * a, iterations);
    }

    private void LinearSolve(int N, int b, float[] x, float[] x0, float a, float c, int iterations)
    {
        float cRecip = 1f / c;

        for (int k = 0; k < iterations; k++)
        {
            for (int j = 1; j <= N; j++)
            {
                for (int i = 1; i <= N; i++)
                {
                    x[IX(i, j)] = (x0[IX(i, j)] +
                        a * (x[IX(i - 1, j)] + x[IX(i + 1, j)] +
                             x[IX(i, j - 1)] + x[IX(i, j + 1)])) * cRecip;
                }
            }
            SetBoundary(N, b, x);
        }
    }

    private void Advect(int N, int b, float[] d, float[] d0, float[] velocX, float[] velocY, float dt)
    {
        float dt0 = dt * N;

        for (int j = 1; j <= N; j++)
        {
            for (int i = 1; i <= N; i++)
            {
                float x = i - dt0 * velocX[IX(i, j)];
                float y = j - dt0 * velocY[IX(i, j)];

                // Clamp to grid bounds
                x = MathF.Max(0.5f, MathF.Min(N + 0.5f, x));
                y = MathF.Max(0.5f, MathF.Min(N + 0.5f, y));

                int i0 = (int)x;
                int i1 = i0 + 1;
                int j0 = (int)y;
                int j1 = j0 + 1;

                float s1 = x - i0;
                float s0 = 1 - s1;
                float t1 = y - j0;
                float t0 = 1 - t1;

                // Clamp indices
                i0 = Math.Clamp(i0, 0, N + 1);
                i1 = Math.Clamp(i1, 0, N + 1);
                j0 = Math.Clamp(j0, 0, N + 1);
                j1 = Math.Clamp(j1, 0, N + 1);

                d[IX(i, j)] = s0 * (t0 * d0[IX(i0, j0)] + t1 * d0[IX(i0, j1)]) +
                              s1 * (t0 * d0[IX(i1, j0)] + t1 * d0[IX(i1, j1)]);
            }
        }
        SetBoundary(N, b, d);
    }

    private void Project(int N, float[] velocX, float[] velocY, float[] p, float[] div, int iterations)
    {
        // Compute divergence
        float h = 1f / N;
        for (int j = 1; j <= N; j++)
        {
            for (int i = 1; i <= N; i++)
            {
                div[IX(i, j)] = -0.5f * h * (
                    velocX[IX(i + 1, j)] - velocX[IX(i - 1, j)] +
                    velocY[IX(i, j + 1)] - velocY[IX(i, j - 1)]);
                p[IX(i, j)] = 0;
            }
        }
        SetBoundary(N, 0, div);
        SetBoundary(N, 0, p);

        // Solve pressure Poisson equation
        LinearSolve(N, 0, p, div, 1, 4, iterations);

        // Subtract pressure gradient from velocity
        for (int j = 1; j <= N; j++)
        {
            for (int i = 1; i <= N; i++)
            {
                velocX[IX(i, j)] -= 0.5f * N * (p[IX(i + 1, j)] - p[IX(i - 1, j)]);
                velocY[IX(i, j)] -= 0.5f * N * (p[IX(i, j + 1)] - p[IX(i, j - 1)]);
            }
        }
        SetBoundary(N, 1, velocX);
        SetBoundary(N, 2, velocY);
    }

    private void SetBoundary(int N, int b, float[] x)
    {
        // Handle edges
        for (int i = 1; i <= N; i++)
        {
            x[IX(0, i)] = b == 1 ? -x[IX(1, i)] : x[IX(1, i)];
            x[IX(N + 1, i)] = b == 1 ? -x[IX(N, i)] : x[IX(N, i)];
            x[IX(i, 0)] = b == 2 ? -x[IX(i, 1)] : x[IX(i, 1)];
            x[IX(i, N + 1)] = b == 2 ? -x[IX(i, N)] : x[IX(i, N)];
        }

        // Handle corners
        x[IX(0, 0)] = 0.5f * (x[IX(1, 0)] + x[IX(0, 1)]);
        x[IX(0, N + 1)] = 0.5f * (x[IX(1, N + 1)] + x[IX(0, N)]);
        x[IX(N + 1, 0)] = 0.5f * (x[IX(N, 0)] + x[IX(N + 1, 1)]);
        x[IX(N + 1, N + 1)] = 0.5f * (x[IX(N, N + 1)] + x[IX(N + 1, N)]);
    }

    private void AdvectParticles(FluidState state, int N, float dt)
    {
        float cellWidth = state.Width / N;
        float cellHeight = state.Height / N;

        for (int i = 0; i < state.Particles.Count; i++)
        {
            Particle p = state.Particles[i];

            // Get grid cell for this particle
            float gx = p.Position.x / cellWidth;
            float gy = p.Position.y / cellHeight;

            // Bilinear interpolation of velocity
            Vector3 vel = InterpolateVelocity(gx, gy, N, cellWidth, cellHeight);

            // Update particle
            p.PreviousPosition = p.Position;
            p.Velocity = vel;
            p.Position += vel * dt;

            // Boundary conditions - wrap around or bounce
            float padding = 0.1f;
            if (p.Position.x < padding)
            {
                p.Position.x = padding;
                p.Velocity.x = MathF.Abs(p.Velocity.x) * 0.5f;
            }
            if (p.Position.x > state.Width - padding)
            {
                p.Position.x = state.Width - padding;
                p.Velocity.x = -MathF.Abs(p.Velocity.x) * 0.5f;
            }
            if (p.Position.y < padding)
            {
                p.Position.y = padding;
                p.Velocity.y = MathF.Abs(p.Velocity.y) * 0.5f;
            }
            if (p.Position.y > state.Height - padding)
            {
                p.Position.y = state.Height - padding;
                p.Velocity.y = -MathF.Abs(p.Velocity.y) * 0.5f;
            }

            state.Particles[i] = p;
        }
    }

    private Vector3 InterpolateVelocity(float gx, float gy, int N, float cellWidth, float cellHeight)
    {
        // Clamp to valid grid range
        gx = MathF.Max(0.5f, MathF.Min(N + 0.5f, gx));
        gy = MathF.Max(0.5f, MathF.Min(N + 0.5f, gy));

        int i0 = (int)gx;
        int j0 = (int)gy;
        int i1 = i0 + 1;
        int j1 = j0 + 1;

        float s = gx - i0;
        float t = gy - j0;

        // Clamp indices
        i0 = Math.Clamp(i0, 0, N + 1);
        i1 = Math.Clamp(i1, 0, N + 1);
        j0 = Math.Clamp(j0, 0, N + 1);
        j1 = Math.Clamp(j1, 0, N + 1);

        // Bilinear interpolation
        float vxInterp = (1 - s) * (1 - t) * vx![IX(i0, j0)] +
                         s * (1 - t) * vx[IX(i1, j0)] +
                         (1 - s) * t * vx[IX(i0, j1)] +
                         s * t * vx[IX(i1, j1)];

        float vyInterp = (1 - s) * (1 - t) * vy![IX(i0, j0)] +
                         s * (1 - t) * vy[IX(i1, j0)] +
                         (1 - s) * t * vy[IX(i0, j1)] +
                         s * t * vy[IX(i1, j1)];

        return new Vector3(vxInterp * cellWidth, vyInterp * cellHeight, 0);
    }
}
