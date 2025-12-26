using FluidSim.Core.Units;

namespace FluidSim.Core;

public class DisplayParameter
{
    public float Value
    {
        get;
        set
        {
            switch (Domain)
            {
                case ParameterDomain.Integer:
                    field = (int)value;
                    break;
                case ParameterDomain.Binary:
                    field = value > 0.5f ? Range.Min : Range.Max;
                    break;
                default:
                    field = value;
                    break;
            }
        }
    }
    public Interval Range { get; set; }
    public ParameterDomain Domain { get; set; }
    public Unit Unit { get; set; }
    public bool ReinitializeOnChange { get; set; } = false;

    public DisplayParameter(float value, Interval range, ParameterDomain domain, Unit? unit = null, bool reinitializeOnChange = true)
    {
        Domain = domain;
        Value = value;
        Range = range;
        Unit = unit ?? Units.Units.None;
        ReinitializeOnChange = reinitializeOnChange;
    }
}

public enum ParameterDomain
{
    Integer,
    Decimal,
    Binary
}