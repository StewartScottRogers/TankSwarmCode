# Chapter 4: Physics Engine

[← Architecture](ch03-architecture.md) | [Table of Contents](TOC.md) | [Next: Tank AI Framework →](ch05-tank-ai-framework.md)

---

## Overview

`ArenaEngine` (`TankSwarmCode.Arena/ArenaEngine.cs`) is the heart of the simulation. It owns all mutable runtime state and drives a deterministic tick-by-tick physics loop. Each tick corresponds to one discrete time step — there is no sub-tick interpolation.

---

## Tick Cycle

Every call to `ArenaEngine.Tick()` executes the following phases in order:

```
1.  OnTick callbacks              — parallel; each tank writes its TankCommand
2.  FlushCommands                 — sequential; collect all TankCommands
3.  ApplyMovement                 — sequential; update positions; wall & building collisions
4.  ApplyFiring                   — sequential; create new bullets (skipped for jammers)
5.  ApplyEcm                      — sequential; deduct ECM energy costs; set ActiveEcm;
                                    update ghost echo positions
6.  MoveBullets                   — parallel; advance bullet positions; decay deflected bullets
7.  CheckBulletTankCollisions     — sequential; apply damage; lethal hits remove bullet;
                                    non-lethal hits deflect bullet (ricochet)
8.  CheckTankTankCollisions       — sequential; apply mutual 0.6 damage; push tanks apart
9.  ProcessRadarScans             — parallel; fire OnScannedTank events; ECM drop/corrupt/ghost
                                    injection; fire OnPainted on scanned tanks
10. DeliverSwarmMessages          — sequential; route broadcasts to living, non-jamming allies;
                                    fire OnSwarmMessage for each delivered message
11. UpdateTankStates              — parallel; push fresh immutable TankState to each tank
12. RebuildSnapshots              — remove spent bullets; publish RicochetFlashes &
                                    ActiveGhostEchoes for the renderer
13. CheckRoundEnd                 — determine if a winner exists; fire OnRoundEnded
```

The ordering is significant: movement and firing happen before collision checks, ECM is applied before radar scans, and state is synchronised after all physics are resolved.

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
- Position update: `x += sin(heading°) × velocity`, `y -= cos(heading°) × velocity` (screen Y-axis is inverted).

### Turning

The maximum body turn rate depends on current speed:

```
BodyTurnRate = MaxTurnRate - VelocityTurnPenalty × |velocity|
             = 10° - 0.75° × |velocity|
```

At maximum velocity (8 px/tick) the body can turn at most 4°/tick. At rest it can turn up to 10°/tick. This prevents high-speed pivoting.

The gun and radar are not affected by velocity:

| Component | Max Turn Rate |
|-----------|--------------|
| Body | 10° − 0.75° × \|velocity\| |
| Gun | 20°/tick |
| Radar | 45°/tick |

When the body turns, the gun and radar headings rotate by the same amount (they are mounted on the body). Separate gun/radar turn commands are applied on top of that rotation.

---

## Wall Collision

When a tank's next position would place it outside the arena boundary, movement is blocked and velocity is zeroed. Wall contact also inflicts damage:

```
WallDamage = max(|velocity| × 0.5 − 1.0, 0)
```

At maximum velocity (8 px/tick) a wall hit deals 3.0 damage. Slow-speed grazes below 2 px/tick cost nothing.

`OnHitWall(HitWallEventArgs e)` fires immediately with `e.Bearing` (relative to the tank's heading).

---

## Building Collision

Buildings are axis-aligned rectangles. The engine uses a circle-AABB test with radius `TankHalfSize × √2` (~12.7 px) to ensure the tank's body never visually penetrates a corner.

When a collision is detected:
1. The tank's position is pushed back to the last safe position.
2. Velocity is zeroed.
3. `OnHitTank` is **not** fired (it is a building, not another tank).

---

## Tank–Tank Collision

When two tanks overlap (combined radii = `TankHalfSize × 2 = 18 px`):

1. Each tank loses **0.6 energy** (`ArenaConstants.TankCollisionDamage`).
2. Both tanks are pushed apart along the collision axis and then re-checked against buildings.
3. `OnHitTank(HitTankEventArgs e)` is fired on both tanks; `e.Other` is the other tank's `TankState`, `e.Bearing` is its relative bearing.

---

## Bullet Physics

### Firing

`SetFire(power)` queues a firing command. Constraints:

- Power is clamped to `[0.1, 3.0]`.
- If the tank's energy is below the requested power, the shot is silently cancelled.
- Firing costs `power` energy immediately.
- Tanks running `EcmMode.Jam` or `EcmMode.JamAndSpoof` cannot fire — the shot is suppressed.

### Bullet Speed

```
BulletSpeed = 20 − 3 × power   (px/tick)
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
Damage       = 4 × power + (power > 1 ? 2 × (power − 1) : 0)
EnergyReturn = 3 × power     (credited to the shooter on hit)
```

| Power | Damage | Energy Return |
|-------|--------|--------------|
| 0.1 | 0.4 | 0.3 |
| 1.0 | 4.0 | 3.0 |
| 2.0 | 10.0 | 6.0 |
| 3.0 | 16.0 | 9.0 |

### Bullet Deactivation

A bullet is deactivated (removed) when it:
- Delivers a **lethal** hit to a tank (tank energy drops to ≤ 0).
- Intersects a building rectangle.
- Exits the arena boundary.

### Bullet Ricochet (Non-Lethal Hits)

When a bullet hits a tank but the damage is **not** enough to destroy it, the bullet is **deflected** rather than removed:

1. Bounce heading = incoming heading + 180° ± random spread of ±25°.
2. Speed is reduced to **40 %** of its original speed at the moment of impact.
3. Each subsequent tick the deflected bullet loses **28 %** of its remaining speed (multiplicative decay).
4. When speed falls below 0.4 px/tick the bullet is removed.

Deflected bullets skip all further tank collisions — they are purely visual. The impact position of every ricochet is published on `ArenaEngine.RicochetFlashes` for the renderer to draw a single-frame flash ring.

---

## Radar Scan Processing

After all movement, ECM, and collisions are resolved, each tank's radar arc is evaluated. See [Chapter 7: Radar System](ch07-radar-system.md) for full details.

---

## Round End Condition

After each tick, the engine counts living tanks by swarm:

- If exactly one swarm has survivors → that swarm wins; `OnRoundEnded(Won: true)` fires for survivors, `OnRoundEnded(Won: false)` for all others.
- If no swarms have survivors (mutual annihilation) → `OnRoundEnded(Won: false)` for all.
- Solo tanks (`SwarmId = 0`) are treated as their own one-member swarm.

---

## Safe AI Execution

Every tank callback (`OnTick`, `OnScannedTank`, etc.) is wrapped in a `try/catch`. An exception thrown inside a tank's AI logic is caught, logged, and the tank continues to exist with no command for that tick. This prevents a buggy AI from crashing the engine.

---

[← Architecture](ch03-architecture.md) | [Table of Contents](TOC.md) | [Next: Tank AI Framework →](ch05-tank-ai-framework.md)
