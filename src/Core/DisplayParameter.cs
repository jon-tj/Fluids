using FluidSim.Core.Units;

namespace FluidSim.Core;

public class DisplayParameter
{
    private float OldValue;
    public float Value
    {
        get;
        set
        {
            OldValue = Value;
            switch (Domain)
            {
                case ParameterDomain.Integer:
                    field = (int)value;
                    if (Range is not null)
                    {
                        if (field < Range.Min) field = Range.Min;
                        if (field > Range.Max) field = Range.Max;
                    }
                    break;
                case ParameterDomain.Binary:
                    var threshold = (Range.Min + Range.Max) / 2;
                    field = value < threshold ? Range.Min : Range.Max;
                    break;
                default:
                    field = value;
                    if (Range is not null)
                    {
                        if (field < Range.Min) field = Range.Min;
                        if (field > Range.Max) field = Range.Max;
                    }
                    break;
            }
        }

    }

    public ValueChangedEventArgs? ValueChanged()
    {
        if (OldValue != Value)
        {
            var e = new ValueChangedEventArgs(OldValue, Value);
            OldValue = Value;
            return e;
        }
        return null;
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