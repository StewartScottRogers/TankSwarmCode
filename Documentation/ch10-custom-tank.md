# Chapter 10: Building Your Own Tank

[← Built-in Tank AI](ch09-builtin-tanks.md) | [Table of Contents](TOC.md) | [Next: Arena Rendering →](ch11-rendering.md)

---

## Step 1 — Create a New Project

Add a new C# class library project to the solution. Reference the base class package:

```xml
<!-- YourSwarm.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\TankSwarmCode.SwarmTank\TankSwarmCode.SwarmTank.csproj" />
  </ItemGroup>
</Project>
```

---

## Step 2 — Choose a Base Class

You have two options:

- **`SwarmTankBase`** — full control; implement all AI logic yourself. Use this for bespoke strategies.
- **`SwarmBrainBase`** — inherit the full coordination brain (leader election, epoch strategies, volley scheduling, ECM handling) and configure it via a `TankConfig` record. Use this when you want the built-in team coordination and only need to tune parameters. See [Chapter 9: Built-in Tank AI Examples](ch09-builtin-tanks.md) for the `SwarmBrainBase` API.

The rest of this chapter uses `SwarmTankBase` to show a complete ground-up implementation.

Every tank is a class that inherits from `SwarmTankBase`. Set identity in the constructor — `SwarmId` and `Role` are regular properties, not virtual:

```csharp
using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

public class MyTank : SwarmTankBase
{
    public MyTank(string name)
    {
        _name    = name;
        SwarmId  = 3;
        Role     = TankRole.Attacker;
    }

    private readonly string _name;
    public override string Name => _name;

    // AI state fields
    private string? _priorityTarget;
    private bool    _retreating;
}
```

`SwarmId` determines team membership. Tanks with the same `SwarmId` are allies; different values are enemies. Use `0` for a solo tank.

---

## Step 3 — Implement OnStart

`OnStart` is called once when the arena is ready. Initialise anything that depends on arena dimensions here:

```csharp
public override void OnStart()
{
    // Start the radar spinning immediately
    SetTurnRadarRight(double.MaxValue);
}
```

Use `Arena.ArenaWidth` and `Arena.ArenaHeight` (not `Width`/`Height`) to read the arena size.

---

## Step 4 — Implement OnTick

`OnTick(TickEventArgs e)` is called every tick while the tank is alive. This is your main loop:

```csharp
public override void OnTick(TickEventArgs e)
{
    SetTurnRadarRight(45);   // keep radar spinning

    if (_retreating)
    {
        DoRetreat();
        return;
    }

    var target = GetFreshestEnemy();
    if (target != null)
        ChaseAndFire(target);
    else
        Patrol();
}
```

`e.TickNumber` and `e.LivingTankCount` are available if needed.

---

## Step 5 — Implement ChaseAndFire with Linear Prediction

```csharp
private void ChaseAndFire(RadarContact target)
{
    const double firePower = 2.0;
    double bulletSpeed     = 20.0 - 3.0 * firePower;

    double dx       = target.Position.X - State.Position.X;
    double dy       = target.Position.Y - State.Position.Y;
    double distance = Math.Sqrt(dx * dx + dy * dy);
    double travelTime = distance / bulletSpeed;

    // Predict where the target will be when the bullet arrives
    double predictedX = target.Position.X + target.VelocityVector.X * travelTime;
    double predictedY = target.Position.Y + target.VelocityVector.Y * travelTime;

    double absoluteBearing = Math.Atan2(predictedX - State.Position.X,
                                        predictedY - State.Position.Y)
                             * (180.0 / Math.PI);

    double gunTurn = NormalizeAngle(absoluteBearing - State.GunHeading);
    SetTurnGunRight(gunTurn);

    if (Math.Abs(gunTurn) < 8)
        SetFire(firePower);

    // Steer toward the target
    double bodyTurn = NormalizeAngle(absoluteBearing - State.Heading);
    SetTurnRight(bodyTurn);
    SetAhead(distance > 80 ? 150 : 0);
}

private static double NormalizeAngle(double a)
{
    while (a >  180) a -= 360;
    while (a < -180) a += 360;
    return a;
}
```

---

## Step 6 — Implement Patrol

When no enemy is known, keep moving to avoid being a stationary target:

```csharp
private Vector2D _patrolTarget;
private bool     _hasPatrolTarget;

private void Patrol()
{
    if (!_hasPatrolTarget || State.Position.DistanceTo(_patrolTarget) < 30)
    {
        var rng = Random.Shared;
        _patrolTarget = new Vector2D(
            rng.NextDouble() * (Arena.ArenaWidth  - 100) + 50,
            rng.NextDouble() * (Arena.ArenaHeight - 100) + 50);
        _hasPatrolTarget = true;
    }

    double bearing = State.Position.BearingTo(_patrolTarget);
    SetTurnRight(NormalizeAngle(bearing - State.Heading));
    SetAhead(200);
}
```

---

## Step 7 — Handle Events

Override the relevant event methods:

```csharp
public override void OnScannedTank(ScannedTankEventArgs e)
{
    base.OnScannedTank(e);   // records contact in RadarMap; RadarShare to allies

    // Track the last scanned enemy
    if (e.Result.SwarmId != SwarmId)
        _priorityTarget = e.Result.Name;
}

public override void OnHitByBullet(HitByBulletEventArgs e)
{
    // e.Bearing is relative: negative = came from left, positive = from right
    double evasionTurn = e.Bearing > 0 ? -90 : 90;
    SetTurnRight(evasionTurn);
    SetAhead(60);
}

public override void OnHitWall(HitWallEventArgs e)
{
    // e.Bearing tells us which wall; turn away and reverse
    SetTurnRight(90);
    SetBack(40);
}

public override void OnPainted(PaintedEventArgs e)
{
    base.OnPainted(e);   // auto-broadcast [PAINTED] to allies
    SetTurnRight(20);    // evasive twitch
}

public override void OnSwarmMessage(SwarmMessageEventArgs e)
{
    base.OnSwarmMessage(e);   // merge RadarShare contacts

    if (e.Message.Type == SwarmMessageType.TargetLocked)
        _priorityTarget = e.Message.TargetName;
}

public override void OnRoundEnded(RoundEndedEventArgs e)
{
    // e.Won and e.TotalTicks available for statistics
}
```

---

## Step 8 — Test Your Tank

### Option A — GUI

In `TankSwarmArena.cs`, add a method to register your swarm and wire it to a menu item:

```csharp
private void AddMySwarm()
{
    _engine.AddTank(new MyTank("MyTank1"));
    _engine.AddTank(new MyTank("MyTank2"));
}
```

### Option B — Headless CLI (no GUI required)

Build your project as a class library, then point the CLI at the output DLL:

```bash
dotnet build -c Release

dotnet run --project TankSwarmCode.Cli/TankSwarmCode.Cli.csproj \
  --bot1 YourSwarm/bin/Release/net10.0/YourSwarm.dll \
  --bot2 TankSwarmCode.SwarmTanks.Blue/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Blue.dll \
  --batch 50 --format table
```

Use `--list` to verify the CLI can find your tanks before running a match:

```bash
TankSwarmCode.Cli --list YourSwarm/bin/Release/net10.0/YourSwarm.dll
```

See [Chapter 15: Headless CLI Runner](ch15-cli.md) for all options.

---

## Common Mistakes

| Mistake | Effect | Fix |
|---------|--------|-----|
| Not normalising angle differences | Gun spins the wrong way; oscillates | Always use `NormalizeAngle()` before any `SetTurnGunRight` call |
| Using `Arena.Width` / `Arena.Height` | Compile error; these properties don't exist | Use `Arena.ArenaWidth` / `Arena.ArenaHeight` |
| Calling `SetAhead` to a fixed large value without checking distance | Runs into the target; collision damage | Reduce move distance when close |
| Firing at full power from long range | Slow bullets are easy to dodge | Lower power at range for faster bullets |
| Ignoring `RadarMap` staleness | Firing at positions the enemy vacated ticks ago | Compare `Timestamp` to `Arena.TickNumber`; discard contacts older than ~5 ticks |
| Calling `SetFire` every tick regardless of alignment | Wastes energy | Gate `SetFire` on gun alignment tolerance |
| Not calling `base.OnScannedTank(e)` | `RadarMap` never updates; no `RadarShare` to allies | Always call base first |
| Not calling `base.OnSwarmMessage(e)` | `RadarShare` contacts are never merged | Always call base first |
| Not handling `OnHitWall` | Tank gets trapped; takes damage each tick | Add a wall-escape routine |

---

## Tips

- **Radar first**: start the radar spinning in `OnStart` — you cannot shoot what you cannot see.
- **Keep moving**: a stationary tank is easy to hit and takes wall damage if cornered.
- **Use RadarMap, not `OnScannedTank` alone**: `RadarMap` is updated by allies too; your tank may know about enemies it has never directly scanned.
- **Energy management**: check `State.Energy` before firing at high power; a dead tank contributes nothing.
- **ECM awareness**: if `Arena.GetActiveBullets()` shows a bullet heading your way, consider `SetEcm(EcmMode.Jam)` as a momentary defensive measure.
- **Role as a contract**: set `Role` honestly — `SwarmBrainBase`'s ECM handling checks `Role == EcmSpecialist`, and any coordination logic you write can use `Role` to differentiate behaviour across swarm members.

---

[← Built-in Tank AI](ch09-builtin-tanks.md) | [Table of Contents](TOC.md) | [Next: Arena Rendering →](ch11-rendering.md)
