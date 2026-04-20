using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.SwarmTank.Events;

/// <summary>
/// Fired when an enemy radar beam sweeps over this tank, revealing the painter's position.
/// </summary>
public sealed class PaintedEventArgs : EventArgs
{
    public PaintedEventArgs(string painterName, int painterSwarmId, Vector2D painterPosition)
    {
        PainterName = painterName;
        PainterSwarmId = painterSwarmId;
        PainterPosition = painterPosition;
    }

    /// <summary>Name of the tank whose radar painted us.</summary>
    public string PainterName { get; }

    /// <summary>Swarm the painter belongs to.</summary>
    public int PainterSwarmId { get; }

    /// <summary>Arena position of the painter at the moment of the scan.</summary>
    public Vector2D PainterPosition { get; }
}
