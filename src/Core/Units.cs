namespace FluidSim.Core.Units;

public enum UnitType
{
    Distance,
    Time,
    Speed,
    Acceleration
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
    public static Unit MetersPerSecond = new("m/s", "Meters per Second", UnitType.Speed);
    public static Unit MetersPerSecond2 = new("m/s²", "Meters per Second Squared", UnitType.Acceleration);
    public static Unit None = new("", "None", null);
}