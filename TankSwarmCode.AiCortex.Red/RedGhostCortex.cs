using TankSwarmCode.AiCortex.Red.Library;
using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Enums;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Red;

public sealed class RedGhostCortex : RedCortexBase
{
    protected override TankConfiguration TankConfiguration { get; } = new()
    {
        FormationSlot = 3,
        MaxFirePower = 0.1,
        PreferredRange = 150.0,
        HasEcm = true,
        OffensiveEcmMode = EcmMode.JamAndSpoof,
        RetreatEnergyThreshold = 40.0
    };

    public override void OnTick(ITankContext ctx)
    {
        base.OnTick(ctx);
        // Proactively jam whenever enemies are visible — don't wait for ECMScreen strategy
        bool hasEnemy = ctx.RadarMap.Values.Any(c => !c.IsAlly && ctx.Arena.TickNumber - c.Timestamp < 30);
        if (hasEnemy)
            ctx.SetEcm(EcmMode.JamAndSpoof);
    }
}
