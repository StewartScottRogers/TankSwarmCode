using TankSwarmCode.AiCortex.Red.Library;
using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Enums;
using TankSwarmCode.SwarmTank.Events;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.SwarmTanks.Red;

public sealed class Red5 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red5");
    public Red5() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red5";
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

public sealed class Red6 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red6");
    public Red6() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red6";
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

public sealed class Red7 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red7");
    public Red7() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red7";
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

public sealed class Red8 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red8");
    public Red8() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red8";
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

public sealed class Red9 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red9");
    public Red9() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red9";
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

public sealed class Red10 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red10");
    public Red10() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red10";
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

public sealed class Red11 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red11");
    public Red11() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red11";
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

public sealed class Red12 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red12");
    public Red12() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red12";
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

public sealed class Red13 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red13");
    public Red13() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red13";
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

public sealed class Red14 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red14");
    public Red14() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red14";
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

public sealed class Red15 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red15");
    public Red15() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red15";
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

public sealed class Red16 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red16");
    public Red16() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red16";
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

public sealed class Red17 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red17");
    public Red17() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red17";
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

public sealed class Red18 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red18");
    public Red18() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red18";
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

public sealed class Red19 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red19");
    public Red19() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red19";
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

public sealed class Red20 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red20");
    public Red20() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red20";
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

public sealed class Red21 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red21");
    public Red21() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red21";
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

public sealed class Red22 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red22");
    public Red22() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red22";
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

public sealed class Red23 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red23");
    public Red23() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red23";
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

public sealed class Red24 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red24");
    public Red24() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red24";
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

public sealed class Red25 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red25");
    public Red25() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red25";
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

public sealed class Red26 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red26");
    public Red26() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red26";
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

public sealed class Red27 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red27");
    public Red27() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red27";
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

public sealed class Red28 : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("Red28");
    public Red28() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "Red28";
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

