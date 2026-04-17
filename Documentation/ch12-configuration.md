# Chapter 12: Configuration & Constants

[← Arena Rendering](ch11-rendering.md) | [Table of Contents](TOC.md) | [Next: ECM System →](ch13-ecm-system.md)

---

## Physics Constants (`ArenaConstants.cs`)

All physics values are defined in `TankSwarmCode.SwarmTank.Interfaces/ArenaConstants.cs`. They apply to every tank in every round. Changing a value here affects the entire simulation — all projects reference this file.

### Movement

| Constant | Value | Description |
|----------|-------|-------------|
| `MaxVelocity` | 8.0 px/tick | Maximum forward/reverse speed |
| `Acceleration` | 1.0 px/tick² | Rate of speed increase |
| `Deceleration` | 2.0 px/tick² | Rate of speed decrease (braking or reversing) |
| `MaxTurnRate` | 10.0 °/tick | Body turn rate at zero velocity |
| `VelocityTurnPenalty` | 0.75 °/tick per px/tick | Reduction in turn rate per unit of speed |

Effective body turn rate at any given velocity:

```
BodyTurnRate = MaxTurnRate − VelocityTurnPenalty × |velocity|
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
| `TankHalfSize` | 9.0 px | Half the tank body side length (collision radius) |
| `TankStartEnergy` | 100.0 | Energy each tank begins the round with |
| `TankCollisionDamage` | 0.6 | Energy lost by each tank in a tank–tank collision |

### Bullet Physics

| Constant | Value | Description |
|----------|-------|-------------|
| `BulletMinPower` | 0.1 | Minimum firing power |
| `BulletMaxPower` | 3.0 | Maximum firing power |
| `BulletRadius` | 3.0 px | Collision radius of a bullet |

**Derived formulas** (calculated from power, not stored as constants):

```
BulletSpeed   = 20 − 3 × power
BulletDamage  = 4 × power + (power > 1 ? 2 × (power − 1) : 0)
EnergyReturn  = 3 × power
```

| Power | Speed | Damage | Return |
|-------|-------|--------|--------|
| 0.1 | 19.7 | 0.4 | 0.3 |
| 1.0 | 17.0 | 4.0 | 3.0 |
| 2.0 | 14.0 | 10.0 | 6.0 |
| 3.0 | 11.0 | 16.0 | 9.0 |

### Wall Damage

| Constant | Value | Description |
|----------|-------|-------------|
| `WallDamageFactor` | 0.5 | Multiplier on velocity for wall impact damage |
| `WallDamageThreshold` | 1.0 px/tick | Minimum speed at which wall damage is applied |

```
WallDamage = max(|velocity| × WallDamageFactor − WallDamageThreshold, 0)
```

### ECM Constants

| Constant | Value | Description |
|----------|-------|-------------|
| `EcmJamCostPerTick` | 0.5 | Energy drained per tick in Jam mode |
| `EcmSpoofCostPerTick` | 0.8 | Energy drained per tick in Spoof mode |
| `EcmBurnthroughCostPerTick` | 0.3 | Energy drained per tick in Burnthrough mode |
| `EcmJamDropChance` | 0.50 | Jam scan-drop probability (no Burnthrough counter) |
| `EcmJamCorruptChance` | 0.30 | Jam scan-corrupt probability (no Burnthrough counter) |
| `EcmBurnthroughDropChance` | 0.08 | Jam scan-drop with Burnthrough active |
| `EcmBurnthroughCorruptChance` | 0.08 | Jam scan-corrupt with Burnthrough active |
| `EcmBurnthroughGhostFilterChance` | 0.70 | Ghost discard probability with Burnthrough |
| `EcmSpoofRadius` | 130.0 px | Maximum initial ghost offset from the spoofing tank |
| `EcmSpoofGhostCount` | 2 | Number of ghost contacts per Spoof/JamAndSpoof tank |

Note: `JamAndSpoof` costs are the sum of Jam + Spoof: 0.5 + 0.8 = **1.3 energy/tick**.

---

## Building Generation Parameters

These values live inside `ArenaEngine.cs` and control the random building layout each round.

| Parameter | Value | Description |
|-----------|-------|-------------|
| Minimum buildings | 4 | Per round, regardless of arena size |
| Maximum buildings | 14 | Per round, regardless of arena size |
| Scaling | Area-based | More buildings in larger arenas |
| Wall clearance | ~30 px | Minimum gap between building edge and arena boundary |
| Inter-building gap | ~20 px | Minimum gap between any two buildings |
| Tank spawn clearance | ~15 px | Buildings kept away from initial tank spawn points |

---

## Rendering Parameters (`ArenaUserControl.cs`)

| Parameter | Value | Description |
|-----------|-------|-------------|
| `TankBodySize` | 18 px | Rendered side length of the tank square |
| `GunLength` | 22 px | Length of the gun barrel |
| `BarrelW` | 4 px | Width of the gun barrel |
| `TurretR` | 5 px | Turret circle radius |
| `RadarLength` | 16 px | Length of the radar beam line |
| `EnergyBarWidth` | 36 px | Width of the energy bar |
| `EnergyBarHeight` | 4 px | Height of the energy bar |
| `RadarTrailLength` | 12 frames | Radar heading history for phosphor-decay trail |
| `ScanHaloLifetime` | 10 ticks | Duration of a radar detection pulse animation |
| `ExplosionDurationMs` | 2100 ms | Total length of the explosion animation |

---

## Application Defaults (`TankSwarmArena.cs`)

| Parameter | Default | Description |
|-----------|---------|-------------|
| Default tick rate | 20 ticks/sec | Simulation speed on startup |
| Default arena width | 800 px | Initial arena width |
| Default arena height | 600 px | Initial arena height |
| Radio log max lines | 300 lines | Auto-trims to 250 when exceeded |

---

## Quick Reference Card

```
Movement
  MaxVelocity         = 8.0 px/tick
  Acceleration        = 1.0 px/tick²
  Deceleration        = 2.0 px/tick²
  BodyTurnRate(v)     = 10.0 − 0.75 × |v|   (°/tick)
  GunTurnRate         = 20.0 °/tick
  RadarTurnRate       = 45.0 °/tick

Tank
  HalfSize            = 9.0 px
  StartEnergy         = 100
  CollisionDamage     = 0.6 per tank per collision

Bullet
  Speed(p)            = 20 − 3 × p            (px/tick)
  Damage(p)           = 4p + 2(p−1) if p > 1  (else 4p)
  EnergyReturn(p)     = 3p
  PowerRange          = [0.1, 3.0]
  Radius              = 3.0 px

Wall
  Damage(v)           = max(|v| × 0.5 − 1.0, 0)

ECM
  Jam cost            = 0.5 /tick
  Spoof cost          = 0.8 /tick
  JamAndSpoof cost    = 1.3 /tick (0.5 + 0.8)
  Burnthrough cost    = 0.3 /tick
  Jam drop chance     = 50% (8% with Burnthrough)
  Jam corrupt chance  = 30% (8% with Burnthrough)
  Ghost filter        = 70% (Burnthrough)
```

---

## Modifying Constants

`ArenaConstants.cs` is in the `TankSwarmCode.SwarmTank.Interfaces` project, which all other projects reference. Changing a value here affects the entire simulation immediately.

**When tuning physics**, keep these relationships in mind:

- Increasing `MaxVelocity` without adjusting `MaxTurnRate` makes high-speed turning nearly impossible (it bottoms out at `MaxTurnRate − VelocityTurnPenalty × MaxVelocity = 10 − 6 = 4°/tick`).
- Increasing `BulletMaxPower` changes the damage ceiling; consider adjusting `TankStartEnergy` proportionally for similar round lengths.
- Decreasing `MaxRadarTurnRate` below 45°/tick increases the number of ticks to scan the full arena (360 / rate).
- Building generation parameters are embedded as numeric literals in `ArenaEngine` — search for them if you need to change building density.

---

[← Arena Rendering](ch11-rendering.md) | [Table of Contents](TOC.md) | [Next: ECM System →](ch13-ecm-system.md)
