namespace TankSwarmCode.Arena;

/// <summary>
/// Carries a single formatted radio-traffic line and the SwarmId of the transmitting unit.
/// Raised by <see cref="ArenaUserControl.RadioTransmission"/> whenever a non-radar swarm
/// message is broadcast.
/// </summary>
public sealed class RadioTransmissionEventArgs : EventArgs
{
    /// <summary>
    /// Ready-to-display radio line in strict [MSGTYPE]: content format, e.g.
    /// <c>[TARGET]: RedScout contacts BlueWarden at 240 380</c>.
    /// </summary>
    public string FormattedLine { get; }

    /// <summary>SwarmId of the tank that broadcast this message (for colour-coding).</summary>
    public int SwarmId { get; }

    public RadioTransmissionEventArgs(string formattedLine, int swarmId)
    {
        FormattedLine = formattedLine;
        SwarmId = swarmId;
    }
}
