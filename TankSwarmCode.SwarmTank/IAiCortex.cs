using TankSwarmCode.SwarmTank.Events;

namespace TankSwarmCode.SwarmTank;

public interface IAiCortex
{
    void OnStart(ITankContext ctx);
    void OnTick(ITankContext ctx);
    void OnSwarmMessage(ITankContext ctx, SwarmMessageEventArgs e);
    void OnRoundEnded(ITankContext ctx, RoundEndedEventArgs e);
}
