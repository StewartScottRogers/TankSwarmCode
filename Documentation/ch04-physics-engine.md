# Chapter 4: Physics Engine

[← Architecture](ch03-architecture.md) | [Table of Contents](TOC.md) | [Next: Tank AI Framework →](ch05-tank-ai-framework.md)

---

## Overview

`ArenaEngine` (`TankSwarmCode.Arena/ArenaEngine.cs`) is the heart of the simulation. It owns the mutable runtime state and drives a deterministic tick-by-tick physics loop. Each tick corresponds to one discrete time step — there is no sub-tick interpolation.

---

## Tick Cycle

Every call to `ArenaEngine.Tick()` executes the following phases in order:

```
1.  OnTick callbacks          — parallel, each tank writes its TankCommand
2.  FlushCommands             — sequential, collect all TankCommands
3.  ApplyMovement             — sequential, update positions, handle wall & building collisions
4.  ApplyFiring               — sequential, create new bullets
5.  MoveBullets               — parallel, advance each bullet position
6.  CheckBulletTankCollisions — sequential, apply damage & deactivate bullets
7.  CheckTankTankCollisions   — sequential, apply damage & push apart
8.  ProcessRadarScans         — parallel, fire ScannedTank events, build RadarContacts
9.  DeliverSwarmMessages      — sequential, merge broadcasts into ally RadarMaps
10. UpdateTankStates          — parallel, push fresh immutable TankState to each tank
11. RebuildSnapshots          — remove spent bullets, rebuild cached snapshot lists
12. CheckRoundEnd             — determine if a winner exists; fire OnRoundEnded
```

The ordering is significant: movement and firing happen before collision checks, and radar scans happen after all positions are settled for the tick.

---

## Movement Model

### Velocity and Acceleration

Tank movement follows the Robocode model:

| Parameter | Value |
|-----------|-------|
| Max velocity | 8.0 px/tick |
| Acceleration | 1.0 px/tick² |
| Deceleration | 2.0 px/tick² |

- `SetAhead(distance)` — accelerates toward max velocity while distance remains.
- `SetBack(distance)` — accelerates in reverse.
- Deceleration is applied when stopping or reversing direction.

### Turning

The maximum body turn rate depends on current speed:

```
MaxTurnRate = 10° - 0.75° × |velocity|
```

At maximum velocity (8 px/tick) the body can turn at most 4°/tick. At rest it can turn up to 10°/tick. This prevents high-speed pivoting.

The gun and radar are not affected by velocity:

| Component | Max Turn Rate |
|-----------|--------------|
| Body | 10° − 0.75° × \|velocity\| |
| Gun | 20°/tick |
| Radar | 45°/tick |

When the body turns, the gun and radar headings rotate by the same amount (they are mounted on the body). Separate gun/radar turn commands are applied on top of that.

---

## Wall Collision

When a tank's next position would place it outside the arena boundary, its movement is blocked and velocity is zeroed. Wall contact also inflicts damage:

```
WallDamage = max(|velocity| × 0.5 − 1.0, 0)
```

At maximum velocity (8 px/tick) a wall hit deals 3.0 damage. Slow-speed grazes deal nothing (threshold at 2 px/tick).

The `OnHitWall` event is fired immediately.

---

## Building Collision

Buildings are axis-aligned rectangles. The engine uses a **circle-AABB** test with radius equal to `TankHalfSize × √2` (~12.7 px) to ensure the tank's body never visually penetrates a corner.

When a collision is detected:
1. The tank's position is pushed back to the last safe position.
2. Velocity is zeroed.
3. `OnHitTank` is **not** fired (it's a building, not another tank); the movement is simply blocked.

---

## Tank–Tank Collision

When two tanks overlap (combined radii = `TankHalfSize × 2`):

1. Each tank loses **0.6 energy**.
2. The tanks are pushed apart along the collision axis.
3. `OnHitTank` is fired on both tanks with a `HitTankEventArgs` containing the other tank's `TankState`.

---

## Bullet Physics

### Firing

`SetFire(power)` queues a firing command. Constraints:

- Power is clamped to `[0.1, 3.0]`.
- If the tank's energy is below the requested power, the shot is silently cancelled.
- Firing costs `power` energy immediately.

### Bullet Speed

```
BulletSpeed = 20 − 3 × power
```

| Power | Speed (px/tick) |
|-------|----------------|
| 0.1 | 19.7 |
| 1.0 | 17.0 |
| 2.0 | 14.0 |
| 3.0 | 11.0 |

Higher power = slower bullet. This creates a trade-off between damage and hit probability at range.

### Damage and Energy Return

```
Damage      = 4 × power + (power > 1 ? 2 × (power − 1) : 0)
EnergyReturn = 3 × power          (returned to shooter on hit)
```

| Power | Damage | Energy Return |
|-------|--------|--------------|
| 0.1 | 0.4 | 0.3 |
| 1.0 | 4.0 | 3.0 |
| 2.0 | 10.0 | 6.0 |
| 3.0 | 18.0 | 9.0 |

### Bullet Deactivation

A bullet is deactivated (removed) when it:
- Hits a tank.
- Intersects a building rectangle.
- Exits the arena boundary.

---

## Radar Scan Processing

After all movement and collisions are resolved, each tank's radar arc is evaluated. See [Chapter 7: Radar System](ch07-radar-system.md) for full details.

---

## Round End Condition

After each tick, the engine counts living tanks by swarm:

- If exactly one swarm has survivors → that swarm wins; `OnRoundEnded(Won: true)` is fired for survivors, `OnRoundEnded(Won: false)` for defeated tanks.
- If no swarms have survivors (mutual annihilation) → `OnRoundEnded(Won: false)` for all.
- Solo tanks (`SwarmId = 0`) are treated as their own one-member swarm.

---

## Safe AI Execution

Every tank callback (`OnTick`, `OnScannedTank`, etc.) is wrapped in a `try/catch`. An exception thrown in a tank's AI logic is caught, logged, and the tank continues to exist in the simulation (issuing no command that tick). This prevents a buggy AI from crashing the engine.

---

[← Architecture](ch03-architecture.md) | [Table of Contents](TOC.md) | [Next: Tank AI Framework →](ch05-tank-ai-framework.md)
