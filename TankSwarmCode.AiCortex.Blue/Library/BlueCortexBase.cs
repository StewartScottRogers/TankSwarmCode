using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Enums;
using TankSwarmCode.SwarmTank.Events;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Blue.Library;

public abstract class BlueCortexBase : IAiCortex
{
    protected abstract TankConfiguration TankConfiguration { get; }

    private SwarmCoordinator _swarm = null!;
    protected double RadarSpin { get; set; } = 45.0;
    private long _scheduledFireTick = -1;

    public virtual void OnStart(ITankContext ctx)
    {
        _swarm = new SwarmCoordinator();
        _scheduledFireTick = -1;
    }

    public virtual void OnTick(ITankContext ctx)
    {
        _swarm.UpdateSelf(ctx, TankConfiguration);
        _swarm.BroadcastAllyPing(ctx, TankConfiguration);
        _swarm.PruneStaleAllies(ctx);

        string leader = _swarm.DetermineLeader();
        if (leader == ctx.Name
            && (ctx.Arena.TickNumber % SwarmCoordinator.LeadershipEpochTicks == 0 || _swarm.StrategyEpoch < 0))
        {
            long? leaderFireTick = _swarm.RunEpochLogic(ctx, TankConfiguration);
            if (leaderFireTick.HasValue) _scheduledFireTick = leaderFireTick.Value;
        }

        _swarm.HandleEcm(ctx, TankConfiguration);

        if (_scheduledFireTick > 0 && ctx.Arena.TickNumber >= _scheduledFireTick)
        {
            RadarContact? volleyTarget = _swarm.GetStrategyTarget(ctx);
            if (volleyTarget != null)
            {
                double power = Math.Min(TankConfiguration.MaxFirePower, ctx.State.Energy * 0.1);
                if (power >= 0.1 && ctx.State.Energy >= 5 && !TankNavigation.IsWallInLineOfFire(ctx, volleyTarget.Position))
                    ctx.SetFire(power);
            }
            _scheduledFireTick = -1;
        }

        _swarm.ExecuteStrategy(ctx, TankConfiguration, RadarSpin);
    }

    public virtual void OnSwarmMessage(ITankContext ctx, SwarmMessageEventArgs e)
    {
        switch (e.Message.Type)
        {
            case SwarmMessageType.AllyPing:
                _swarm.HandleAllyPing(ctx, e);
                break;
            case SwarmMessageType.StrategyCommand:
                _swarm.HandleStrategyCommand(e);
                break;
            case SwarmMessageType.VolleyFire:
                long? tick = _swarm.HandleVolleyFire(ctx, TankConfiguration, e);
                if (tick.HasValue) _scheduledFireTick = tick.Value;
                break;
            case SwarmMessageType.EcmAlert:
                _swarm.HandleEcmAlert(ctx);
                break;
        }
    }

    public virtual void OnRoundEnded(ITankContext ctx, RoundEndedEventArgs e) { }
}
