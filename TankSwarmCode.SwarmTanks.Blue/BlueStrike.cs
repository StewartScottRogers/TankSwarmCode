using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

/// <summary>
/// Formation leader (Slot 0). High firepower at medium range; acts as swarm commander
/// because the lowest FormationSlot wins leader election in SwarmBrainBase.
/// </summary>
public sealed class BlueStrike : SwarmBrainBase
{
    public BlueStrike() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "BlueStrike";
    protected override TankConfig Config { get; } = new()
    {
        FormationSlot = 0,
        MaxFirePower = 3.0,
        PreferredRange = 250.0,
        HasEcm = false,
        RetreatEnergyThreshold = 25.0
    };
}
