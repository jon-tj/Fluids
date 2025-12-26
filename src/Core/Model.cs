namespace FluidSim.Core;

public class FluidState(Particle[] Particles, float Width, float Height, float Depth = 1)
{
    public Particle[] Particles { get; } = Particles;
    public float Width { get; } = Width;
    public float Height { get; } = Height;
    public float Depth { get; } = Depth;

    public static FluidState Deserialize(byte[] data)
    {
        using var br = new BinaryReader(new MemoryStream(data));
        float width = br.ReadSingle();
        float height = br.ReadSingle();
        float depth = br.ReadSingle();
        int numParticles = br.ReadInt32();
        Particle[] particles = new Particle[numParticles];
        for (int i = 0; i < numParticles; i++)
        {
            float mass = br.ReadSingle();
            float px = br.ReadSingle();
            float py = br.ReadSingle();
            float pz = br.ReadSingle();
            float px1 = br.ReadSingle();
            float py1 = br.ReadSingle();
            float pz1 = br.ReadSingle();
            float vx = br.ReadSingle();
            float vy = br.ReadSingle();
            float vz = br.ReadSingle();
            particles[i] = new Particle(mass, new Vector3(px, py, pz), new Vector3(px1, py1, pz1), new Vector3(vx, vy, vz));
        }
        return new FluidState(particles, width, height, depth);
    }

    public byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(Width);
        bw.Write(Height);
        bw.Write(Depth);
        bw.Write(Particles.Length);
        foreach (var p in Particles)
        {
            bw.Write(p.Mass);
            bw.Write(p.Position.x);
            bw.Write(p.Position.y);
            bw.Write(p.Position.z);
            bw.Write(p.PreviousPosition.x);
            bw.Write(p.PreviousPosition.y);
            bw.Write(p.PreviousPosition.z);
            bw.Write(p.Velocity.x);
            bw.Write(p.Velocity.y);
            bw.Write(p.Velocity.z);
        }
        return ms.ToArray();
    }

    public static FluidState UniformRandom(int numParticles, float mass, int width, int height, int depth)
    {
        Particle[] particles = new Particle[numParticles];
        Random rand = new Random();
        for (int i = 0; i < numParticles; i++)
        {
            Vector3 pos0 = new Vector3(
                (float)(rand.NextDouble() * width),
                (float)(rand.NextDouble() * height),
                (float)(rand.NextDouble() * depth)
            );
            particles[i] = new Particle(mass, pos0, pos0, Vector3.Zero);
        }
        return new FluidState(particles, width, height, depth);
    }
}

public class Particle(float Mass, Vector3 Position, Vector3 PreviousPosition, Vector3 Velocity)
{
    public float Mass { get; set; } = Mass;
    public Vector3 Position { get; set; } = Position;
    public Vector3 PreviousPosition { get; set; } = PreviousPosition;
    public Vector3 Velocity { get; set; } = Velocity;
}

public class SimulationEnvironment
{
    public DateTime SimulationBeginTime = DateTime.Now;
    public float TimeSinceSimulationBegin
    {
        get => (float)(DateTime.Now - SimulationBeginTime).TotalSeconds;
    }
}

public class Vector3
{
    public float x, y, z;

    public Vector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 Dot(Vector3 other)
    {
        return new Vector3(x * other.x, y * other.y, z * other.z);
    }
    public float Magnitude()
    {
        return (float)Math.Sqrt(x * x + y * y + z * z);
    }

    public Vector3 Normalized()
    {
        float mag = Magnitude();
        return new Vector3(x / mag, y / mag, z / mag);
    }

    public static Vector3 operator +(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static Vector3 operator *(Vector3 a, float b)
    {
        return new Vector3(a.x * b, a.y * b, a.z * b);
    }
    public static Vector3 operator *(Vector3 a, Vector3 b)
    {
        return a.Dot(b);
    }

    public static Vector3 operator /(Vector3 a, float b)
    {
        return new Vector3(a.x / b, a.y / b, a.z / b);
    }

    public static Vector3 Zero => new Vector3(0, 0, 0);
    public static Vector3 Gravity => new Vector3(0, -9.81f, 0);
    public static Vector3 Up => new Vector3(0, 1, 0);
    public static Vector3 Right => new Vector3(1, 0, 0);
    public static Vector3 Forward => new Vector3(0, 0, 1);
    public static Vector3 Down => new Vector3(0, -1, 0);
    public static Vector3 Left => new Vector3(-1, 0, 0);
    public static Vector3 Back => new Vector3(0, 0, -1);
}