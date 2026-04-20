using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.SwarmTank.Events;

/// <summary>Fired when this tank is struck by a bullet.</summary>
public sealed class HitByBulletEventArgs : EventArgs
{
    public HitByBulletEventArgs(BulletState bullet, double bearing)
    {
        Bullet = bullet;
        Bearing = bearing;
    }

    /// <summary>The bullet that hit this tank.</summary>
    public BulletState Bullet { get; }

    /// <summary>
    /// Bearing of the bullet relative to this tank's heading.
    /// Negative = came from the left, positive = came from the right.
    /// </summary>
    public double Bearing { get; }
}
