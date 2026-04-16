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

## Step 2 — Subclass SwarmTankBase

Every tank is a class that inherits from `SwarmTankBase`:

```csharp
using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

public class MyTank : SwarmTankBase
{
    // Declare the swarm this tank belongs to
    public override int  SwarmId => 3;
    public override TankRole Role => TankRole.Attacker;

    // Fields for AI state
    private string? _targetName;
    private bool    _retreating;
}
```

`SwarmId` determines team membership. Tanks with the same `SwarmId` are allies; different values are enemies. Use `0` for a solo tank.

---

## Step 3 — Implement OnStart

`OnStart` is called once when the arena is ready. Initialise anything that depends on arena dimensions here:

```csharp
protected override void OnStart()
{
    // Start the radar spinning immediately
    SetTurnRadarRight(double.MaxValue);

    // Any position-based initialisation
    _homeX = Arena.Width  / 2;
    _homeY = Arena.Height / 2;
}
```

---

## Step 4 — Implement OnTick

`OnTick` is called every simulation tick while the tank is alive. This is your main loop:

```csharp
protected override void OnTick()
{
    // Always spin the radar (45°/tick)
    SetTurnRadarRight(45);

    if (_retreating)
    {
        DoRetreat();
        return;
    }

    var target = GetFreshestEnemy();
    if (target != null)
    {
        ChaseAndFire(target);
    }
    else
    {
        Patrol();
    }
}
```

---

## Step 5 — Implement ChaseAndFire with Linear Prediction

```csharp
private void ChaseAndFire(RadarContact target)
{
    // --- Aim the gun using linear prediction ---
    const double firePower   = 2.0;
    double bulletSpeed       = 20 - 3 * firePower;

    double dx = target.Position.X - State.Position.X;
    double dy = target.Position.Y - State.Position.Y;
    double distance          = Math.Sqrt(dx * dx + dy * dy);
    double travelTime        = distance / bulletSpeed;

    // Predict where the target will be
    double predictedX = target.Position.X + target.VelocityVector.X * travelTime;
    double predictedY = target.Position.Y + target.VelocityVector.Y * travelTime;

    // Bearing to predicted position (0 = North, clockwise)
    double absoluteBearing = Math.Atan2(predictedX - State.Position.X,
                                        predictedY - State.Position.Y)
                             * (180.0 / Math.PI);

    double gunTurn = NormalizeTo180(absoluteBearing - State.GunHeading);
    SetTurnGunRight(gunTurn);

    // Fire when gun is aligned within 8 degrees
    if (Math.Abs(gunTurn) < 8)
        SetFire(firePower);

    // --- Steer toward the target ---
    double bodyTurn = NormalizeTo180(absoluteBearing - State.Heading);
    SetTurnRight(bodyTurn);
    SetAhead(distance > 80 ? 150 : 0);   // stop when close
}

// Normalise angle to (-180, +180]
private static double NormalizeTo180(double angle)
{
    while (angle >  180) angle -= 360;
    while (angle < -180) angle += 360;
    return angle;
}
```

---

## Step 6 — Implement Patrol

When no target is known, keep moving to avoid being a stationary target:

```csharp
private Vector2D _patrolPoint = new(0, 0);
private bool     _patrolPointSet;

private void Patrol()
{
    if (!_patrolPointSet || IsNear(_patrolPoint, 30))
    {
        // Pick a random point in the arena
        var rng = Random.Shared;
        _patrolPoint    = new Vector2D(
            rng.NextDouble() * (Arena.Width  - 100) + 50,
            rng.NextDouble() * (Arena.Height - 100) + 50);
        _patrolPointSet = true;
    }

    double bearing = Math.Atan2(
        _patrolPoint.X - State.Position.X,
        _patrolPoint.Y - State.Position.Y) * (180.0 / Math.PI);

    double turn = NormalizeTo180(bearing - State.Heading);
    SetTurnRight(turn);
    SetAhead(200);
}

private bool IsNear(Vector2D point, double radius)
{
    double dx = point.X - State.Position.X;
    double dy = point.Y - State.Position.Y;
    return Math.Sqrt(dx * dx + dy * dy) < radius;
}
```

---

## Step 7 — Handle Events

Override the relevant event methods:

```csharp
protected override void OnScannedTank(ScannedTankEventArgs e)
{
    // Track the target name for future ticks
    if (!e.Contact.IsAlly)
        _targetName = e.Contact.Name;
}

protected override void OnHitByBullet(HitByBulletEventArgs e)
{
    // Evade: turn perpendicular to the incoming bullet
    double evasionTurn = e.BearingDegrees > 0 ? -90 : 90;
    SetTurnRight(evasionTurn);
    SetAhead(60);
}

protected override void OnHitWall(HitWallEventArgs e)
{
    // Turn away from the wall and reverse briefly
    SetTurnRight(45);
    SetBack(40);
}

protected override void OnSwarmMessage(SwarmMessageEventArgs e)
{
    if (e.Message.Type == SwarmMessageType.TargetLocked)
        _targetName = e.Message.TargetName;
}
```

---

## Step 8 — Register the Tank

In `TankSwarmArena.cs`, add your tank to the list that the menu populates. Look for the existing `AddRedSwarm()` / `AddBlueSwarm()` methods and add a similar method:

```csharp
private void AddMySwarm()
{
    _engine.AddTank(new MyTank { Name = "MyTank1" });
    _engine.AddTank(new MyTank2 { Name = "MyTank2" });
}
```

Then wire it to a menu item in the designer or in the form's constructor.

---

## Common Mistakes

| Mistake | Effect | Fix |
|---------|--------|-----|
| Not normalising angle differences | Gun spins the wrong way around, oscillates | Always use `NormalizeTo180()` before `SetTurnGunRight` |
| Setting `SetAhead` to a fixed large value without checking distance | Runs into the target and loses energy | Reduce distance when close |
| Firing at full power from long range | Slow bullets easy to dodge | Lower power at range: faster bullet, lower damage |
| Ignoring `RadarMap` staleness | Firing at positions the enemy left 20 ticks ago | Check `Timestamp` vs `Arena.CurrentTick`; discard if > 5 ticks old |
| Calling `SetFire` every tick regardless of alignment | Wastes energy and flags misfires | Gate `SetFire` on gun alignment tolerance |
| Not handling `OnHitWall` | Tank gets trapped against a wall and takes damage per tick | Always add a wall-escape routine |

---

## Tips

- **Radar first**: start the radar spinning in `OnStart`; you cannot shoot what you cannot see.
- **Keep moving**: a stationary tank is easy to hit and takes wall damage if cornered.
- **Use RadarMap, not OnScannedTank alone**: `RadarMap` is updated by allies too; your tank may know about enemies it has never directly scanned.
- **Energy management**: check `State.Energy` before firing at high power; a dead tank contributes nothing.
- **Role as a contract**: set `Role` honestly — swarm-wide coordination logic (like `BlueCommander`'s orders) may key off `Role` values.

---

[← Built-in Tank AI](ch09-builtin-tanks.md) | [Table of Contents](TOC.md) | [Next: Arena Rendering →](ch11-rendering.md)
