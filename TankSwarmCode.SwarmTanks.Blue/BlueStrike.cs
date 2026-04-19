using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

public sealed class BlueStrike : SwarmBrainBaseBlue
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
