namespace Statecraft.Simulation
{
    /// <summary>
    /// Immutable authoring data required to seed the simulation.
    /// Presentation-only definition data deliberately stays outside this contract.
    /// </summary>
    public interface ISimulationCountryDefinition
    {
        string Id { get; }
        string DisplayName { get; }
        long Population { get; }
        double GdpUsd { get; }
        CountrySimulationSetup SimulationSetup { get; }
    }
}
