using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTank.Interfaces.Events;

/// <summary>Fired when this tank receives a broadcast from a swarm ally.</summary>
public sealed class SwarmMessageEventArgs : EventArgs
{
    public SwarmMessageEventArgs(SwarmMessage message) => Message = message;

    /// <summary>The message sent by the ally.</summary>
    public SwarmMessage Message { get; }
}
