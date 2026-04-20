using TankSwarmCode.AiCortex;
using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

public sealed class BlueTrooper : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex;
    private readonly int _slot;

    public BlueTrooper(int slot)
    {
        SwarmId = 2;
        Role = TankRole.Attacker;
        _slot = slot;
        _cortex = CortexFactory.For("BlueTrooper", slot);
    }

    public override string Name => $"Blue{_slot}";

    public override void OnStart()                               => _cortex.OnStart(this);
    public override void OnTick(TickEventArgs e)                 => _cortex.OnTick(this);
    public override void OnSwarmMessage(SwarmMessageEventArgs e) => _cortex.OnSwarmMessage(this, e);
    public override void OnRoundEnded(RoundEndedEventArgs e)     => _cortex.OnRoundEnded(this, e);

    string ITankContext.Name => Name;
    int ITankContext.SwarmId => SwarmId;
    TankRole ITankContext.Role { get => Role; set => Role = value; }
    TankState ITankContext.State => State;
    IArenaContext ITankContext.Arena => Arena;
    IReadOnlyDictionary<string, RadarContact> ITankContext.RadarMap => RadarMap;
    IReadOnlyDictionary<string, BuildingEcho> ITankContext.BuildingWallMap => BuildingWallMap;
    void ITankContext.SetAhead(double d)          => SetAhead(d);
    void ITankContext.SetBack(double d)           => SetBack(d);
    void ITankContext.SetTurnRight(double d)      => SetTurnRight(d);
    void ITankContext.SetTurnLeft(double d)       => SetTurnLeft(d);
    void ITankContext.SetTurnGunRight(double d)   => SetTurnGunRight(d);
    void ITankContext.SetTurnGunLeft(double d)    => SetTurnGunLeft(d);
    void ITankContext.SetTurnRadarRight(double d) => SetTurnRadarRight(d);
    void ITankContext.SetTurnRadarLeft(double d)  => SetTurnRadarLeft(d);
    void ITankContext.SetFire(double p)           => SetFire(p);
    void ITankContext.SetEcm(EcmMode m)           => SetEcm(m);
    void ITankContext.Broadcast(SwarmMessage msg) => Broadcast(msg);
}
