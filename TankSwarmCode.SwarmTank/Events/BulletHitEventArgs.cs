using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.SwarmTank.Events;

/// <summary>Fired when a bullet fired by this tank strikes an enemy.</summary>
public sealed class BulletHitEventArgs : EventArgs
{
    public BulletHitEventArgs(BulletState bullet, TankState victim)
    {
        Bullet = bullet;
        Victim = victim;
    }

    /// <summary>The bullet that hit the enemy.</summary>
    public BulletState Bullet { get; }

    /// <summary>State of the tank that was struck.</summary>
    public TankState Victim { get; }
}
