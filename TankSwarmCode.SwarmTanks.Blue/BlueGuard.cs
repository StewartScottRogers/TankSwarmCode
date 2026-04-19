using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

/// <summary>
/// Defensive anchor (Slot 3). Reduced firepower and the highest non-ECM retreat threshold (30)
/// so it pulls back early, preserving energy as a fallback rallying point.
/// </summary>
public sealed class BlueGuard : SwarmBrainBase
{
    public BlueGuard() { SwarmId = 2; Role = TankRole.Defender; }
    public override string Name => "BlueGuard";
    protected override TankConfig Config { get; } = new()
    {
        FormationSlot = 3,
        MaxFirePower = 2.0,
        PreferredRange = 200.0,
        HasEcm = false,
        RetreatEnergyThreshold = 30.0
    };
}
