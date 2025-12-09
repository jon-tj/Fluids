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
                case ParameterDomain.Decimal:
                    field = value;
                    break;
                case ParameterDomain.Integer:
                    field = (int)value;
                    break;
                case ParameterDomain.Binary:
                    field = value > 0.5f ? Range.Min : Range.Max;
                    break;
            }
        }
    }
    public Interval Range;
    public ParameterDomain Domain;
    public Unit Unit;
    public bool ReinitializeOnChange = false;

    public DisplayParameter(float value, Interval range, ParameterDomain domain, Unit? unit = null, bool reinitializeOnChange = true)
    {
        Value = value;
        Range = range;
        Domain = domain;
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