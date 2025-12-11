namespace FluidSim.Core;

public class SolverMetadata
{
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public Dictionary<string, DisplayParameter> Parameters { get; set; }
    public SimDimensionality Dimensionality { get; set; }
    public SimDomainNature DomainNature { get; set; }

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