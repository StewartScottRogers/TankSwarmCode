namespace TankSwarmCode.SwarmTank.Events;

/// <summary>Fired when this tank collides with an arena wall.</summary>
public sealed class HitWallEventArgs : EventArgs
{
    public HitWallEventArgs(double bearing) => Bearing = bearing;

    /// <summary>
    /// Bearing of the wall relative to this tank's heading.
    /// Use to determine which wall was hit.
    /// </summary>
    public double Bearing { get; }
}
