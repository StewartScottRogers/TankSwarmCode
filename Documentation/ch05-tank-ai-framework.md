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
    └─► OnStart()              — called once; set up initial state here

    ┌── tick N ──────────────────────────────────────────
    │   OnTick()               — main AI logic, runs every tick
    │   (if radar contact)  ─► OnScannedTank()
    │   (if hit by bullet)  ─► OnHitByBullet()
    │   (if hit wall)       ─► OnHitWall()
    │   (if hit tank)       ─► OnHitTank()
    │   (if bullet landed)  ─► OnBulletHit()
    │   (if message queued) ─► OnSwarmMessage()
    └────────────────────────────────────────────────────

    (energy reaches 0)
    └─► OnDeath()              — tank is destroyed

Round ends
    └─► OnRoundEnded()         — all tanks receive this; Won flag indicates result
```

All events are fired within the same tick they occur, before `OnTick` on the subsequent tick.

---

## Properties Available to AI

These read-only properties are always current (updated each tick by the engine):

| Property | Type | Description |
|----------|------|-------------|
| `State` | `TankState` | Full immutable snapshot of this tank's current state |
| `Arena` | `IArenaContext` | Read-only view of the arena (dimensions, all tanks, bullets, buildings) |
| `RadarMap` | `Dictionary<string, RadarContact>` | Known enemy/ally positions (own scans + ally broadcasts) |

### Commonly Used `State` Fields

| Field | Type | Notes |
|-------|------|-------|
| `Position` | `Vector2D` | Current (X, Y) in arena pixels |
| `Heading` | `double` | Body heading in degrees (0 = North, clockwise) |
| `GunHeading` | `double` | Absolute gun heading |
| `RadarHeading` | `double` | Absolute radar heading |
| `Velocity` | `double` | Current speed (negative = reversing) |
| `Energy` | `double` | Current energy (0–100) |
| `IsAlive` | `bool` | False after `OnDeath` |
| `Name` | `string` | Unique tank name |
| `SwarmId` | `int` | Swarm group (0 = solo) |
| `Role` | `TankRole` | Scout, Attacker, Defender, Support, or EcmSpecialist |
| `ActiveEcm` | `EcmMode` | ECM mode active this tick (Off / Jam / Spoof / Burnthrough) |

---

## Command API

Commands are **buffered**: calling a setter stores the intent. At the end of the tick the engine collects all commands and executes them. Calling a setter multiple times in the same tick keeps the last value.

### Movement

```csharp
SetAhead(double distance)        // move forward
SetBack(double distance)         // move backward
SetTurnRight(double degrees)     // turn body clockwise
SetTurnLeft(double degrees)      // turn body counter-clockwise
```

You can combine movement and turning in the same tick. The tank will turn while moving (arcing path).

### Gun

```csharp
SetTurnGunRight(double degrees)
SetTurnGunLeft(double degrees)
SetFire(double power)            // power in [0.1, 3.0]
```

### Radar

```csharp
SetTurnRadarRight(double degrees)
SetTurnRadarLeft(double degrees)
```

To keep the radar locked on a target, recalculate and set the radar turn every tick. A common pattern is to spin the radar continuously by calling `SetTurnRadarRight(double.MaxValue)` (clamped to 45°/tick by the engine).

### ECM (Electronic Counter-Measures)

```csharp
SetEcm(EcmMode mode)   // Off / Jam / Spoof / Burnthrough
```

Activates an ECM mode for the current tick. The engine deducts the energy cost and applies the effect during radar processing. See [Chapter 13: ECM System](ch13-ecm-system.md) for full details.

| Mode | Effect | Energy/tick |
|------|--------|-------------|
| `Off` | No effect | 0 |
| `Jam` | Corrupts / drops enemy radar scans of this tank | 0.5 |
| `Spoof` | Projects ghost contacts into enemy radar sweeps | 0.8 |
| `Burnthrough` | Pierces enemy jamming; filters ghost contacts | 0.3 |

---

## Lifecycle Hook Reference

### `OnStart()`

```csharp
protected virtual void OnStart() { }
```

Called once when the arena initialises or is resized. Use this to initialise fields that depend on `Arena.Width` / `Arena.Height`, such as patrol waypoints or home positions.

---

### `OnTick()`

```csharp
protected virtual void OnTick() { }
```

The primary AI method. Called every tick while the tank is alive. Issue movement and firing commands here. This is where the majority of AI logic lives.

---

### `OnScannedTank(ScannedTankEventArgs e)`

```csharp
protected virtual void OnScannedTank(ScannedTankEventArgs e) { }
```

Fired when the radar arc sweeps across another tank during this tick (line-of-sight to the target must be clear of buildings).

`ScannedTankEventArgs` fields:

| Field | Description |
|-------|-------------|
| `Contact` | `RadarContact` — full snapshot of the detected tank |
| `BearingDegrees` | Relative bearing from this tank's body heading (−180 to +180) |
| `Distance` | Euclidean distance to the scanned tank |

The engine automatically adds the contact to `RadarMap` and broadcasts a `RadarShare` message to allies before this event is raised.

---

### `OnHitByBullet(HitByBulletEventArgs e)`

```csharp
protected virtual void OnHitByBullet(HitByBulletEventArgs e) { }
```

Fired when this tank is struck by a bullet.

| Field | Description |
|-------|-------------|
| `Bullet` | `BulletState` — the bullet that hit |
| `BearingDegrees` | Relative bearing the bullet came from |
| `Damage` | Energy lost |

Useful for evasive manoeuvres — turn perpendicular to the incoming bullet's heading.

---

### `OnHitWall(HitWallEventArgs e)`

```csharp
protected virtual void OnHitWall(HitWallEventArgs e) { }
```

| Field | Description |
|-------|-------------|
| `Damage` | Energy lost from the wall collision |
| `Velocity` | Speed at the moment of impact |

---

### `OnHitTank(HitTankEventArgs e)`

```csharp
protected virtual void OnHitTank(HitTankEventArgs e) { }
```

| Field | Description |
|-------|-------------|
| `OtherTank` | `TankState` snapshot of the tank that was collided with |
| `BearingDegrees` | Relative bearing to the other tank |

Fires for both tanks involved in the collision. Both lose 0.6 energy.

---

### `OnBulletHit(BulletHitEventArgs e)`

```csharp
protected virtual void OnBulletHit(BulletHitEventArgs e) { }
```

Fired on the **shooter** when one of their bullets hits an enemy.

| Field | Description |
|-------|-------------|
| `Bullet` | The bullet that connected |
| `HitTank` | `TankState` of the tank that was hit |
| `Damage` | Damage dealt |
| `EnergyReturn` | Energy returned to this tank (= 3 × power) |

---

### `OnSwarmMessage(SwarmMessageEventArgs e)`

```csharp
protected virtual void OnSwarmMessage(SwarmMessageEventArgs e) { }
```

Fired for each message delivered from an ally this tick. See [Chapter 6: Swarm Communication](ch06-swarm-communication.md) for the full message model.

---

### `OnDeath()`

```csharp
protected virtual void OnDeath() { }
```

Called when this tank's energy drops to 0. The tank stops issuing commands but its destroyed hull remains visible in the arena.

---

### `OnRoundEnded(RoundEndedEventArgs e)`

```csharp
protected virtual void OnRoundEnded(RoundEndedEventArgs e) { }
```

| Field | Description |
|-------|-------------|
| `Won` | `true` if this tank's swarm won the round |
| `SurvivingSwarmId` | The swarm ID of the winner (or 0 for mutual destruction) |

Fired on all tanks — alive and dead — at round end. Use this to accumulate statistics across rounds if desired.

---

## Helper Methods

### `GetFreshestEnemy()`

```csharp
protected RadarContact? GetFreshestEnemy()
```

Scans the `RadarMap` and returns the enemy contact with the most recent timestamp, or `null` if no enemies are known. A quick way to find a target without writing custom RadarMap iteration.

### `Broadcast(SwarmMessage message)`

```csharp
protected void Broadcast(SwarmMessage message)
```

Queues a message for delivery to all living allies before their next tick. See [Chapter 6](ch06-swarm-communication.md).

---

## Minimal Tank Example

```csharp
using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Models;

public class SimpleTank : SwarmTankBase
{
    protected override void OnStart()
    {
        // Spin the radar continuously
        SetTurnRadarRight(double.MaxValue);
    }

    protected override void OnTick()
    {
        // Keep moving forward
        SetAhead(100);

        // Spin to look for targets
        SetTurnRadarRight(45);

        // Fire at the freshest known enemy
        var target = GetFreshestEnemy();
        if (target != null)
        {
            // Aim gun at target
            double angleToTarget = /* bearing calculation */ 0;
            SetTurnGunRight(angleToTarget);
            SetFire(1.5);
        }
    }

    protected override void OnHitWall(HitWallEventArgs e)
    {
        // Turn away from the wall
        SetTurnRight(90);
    }
}
```

For a complete working example with linear-prediction firing, see [Chapter 10: Building Your Own Tank](ch10-custom-tank.md).

---

[← Physics Engine](ch04-physics-engine.md) | [Table of Contents](TOC.md) | [Next: Swarm Communication →](ch06-swarm-communication.md)
