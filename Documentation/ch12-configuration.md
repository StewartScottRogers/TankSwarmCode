# Chapter 12: Configuration & Constants

[← Arena Rendering](ch11-rendering.md) | [Table of Contents](TOC.md) | [Next: ECM System →](ch13-ecm-system.md) | [Next: ECM System →](ch13-ecm-system.md)

---

## Physics Constants (`ArenaConstants.cs`)

All physics values are defined in `TankSwarmCode.SwarmTank.Interfaces/ArenaConstants.cs`. They apply to every tank in every round.

### Movement

| Constant | Value | Description |
|----------|-------|-------------|
| `MaxVelocity` | 8.0 px/tick | Maximum forward/reverse speed |
| `Acceleration` | 1.0 px/tick² | Rate of speed increase |
| `Deceleration` | 2.0 px/tick² | Rate of speed decrease (braking or reversing) |
| `MaxTurnRateBase` | 10.0 °/tick | Body turn rate at zero velocity |
| `TurnRateVelocityFactor` | 0.75 °/tick per px/tick | Reduction in turn rate per unit of speed |

The effective body turn rate at any given tick:

```
BodyTurnRate = MaxTurnRateBase − TurnRateVelocityFactor × |velocity|
             = 10.0 − 0.75 × |velocity|
```

### Gun & Radar

| Constant | Value | Description |
|----------|-------|-------------|
| `MaxGunTurnRate` | 20.0 °/tick | Gun rotation limit per tick |
| `MaxRadarTurnRate` | 45.0 °/tick | Radar rotation limit per tick |

### Tank Dimensions & Energy

| Constant | Value | Description |
|----------|-------|-------------|
| `TankHalfSize` | 9.0 px | Half the tank body side length |
| `StartEnergy` | 100.0 | Energy each tank begins the round with |
| `TankCollisionDamage` | 0.6 | Energy lost by each tank in a tank–tank collision |

### Bullet Physics

| Constant | Value | Description |
|----------|-------|-------------|
| `BulletMinPower` | 0.1 | Minimum firing power |
| `BulletMaxPower` | 3.0 | Maximum firing power |
| `BulletRadius` | 3.0 px | Collision radius of a bullet |

**Derived formulas** (not constants, but derived from power):

```
BulletSpeed     = 20 − 3 × power
BulletDamage    = 4 × power + (power > 1 ? 2 × (power − 1) : 0)
EnergyReturn    = 3 × power
```

### Wall Damage

| Constant | Value | Description |
|----------|-------|-------------|
| `WallDamageFactor` | 0.5 | Multiplier on velocity for wall impact damage |
| `WallDamageThreshold` | 1.0 px/tick | Minimum speed at which wall damage is applied |

```
WallDamage = max(|velocity| × WallDamageFactor − WallDamageThreshold, 0)
```

---

## Building Generation Parameters

These values live in `ArenaEngine.cs` and control the random building layout.

| Parameter | Value | Description |
|-----------|-------|-------------|
| Minimum buildings | 4 | Per round, regardless of arena size |
| Maximum buildings | 14 | Per round, regardless of arena size |
| Scaling | Area-based | More buildings in larger arenas |
| Wall clearance | ~30 px | Minimum gap between building edge and arena wall |
| Inter-building gap | ~20 px | Minimum gap between any two buildings |
| Tank spawn clearance | ~15 px | Buildings kept away from tank spawn points |

---

## Rendering Parameters (`ArenaUserControl.cs`)

| Parameter | Value | Description |
|-----------|-------|-------------|
| `TankBodySize` | 18 px | Rendered side length of the tank square |
| `GunLength` | 22 px | Length of the gun barrel line |
| `RadarLength` | 16 px | Length of the radar beam line |
| `EnergyBarWidth` | 36 px | Width of the energy bar |
| `EnergyBarHeight` | 4 px | Height of the energy bar |
| `RadarHaloLifetime` | 10 ticks | Duration of a radar detection pulse animation |
| `ExplosionDuration` | 2100 ms | Total length of the explosion animation |

---

## Application Defaults (`TankSwarmArena.cs`)

| Parameter | Default | Description |
|-----------|---------|-------------|
| Default tick rate | 10 ticks/sec | Simulation speed on startup |
| Default arena width | 800 px | Initial arena width |
| Default arena height | 600 px | Initial arena height |
| Radio log max lines | 300 lines | Auto-trims to 250 when exceeded |

---

## Modifying Constants

`ArenaConstants.cs` is in the `TankSwarmCode.SwarmTank.Interfaces` project, which all other projects reference. Changing a value here affects the entire simulation immediately — no other files need updating.

**When tuning physics**, keep these relationships in mind:

- Increasing `MaxVelocity` without increasing `MaxTurnRateBase` makes high-speed turning nearly impossible.
- Increasing `BulletMaxPower` changes the damage ceiling; adjust tank `StartEnergy` proportionally if you want longer rounds.
- Decreasing `MaxRadarTurnRate` below 45°/tick increases the number of ticks needed to scan the entire arena (360 / rate).
- The building clearance values are embedded in `ArenaEngine` — search for their numeric literals if you need to change them.

---

## Quick Reference Card

```
Movement
  MaxVelocity            = 8.0 px/tick
  Acceleration           = 1.0 px/tick²
  Deceleration           = 2.0 px/tick²
  BodyTurnRate(v)        = 10.0 − 0.75 × |v|   (°/tick)
  GunTurnRate            = 20.0 °/tick
  RadarTurnRate          = 45.0 °/tick

Tank
  HalfSize               = 9.0 px
  StartEnergy            = 100
  CollisionDamage        = 0.6 per tank per collision

Bullet
  Speed(p)               = 20 − 3 × p            (px/tick)
  Damage(p)              = 4p + 2(p−1) if p > 1
  EnergyReturn(p)        = 3p
  PowerRange             = [0.1, 3.0]
  Radius                 = 3.0 px

Wall
  Damage(v)              = max(|v| × 0.5 − 1.0, 0)
```

---

[← Arena Rendering](ch11-rendering.md) | [Table of Contents](TOC.md) | [Next: ECM System →](ch13-ecm-system.md)
