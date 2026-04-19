using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

/// <summary>
/// Long-range sniper (Slot 1, backup leader). Longest PreferredRange in the swarm (300 px)
/// and lowest RetreatThreshold — stays engaged even at low energy to maintain fire coverage.
/// </summary>
public sealed class BlueSharp : SwarmBrainBase
{
    public BlueSharp() { SwarmId = 2; Role = TankRole.Support; }
    public override string Name => "BlueSharp";
    protected override TankConfig Config { get; } = new()
    {
        FormationSlot = 1,
        MaxFirePower = 3.0,
        PreferredRange = 300.0,
        HasEcm = false,
        RetreatEnergyThreshold = 20.0
    };
}
