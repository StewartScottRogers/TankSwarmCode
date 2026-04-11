namespace TankSwarmCode.SwarmTank.Interfaces.Events;

/// <summary>Fired for every surviving tank when a round finishes.</summary>
public sealed class RoundEndedEventArgs : EventArgs
{
    public RoundEndedEventArgs(bool won, long totalTicks)
    {
        Won = won;
        TotalTicks = totalTicks;
    }

    /// <summary>True if this tank's swarm was the last one standing.</summary>
    public bool Won { get; }

    /// <summary>Total number of ticks the round lasted.</summary>
    public long TotalTicks { get; }
}
