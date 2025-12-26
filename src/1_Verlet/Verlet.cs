using FluidSim.Core;
using FluidSim.Core.Units;

namespace FluidSim.Solvers.Verlet;

public class StableFluidsSolver : IFluidSolver
{
    public string Id => "Verlet";

    public SolverMetadata Metadata =>
        new(
            displayName: "Verlet",
            description: "Solve a particle simulation with high density using chunking.",
            parameters: new()
            {
                ["Substeps"] = new DisplayParameter(4, new Interval(1, 10), ParameterDomain.Integer),
                ["Time Step"] = new DisplayParameter(0.05f, Intervals.Unit, ParameterDomain.Decimal, Units.Second),
                ["Radius"] = new DisplayParameter(0.25f, Intervals.Unit, ParameterDomain.Decimal, Units.Meter),
                ["Chunk Size"] = new DisplayParameter(0.25f, new Interval(0, 1), ParameterDomain.Decimal, Units.Meter),
                // ["Initial Skew"] = new DisplayParameter(0.05f, new Interval(0, 1), ParameterDomain.Decimal),
            }
        );
    public FluidState Step(FluidState state)
    {
        int substeps = (int)Metadata.Parameters["Substeps"].Value;
        float dt = Metadata.Parameters["Time Step"].Value;
        dt /= substeps;
        float radius = Metadata.Parameters["Radius"].Value;
        float chunkSize = Metadata.Parameters["Chunk Size"].Value;
        Vector3 acceleration = Vector3.Gravity;

        void CorrectWallCollisions()
        {
            for (int j = 0; j < state.Particles.Length; j++)
            {
                Particle p = state.Particles[j];
                if (p.Position.y < 0)
                {
                    p.Position.y = 0f;
                    p.Velocity.y = Math.Max(0, p.Velocity.y);
                    state.Particles[j] = p;
                }
                if (p.Position.x < 0)
                {
                    p.Position.x = 0f;
                    if (p.Velocity.x < 0)
                        p.Velocity.x = -p.Velocity.x * 0.5f;
                    p.PreviousPosition.x = p.Position.x * 2 - p.PreviousPosition.x;
                    state.Particles[j] = p;
                }
                if (p.Position.x > state.Width)
                {
                    p.Position.x = state.Width;
                    if (p.Velocity.x > 0)
                        p.Velocity.x = -p.Velocity.x * 0.5f;
                    p.PreviousPosition.x = p.Position.x * 2 - p.PreviousPosition.x;
                    state.Particles[j] = p;
                }
            }
        }

        for (int step = 0; step < substeps; step++)
        {
            // --- Verlet integration ---
            for (int j = 0; j < state.Particles.Length; j++)
            {
                Particle p = state.Particles[j];
                Vector3 currentPos = p.Position;
                p.Position = currentPos + (currentPos - p.PreviousPosition) + acceleration * dt * dt;
                p.Velocity = (p.Position - p.PreviousPosition) / dt;
                p.PreviousPosition = currentPos;
                state.Particles[j] = p;
            }

            // --- build chunks for collision detection ---
            var chunks = BuildChunks(state.Particles, chunkSize);

            CorrectWallCollisions();

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
                                        pi.Position += correction * 0.4f;
                                        pj.Position -= correction * 0.4f;
                                        pi.PreviousPosition += correction * 0.35f;
                                        pj.PreviousPosition -= correction * 0.35f;

                                        state.Particles[i] = pi;
                                        state.Particles[j] = pj;
                                    }
                                }
                        }
            }

            CorrectWallCollisions();
        }

        return state;
    }


    private Dictionary<(int, int, int), List<int>> BuildChunks(Particle[] particles, float chunkSize)
    {
        var chunks = new Dictionary<(int, int, int), List<int>>();

        for (int i = 0; i < particles.Length; i++)
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
