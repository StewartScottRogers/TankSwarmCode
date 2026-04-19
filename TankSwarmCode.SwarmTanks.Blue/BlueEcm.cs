using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

/// <summary>
/// ECM specialist (Slot 4). The only Blue tank with HasEcm=true; activates Jam in ECMScreen
/// strategy and Burnthrough when an enemy jammer is detected. Lowest firepower (1.5) and
/// highest retreat threshold (35) reflect the trade: survive to keep jamming, not to kill.
/// </summary>
public sealed class BlueEcm : SwarmBrainBase
{
    public BlueEcm() { SwarmId = 2; Role = TankRole.EcmSpecialist; }
    public override string Name => "BlueEcm";
    protected override TankConfig Config { get; } = new()
    {
        FormationSlot = 4,
        MaxFirePower = 1.5,
        PreferredRange = 150.0,
        HasEcm = true,
        OffensiveEcmMode = EcmMode.Jam,
        RetreatEnergyThreshold = 35.0
    };
}
