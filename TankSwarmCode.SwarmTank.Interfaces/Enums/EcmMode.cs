namespace TankSwarmCode.SwarmTank.Interfaces.Enums;

/// <summary>
/// Electronic Counter-Measures mode a tank can activate each tick.
/// All active modes drain energy per tick; <see cref="Off"/> is free.
/// </summary>
public enum EcmMode
{
    /// <summary>ECM system powered down. No energy cost.</summary>
    Off,

    /// <summary>
    /// Jamming mode. Floods the local EM spectrum with noise.
    /// Enemy radar sweeps that hit this tank have a high probability of being
    /// dropped (false negative) or returning corrupted position/heading data.
    /// Burnthrough-equipped scanners largely defeat this, but not entirely.
    /// Energy cost: <see cref="ArenaConstants.EcmJamCostPerTick"/> per tick.
    /// </summary>
    Jam,

    /// <summary>
    /// Deceptive spoofing mode. Projects false radar echo signatures at phantom
    /// positions near this tank. Enemies whose radar sweeps cross those ghost
    /// positions receive fake <c>OnScannedTank</c> events, causing them to waste
    /// fire and mis-track real contacts. Burnthrough scanners can partially filter
    /// ghosts but not entirely. Ghost names begin with <c>"Ghost-"</c>, which
    /// ECCM-aware AIs can detect to identify spoofing activity.
    /// Energy cost: <see cref="ArenaConstants.EcmSpoofCostPerTick"/> per tick.
    /// </summary>
    Spoof,

    /// <summary>
    /// Burnthrough / ECCM (Electronic Counter-Counter-Measures) mode.
    /// Focuses radar power to penetrate enemy jamming and identify spoofed contacts.
    /// Significantly reduces jam drop/corrupt chance and gives a strong chance of
    /// recognising and discarding ghost echoes.
    /// Energy cost: <see cref="ArenaConstants.EcmBurnthroughCostPerTick"/> per tick.
    /// </summary>
    Burnthrough
}
