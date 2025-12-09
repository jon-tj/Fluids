namespace FluidSim.Core;

public record Interval(
    float Min = float.NegativeInfinity,
    float Max = float.PositiveInfinity
);

public static class Intervals
{
    public static Interval Unit = new(Min: 0, Max: 1);
    public static Interval Positive = new(Min: float.Epsilon);
    public static Interval Negative = new(Max: -float.Epsilon);
}