using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

public sealed class BlueGuard : SwarmBrainBaseBlue
{
    public BlueGuard() { SwarmId = 2; Role = TankRole.Defender; }
    public override string Name => "BlueGuard";
    protected internal override TankConfig TankConfig { get; } = new()
    {
        FormationSlot = 3,
        MaxFirePower = 2.0,
        PreferredRange = 200.0,
        HasEcm = false,
        RetreatEnergyThreshold = 30.0
    };
}
