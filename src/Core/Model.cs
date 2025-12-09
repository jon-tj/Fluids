namespace FluidSim.Core;

public record FluidState(float[] Data, int Width, int Height, int Depth = 1);

public class SimulationEnvironment
{
    public DateTime SimulationBeginTime = DateTime.Now;
    public float TimeSinceSimulationBegin
    {
        get => (float)(DateTime.Now - SimulationBeginTime).TotalSeconds;
    }
}