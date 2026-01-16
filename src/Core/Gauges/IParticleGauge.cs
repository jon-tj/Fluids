namespace FluidSim.Core.Gauges;

public class ParticleGaugeResult(string name, float value)
{
    public string Name { get; } = name;
    public float Value { get; set; } = value;
    public static ParticleGaugeResult[] FromVector2(Vector3 vector, string name)
        => [new(name + " X", vector.x), new(name + " Y", vector.y)];
}

public record ParticleGaugeMetadata(string Id, string Description);
public interface IParticleGauge
{
    public ParticleGaugeMetadata Metadata { get; }
    public ParticleGaugeResult[] Gauge(FluidState state);
}

public class VelocityGauge : IParticleGauge
{
    public ParticleGaugeMetadata Metadata => new("Velocity", "Calculate the average velocity.");
    public ParticleGaugeResult[] Gauge(FluidState state)
    {
        // Calculate the average velocity of a collection of particles
        Vector3 sum = Vector3.Zero;
        int count = 0;

        foreach (var p in state.Particles)
        {
            sum += p.Position - p.PreviousPosition;
            count++;
        }

        Vector3 avgVelocity = count > 0 ? sum / count : Vector3.Zero;
        return ParticleGaugeResult.FromVector2(avgVelocity, "Velocity");
    }
}