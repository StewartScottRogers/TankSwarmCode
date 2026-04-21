using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Red;

public sealed class RedHammer : SwarmBrainBase
{
    public RedHammer() { SwarmId = 1; Role = TankRole.EcmSpecialist; }
    public override string Name => "RedHammer";
    protected override TankConfig Config { get; } = new()
    {
        FormationSlot = 0,
        MaxFirePower = 0.1,
        PreferredRange = 150.0,
        HasEcm = true,
        OffensiveEcmMode = EcmMode.JamAndSpoof,
        RetreatEnergyThreshold = 40.0
    };
}
