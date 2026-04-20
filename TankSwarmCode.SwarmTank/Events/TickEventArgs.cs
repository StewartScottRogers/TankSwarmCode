namespace TankSwarmCode.SwarmTank.Events;

/// <summary>Fired once per simulation tick for every living tank.</summary>
public sealed class TickEventArgs : EventArgs
{
    public TickEventArgs(long tickNumber, int livingTankCount)
    {
        TickNumber = tickNumber;
        LivingTankCount = livingTankCount;
    }

    /// <summary>Current tick number (starts at 1).</summary>
    public long TickNumber { get; }

    /// <summary>Number of tanks still alive across all swarms.</summary>
    public int LivingTankCount { get; }
}
