using TankSwarmCode.AiCortex.Blue.Library;
using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Enums;
using TankSwarmCode.SwarmTank.Events;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

public sealed class Blue12 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Blue12");
    public Blue12() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "Blue12";
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

public sealed class Blue13 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Blue13");
    public Blue13() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "Blue13";
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

public sealed class Blue14 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Blue14");
    public Blue14() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "Blue14";
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

public sealed class Blue15 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Blue15");
    public Blue15() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "Blue15";
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

public sealed class Blue16 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Blue16");
    public Blue16() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "Blue16";
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

public sealed class Blue17 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Blue17");
    public Blue17() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "Blue17";
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

public sealed class Blue18 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Blue18");
    public Blue18() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "Blue18";
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

public sealed class Blue19 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Blue19");
    public Blue19() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "Blue19";
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

public sealed class Blue20 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Blue20");
    public Blue20() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "Blue20";
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

public sealed class Blue21 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Blue21");
    public Blue21() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "Blue21";
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

public sealed class Blue22 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Blue22");
    public Blue22() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "Blue22";
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

public sealed class Blue23 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Blue23");
    public Blue23() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "Blue23";
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

public sealed class Blue24 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Blue24");
    public Blue24() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "Blue24";
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

public sealed class Blue25 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Blue25");
    public Blue25() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "Blue25";
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

public sealed class Blue26 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Blue26");
    public Blue26() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "Blue26";
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

public sealed class Blue27 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Blue27");
    public Blue27() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "Blue27";
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
