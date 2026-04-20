using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTank.Interfaces.Events;

/// <summary>Fired when the tank's radar sweep reflects off a building wall.</summary>
public sealed class ScannedBuildingEventArgs : EventArgs
{
    public ScannedBuildingEventArgs(BuildingEcho echo) => Echo = echo;

    /// <summary>Wall echo data — only the faces that faced the scanner.</summary>
    public BuildingEcho Echo { get; }
}
