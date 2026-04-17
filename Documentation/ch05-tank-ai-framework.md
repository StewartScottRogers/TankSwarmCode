# Chapter 5: Tank AI Framework

[← Physics Engine](ch04-physics-engine.md) | [Table of Contents](TOC.md) | [Next: Swarm Communication →](ch06-swarm-communication.md)

---

## Overview

The AI framework is built around a single abstract class: `SwarmTankBase` (`TankSwarmCode.SwarmTank/SwarmTank.cs`). Every tank AI — whether from the built-in swarms or a custom implementation — is a subclass of `SwarmTankBase`.

`SwarmTankBase` implements `ISwarmTank` (the engine's contract) and exposes a friendly API that hides the raw command buffer from the AI author.

---

## Lifecycle

```
Arena starts
    │
    └─► OnStart()                   — called once; set up initial state here

    ┌── tick N ──────────────────────────────────────────────────────────────
    │   OnTick(TickEventArgs e)      — main AI logic, runs every tick
    │   (if radar contact)       ─► OnScannedTank(ScannedTankEventArgs e)
    │   (if painted by enemy)    ─► OnPainted(PaintedEventArgs e)
    │   (if hit by bullet)       ─► OnHitByBullet(HitByBulletEventArgs e)
    │   (if hit wall)            ─► OnHitWall(HitWallEventArgs e)
    │   (if hit tank)            ─► OnHitTank(HitTankEventArgs e)
    │   (if bullet landed)       ─► OnBulletHit(BulletHitEventArgs e)
    │   (if ally message queued) ─► OnSwarmMessage(SwarmMessageEventArgs e)
    └────────────────────────────────────────────────────────────────────────

    (energy reaches 0)
    └─► OnDeath()                   — tank is destroyed

Round ends
    └─► OnRoundEnded(RoundEndedEventArgs e)   — all tanks receive this
```

All events fire within the same tick they occur, before `OnTick` on the subsequent tick.

---

## Properties Available to AI

These read-only properties are always current (updated each tick by the engine):

| Property | Type | Description |
|----------|------|-------------|
| `State` | `TankState` | Full immutable snapshot of this tank's current state |
| `Arena` | `IArenaContext` | Read-only view of the arena (dimensions, bullets, buildings) |
| `RadarMap` | `IReadOnlyDictionary<string, RadarContact>` | Known tank positions — both enemies and allies — from own scans and ally broadcasts |

### Commonly Used `State` Fields

| Field | Type | Notes |
|-------|------|-------|
| `Position` | `Vector2D` | Current (X, Y) in arena pixels |
| `Heading` | `double` | Body heading in degrees (0 = North, clockwise) |
| `GunHeading` | `double` | Absolute gun heading |
| `RadarHeading` | `double` | Absolute radar heading at end of this tick |
| `PrevRadarHeading` | `double` | Radar heading at start of tick; with `RadarHeading` defines the swept arc |
| `Velocity` | `double` | Current speed (negative = reversing) |
| `Energy` | `double` | Current energy (0–100) |
| `IsAlive` | `bool` | False after `OnDeath` fires |
| `Name` | `string` | Unique tank name |
| `SwarmId` | `int` | Swarm group (0 = solo) |
| `Role` | `TankRole` | Scout, Attacker, Defender, Support, EcmSpecialist, or None |
| `ActiveEcm` | `EcmMode` | ECM mode active this tick; visible to enemy scanners |

### `Arena` (IArenaContext)

| Member | Type | Description |
|--------|------|-------------|
| `ArenaWidth` | `double` | Arena width in pixels |
| `ArenaHeight` | `double` | Arena height in pixels |
| `TickNumber` | `long` | Current tick (starts at 1) |
| `LivingTankCount` | `int` | Total living tanks across all swarms |
| `GetSwarmSize(int swarmId)` | `int` | Living tanks in the specified swarm |
| `GetActiveBullets()` | `IReadOnlyList<BulletState>` | All active bullets (useful for evasion) |
| `Buildings` | `IReadOnlyList<BuildingDefinition>` | All buildings this round |

---

## Command API

Commands are **buffered**: calling a setter stores the intent. At the end of the tick the engine collects all commands and executes them. Calling a setter multiple times in one tick keeps the last value.

### Movement

```csharp
SetAhead(double distance)        // move forward
SetBack(double distance)         // move backward
SetTurnRight(double degrees)     // turn body clockwise
SetTurnLeft(double degrees)      // turn body counter-clockwise
```

Movement and turning can be combined in the same tick — the tank arcs while turning.

### Gun

```csharp
SetTurnGunRight(double degrees)
SetTurnGunLeft(double degrees)
SetFire(double power)            // power in [0.1, 3.0]
```

`SetFire` is silently ignored when the tank is jamming (`Jam` or `JamAndSpoof`) or when the tank lacks sufficient energy.

### Radar

```csharp
SetTurnRadarRight(double degrees)
SetTurnRadarLeft(double degrees)
```

To spin the radar continuously, call `SetTurnRadarRight(double.MaxValue)` every tick — the engine clamps it to 45°/tick.

Radar commands have no effect while jamming — the radar is physically offline.

### ECM

```csharp
SetEcm(EcmMode mode)   // Off / Jam / Spoof / Burnthrough / JamAndSpoof
```

Activates an ECM mode for the current tick. The engine deducts the energy cost in the ECM phase and applies the effect during radar processing. See [Chapter 13: ECM System](ch13-ecm-system.md).

| Mode | Effect | Energy/tick |
|------|--------|-------------|
| `Off` | No effect | 0 |
| `Jam` | Corrupts/drops enemy scans; disables own radar, firing, and radio | 0.5 |
| `Spoof` | Projects two drifting ghost contacts into enemy radar sweeps | 0.8 |
| `Burnthrough` | Pierces enemy jamming; filters ghost contacts | 0.3 |
| `JamAndSpoof` | Full Jam + full Spoof simultaneously; radar, firing, and radio all offline | 1.3 |

### Swarm Communication

```csharp
Broadcast(SwarmMessage message)
```

Queues a message for delivery to all living, non-jamming allies at the end of the current tick. See [Chapter 6: Swarm Communication](ch06-swarm-communication.md).

---

## Lifecycle Hook Reference

### `OnStart()`

```csharp
public virtual void OnStart() { }
```

Called once when the arena initialises or is resized. Initialise anything that depends on `Arena.ArenaWidth` / `Arena.ArenaHeight` here — patrol waypoints, home positions, etc.

---

### `OnTick(TickEventArgs e)`

```csharp
public virtual void OnTick(TickEventArgs e) { }
```

The primary AI method. Called every tick while the tank is alive.

| Field | Description |
|-------|-------------|
| `e.TickNumber` | Current simulation tick (same as `Arena.TickNumber`) |
| `e.LivingTankCount` | Number of tanks still alive across all swarms |

---

### `OnScannedTank(ScannedTankEventArgs e)`

```csharp
public virtual void OnScannedTank(ScannedTankEventArgs e) { }
```

Fired when the radar arc sweeps across any tank this tick (line-of-sight must be clear). Not fired while jamming.

The single field is `e.Result` (`ScanResult`):

| `e.Result` Field | Description |
|-----------------|-------------|
| `Name` | Scanned tank's name |
| `SwarmId` | Scanned tank's swarm (compare to `SwarmId` to detect allies) |
| `Bearing` | Relative bearing from this tank's body heading, (−180, 180] |
| `Distance` | Euclidean distance in pixels |
| `Heading` | Scanned tank's absolute body heading |
| `Velocity` | Scanned tank's current speed |
| `Energy` | Scanned tank's remaining energy |
| `Position` | Absolute arena position of the scanned tank |

The base class implementation automatically records the contact in `RadarMap` and broadcasts a `RadarShare` message to allies for non-ally contacts.

To check whether the scanned tank is an ally: `e.Result.SwarmId == SwarmId`.

---

### `OnPainted(PaintedEventArgs e)`

```csharp
public virtual void OnPainted(PaintedEventArgs e) { }
```

Fired when an **enemy** radar beam sweeps over this tank. The base class automatically broadcasts a `Painted` swarm message so allies learn the enemy scanner's position.

| Field | Description |
|-------|-------------|
| `e.PainterName` | Name of the tank whose radar painted this tank |
| `e.PainterSwarmId` | Swarm the painter belongs to |
| `e.PainterPosition` | Arena position of the painter at the moment of the scan |

Always call `base.OnPainted(e)` to preserve the auto-broadcast unless you intend to replace it.

---

### `OnHitByBullet(HitByBulletEventArgs e)`

```csharp
public virtual void OnHitByBullet(HitByBulletEventArgs e) { }
```

Fired when this tank is struck by a bullet.

| Field | Description |
|-------|-------------|
| `e.Bullet` | `BulletState` — the bullet that hit |
| `e.Bearing` | Relative bearing the bullet came from (negative = left, positive = right) |

Useful for evasive manoeuvres — turn perpendicular to the incoming bullet's bearing.

---

### `OnHitWall(HitWallEventArgs e)`

```csharp
public virtual void OnHitWall(HitWallEventArgs e) { }
```

Fired when this tank collides with an arena wall.

| Field | Description |
|-------|-------------|
| `e.Bearing` | Bearing of the wall relative to this tank's heading |

Use `e.Bearing` to determine which wall was hit and steer away from it.

---

### `OnHitTank(HitTankEventArgs e)`

```csharp
public virtual void OnHitTank(HitTankEventArgs e) { }
```

Fired on both tanks involved in a body collision.

| Field | Description |
|-------|-------------|
| `e.Other` | `TankState` snapshot of the other tank |
| `e.Bearing` | Relative bearing to the other tank |

Both tanks lose 0.6 energy from the collision.

---

### `OnBulletHit(BulletHitEventArgs e)`

```csharp
public virtual void OnBulletHit(BulletHitEventArgs e) { }
```

Fired on the **shooter** when one of their bullets hits an enemy.

| Field | Description |
|-------|-------------|
| `e.Bullet` | `BulletState` of the bullet that connected |
| `e.Victim` | `TankState` of the tank that was hit (post-damage snapshot) |

The shooter's energy is credited `e.Bullet.EnergyReturn` (= 3 × power) automatically by the engine before this event fires.

---

### `OnSwarmMessage(SwarmMessageEventArgs e)`

```csharp
public virtual void OnSwarmMessage(SwarmMessageEventArgs e) { }
```

Fired for each message delivered from an ally this tick. Not fired while the tank is jamming.

| Field | Description |
|-------|-------------|
| `e.Message` | `SwarmMessage` from the ally |

The base class implementation merges incoming `RadarShare` contacts into `RadarMap` before your override runs. Always call `base.OnSwarmMessage(e)` to keep the map current.

---

### `OnDeath()`

```csharp
public virtual void OnDeath() { }
```

Called when this tank's energy drops to 0. The tank stops issuing commands but its destroyed hull remains visible until the round resets.

---

### `OnRoundEnded(RoundEndedEventArgs e)`

```csharp
public virtual void OnRoundEnded(RoundEndedEventArgs e) { }
```

Fired on all tanks — alive and dead — when the round ends.

| Field | Description |
|-------|-------------|
| `e.Won` | `true` if this tank's swarm was the last one standing |
| `e.TotalTicks` | Total number of ticks the round lasted |

---

## Radar Helper Methods

### `GetFreshestEnemy(int staleAfterTicks = 30)`

```csharp
protected RadarContact? GetFreshestEnemy(int staleAfterTicks = 30)
```

Returns the enemy contact in `RadarMap` with the most recent `Timestamp`, filtered to exclude ally contacts and contacts older than `staleAfterTicks` ticks. Returns `null` if none exists.

### `GetFreshestContact(int staleAfterTicks = 30, bool includeAllies = false)`

```csharp
protected RadarContact? GetFreshestContact(int staleAfterTicks = 30, bool includeAllies = false)
```

Same as above but can optionally include allied contacts (for situational awareness, not targeting).

---

## Minimal Tank Example

```csharp
using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

public class SimpleTank : SwarmTankBase
{
    public SimpleTank()
    {
        SwarmId = 3;
        Role = TankRole.Attacker;
    }

    public override string Name => "Simple";

    public override void OnStart()
    {
        // Spin the radar from the first tick
        SetTurnRadarRight(double.MaxValue);
    }

    public override void OnTick(TickEventArgs e)
    {
        SetTurnRadarRight(45);   // keep spinning
        SetAhead(100);

        var target = GetFreshestEnemy();
        if (target != null)
        {
            double bearing = Math.Atan2(
                target.Position.X - State.Position.X,
                target.Position.Y - State.Position.Y) * (180.0 / Math.PI);
            double gunTurn = NormalizeAngle(bearing - State.GunHeading);
            SetTurnGunRight(gunTurn);
            if (Math.Abs(gunTurn) < 10)
                SetFire(1.5);
        }
    }

    public override void OnPainted(PaintedEventArgs e)
    {
        base.OnPainted(e);   // auto-broadcasts [PAINTED] to allies
        SetTurnRight(30);    // evasive when detected
    }

    public override void OnHitWall(HitWallEventArgs e)
    {
        SetTurnRight(90);
        SetBack(40);
    }

    private static double NormalizeAngle(double a)
    {
        while (a >  180) a -= 360;
        while (a < -180) a += 360;
        return a;
    }
}
```

For a complete working example with linear-prediction firing and swarm messaging, see [Chapter 10: Building Your Own Tank](ch10-custom-tank.md).

---

[← Physics Engine](ch04-physics-engine.md) | [Table of Contents](TOC.md) | [Next: Swarm Communication →](ch06-swarm-communication.md)
