using FluidSim.Core;
using FluidSim.Core.Units;

namespace FluidSim.Solvers.Verlet;

public class VerletSolver : IFluidSolver
{
    public string Id => "Verlet";
    public SolverMetadata Metadata => md;
    public SolverMetadata md =
        new(
            id: "Verlet",
            displayName: "Verlet",
            description: """
            Solve a particle simulation with high density using chunking.
            Note that Chunk Size should be at least twice the particle Radius to avoid jitter.
            """,
            parameters: new()
            {
                ["Substeps"] = new DisplayParameter(5, new Interval(1, 20), ParameterDomain.Integer),
                ["Particles"] = new DisplayParameter(300, new Interval(1, 2000), ParameterDomain.Integer),
                ["Time Step"] = new DisplayParameter(0.033f, Intervals.Unit, ParameterDomain.Decimal, Units.Second),
                ["Radius"] = new DisplayParameter(0.2f, Intervals.Unit, ParameterDomain.Decimal, Units.Meter),
                ["Chunk Size"] = new DisplayParameter(0.4f, new Interval(0, 1), ParameterDomain.Decimal, Units.Meter),
                ["Gravity"] = new DisplayParameter(9.81f, new Interval(0, 30), ParameterDomain.Decimal, Units.MetersPerSecond2),
                ["Tunnel Speed"] = new DisplayParameter(0.0f, new Interval(-10f, 10f), ParameterDomain.Decimal, Units.MetersPerSecond),
                // ["Initial Skew"] = new DisplayParameter(0.05f, new Interval(0, 1), ParameterDomain.Decimal),
            }
        );

    private bool isLoopingX
    {
        get => md.Parameters["Tunnel Speed"].Value != 0.0f;
    }

    public FluidState Step(FluidState state)
    {
        if (md.Parameters["Particles"].ValueChanged() is ValueChangedEventArgs particleChangedEvent)
        {
            if (particleChangedEvent.NewValue > particleChangedEvent.OldValue)
            {
                // add particles
                int toAdd = (int)(particleChangedEvent.NewValue - particleChangedEvent.OldValue);
                Random rand = new Random();
                for (int i = 0; i < toAdd; i++)
                {
                    Particle p = state.Particles[0].Clone();
                    p.Position = new Vector3(
                        (float)(rand.NextDouble() * state.Width),
                        (float)(rand.NextDouble() * state.Height),
                        (float)(rand.NextDouble() * state.Depth)
                    );
                    p.PreviousPosition = p.Position;
                    state.Particles.Add(p);
                }
            }
            else
            {
                // remove particles
                int toRemove = (int)(particleChangedEvent.OldValue - particleChangedEvent.NewValue);
                state.Particles.RemoveRange(state.Particles.Count - toRemove, toRemove);
            }
        }
        int substeps = (int)md.Parameters["Substeps"].Value;
        float dt = md.Parameters["Time Step"].Value;
        dt /= substeps;
        float radius = md.Parameters["Radius"].Value;
        float chunkSize = md.Parameters["Chunk Size"].Value;
        Vector3 acceleration = Vector3.Down * md.Parameters["Gravity"].Value;

        for (int step = 0; step < substeps; step++)
        {
            // --- Verlet integration ---
            for (int j = 0; j < state.Particles.Count; j++)
            {
                Particle p = state.Particles[j];
                Vector3 currentPos = p.Position;
                float estimatedSpeed = (p.Position.x - p.PreviousPosition.x) / dt;
                p.Position = currentPos + (currentPos - p.PreviousPosition) + acceleration * dt * dt;
                p.PreviousPosition = currentPos;
                if (false)
                {

                    p.Position.x += md.Parameters["Tunnel Speed"].Value * dt;
                    // p.PreviousPosition.x += md.Parameters["Tunnel Speed"].Value * dt;

                }
                state.Particles[j] = p;
            }

            // --- build chunks for collision detection ---
            var chunks = BuildChunks(state.Particles, chunkSize);

            // --- collision handling ---
            foreach (var kvp in chunks)
            {
                var key = kvp.Key;
                var particleIndices = kvp.Value;

                // check neighboring chunks (including self)
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                        for (int dz = -1; dz <= 1; dz++)
                        {
                            var neighborKey = (key.Item1 + dx, key.Item2 + dy, key.Item3 + dz);
                            if (!chunks.ContainsKey(neighborKey)) continue;

                            var neighborIndices = chunks[neighborKey];

                            // particle-particle collisions
                            foreach (int i in particleIndices)
                                foreach (int j in neighborIndices)
                                {
                                    if (i >= j) continue; // avoid double-counting

                                    Particle pi = state.Particles[i];
                                    Particle pj = state.Particles[j];

                                    Vector3 delta = pi.Position - pj.Position;
                                    float dist = delta.Magnitude();

                                    if (dist < 2 * radius && dist > 0f)
                                    {
                                        // simple separation
                                        Vector3 correction = delta * (0.5f * (2 * radius - dist) / dist);
                                        pi.Position += correction * 0.7f; // Dampen the correction to avoid jitter
                                        pj.Position -= correction * 0.7f;

                                        state.Particles[i] = pi;
                                        state.Particles[j] = pj;
                                    }
                                }
                        }
            }

            // --- boundary conditions ---
            for (int j = 0; j < state.Particles.Count; j++)
            {
                Particle p = state.Particles[j];
                if (p.Position.y < radius)
                {
                    p.Position.y = radius;
                }
                if (p.Position.y > state.Height)
                {
                    p.Position.y = state.Height;
                }
                if (isLoopingX)
                {
                    if ((p.Position.x < 0) && (md.Parameters["Tunnel Speed"].Value < 0))
                    {
                        p.Position.x += state.Width + radius * 2;
                        p.PreviousPosition.x += state.Width + radius * 2;
                        if (p.PreviousPosition.x < p.Position.x)
                            p.PreviousPosition.x = p.Position.x;
                    }
                    if ((p.Position.x > state.Width) && (md.Parameters["Tunnel Speed"].Value > 0))
                    {
                        p.Position.x -= state.Width + radius * 2;
                        p.PreviousPosition.x -= state.Width + radius * 2;
                        if (p.PreviousPosition.x > p.Position.x)
                            p.PreviousPosition.x = p.Position.x;
                    }
                }
                else
                {
                    if (p.Position.x < radius)
                    {
                        p.Position.x = radius;
                    }
                    if (p.Position.x > state.Width - radius)
                    {
                        p.Position.x = state.Width - radius;
                    }
                }
            }
        }

        return state;
    }


    private Dictionary<(int, int, int), List<int>> BuildChunks(List<Particle> particles, float chunkSize)
    {
        var chunks = new Dictionary<(int, int, int), List<int>>();

        for (int i = 0; i < particles.Count; i++)
        {
            Particle p = particles[i];
            var key = (
                (int)Math.Floor(p.Position.x / chunkSize),
                (int)Math.Floor(p.Position.y / chunkSize),
                (int)Math.Floor(p.Position.z / chunkSize)
            );

            if (!chunks.ContainsKey(key))
                chunks[key] = new List<int>();

            chunks[key].Add(i);
        }

        return chunks;
    }
}
