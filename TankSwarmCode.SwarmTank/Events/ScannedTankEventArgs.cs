using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.SwarmTank.Events;

/// <summary>Fired when the tank's radar sweeps over an enemy tank.</summary>
public sealed class ScannedTankEventArgs : EventArgs
{
    public ScannedTankEventArgs(ScanResult result) => Result = result;

    /// <summary>Data about the scanned enemy tank.</summary>
    public ScanResult Result { get; }
}
