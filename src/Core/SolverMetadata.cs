namespace FluidSim.Core;

public class SolverMetadata
{
    public string DisplayName;
    public string Description;
    public Dictionary<string, DisplayParameter> Parameters;
    public SimDimensionality Dimensionality;
    public SimDomainNature DomainNature;

    public SolverMetadata(
        string displayName,
        string description,
        Dictionary<string, DisplayParameter> parameters,
        SimDimensionality dimensionality = SimDimensionality.TwoD,
        SimDomainNature domainNature = SimDomainNature.Continuous)
    {
        DisplayName = displayName;
        Description = description;
        Parameters = parameters;
        Dimensionality = dimensionality;
        DomainNature = domainNature;
    }

    public float this[string paramName]
    {
        get => Parameters[paramName].Value;
    }
}

public enum SimDimensionality
{
    OneD, TwoD, ThreeD
}

public enum SimDomainNature
{
    Continuous, Discrete
}