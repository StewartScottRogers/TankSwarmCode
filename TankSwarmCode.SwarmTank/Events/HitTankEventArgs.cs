using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.SwarmTank.Events;

/// <summary>Fired when this tank physically collides with another tank.</summary>
public sealed class HitTankEventArgs : EventArgs
{
    public HitTankEventArgs(TankState other, double bearing)
    {
        Other = other;
        Bearing = bearing;
    }

    /// <summary>State of the tank this tank collided with.</summary>
    public TankState Other { get; }

    /// <summary>Bearing of the other tank relative to this tank's heading.</summary>
    public double Bearing { get; }
}
