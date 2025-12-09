namespace FluidSim.Core.Units;

public enum UnitType
{
    Distance,
    Time,
}

public record Unit(
    string Symbol,
    string Name,
    UnitType? Type
);

public static class Units
{
    public static Unit Second = new("s", "Second", UnitType.Time);
    public static Unit Meter = new("m", "Meter", UnitType.Distance);
    public static Unit None = new("", "None", null);
}