# Chapter 0: Quick Reference

[Table of Contents](TOC.md) | [Next: Overview →](ch01-overview.md)

One-page reference for experienced users. For explanations see the linked chapters.

---

## CLI Flags  →  [ch15](ch15-cli.md)

### Match options
| Flag | Default | Description |
|------|---------|-------------|
| `--bot1 <dll>` | required | Path to Swarm 1 DLL |
| `--bot2 <dll>` | required | Path to Swarm 2 DLL |
| `--seed <int>` | random | RNG seed; incremented by 1 per match in a batch |
| `--max-ticks <int>` | 5000 | Ticks before match is timed-out |
| `--batch <N>` | 1 | Number of matches to run |
| `--parallel <N>` | 1 | Max concurrent matches |
| `--on-timeout <rule>` | `draw` | `draw` / `energy` / `survivors` |
| `--width <double>` | 800 | Arena width (px) |
| `--height <double>` | 600 | Arena height (px) |
| `--format <fmt>` | `json` | `json` / `ndjson` / `table` / `csv` |
| `--summary` | off | Append aggregate stats (JSON mode; always on in table mode) |
| `--list <dll>` | — | List all `ISwarmTank` classes in a DLL |

### Research standard command
```bash
dotnet run --project TankSwarmCode.Cli/TankSwarmCode.Cli.csproj -- \
  --bot1 TankSwarmCode.SwarmTanks.Red/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Red.dll \
  --bot2 TankSwarmCode.SwarmTanks.Blue/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Blue.dll \
  --batch 200 --parallel 8 --seed 1000 --on-timeout energy --format table
```

---

## Insight Labels  →  [ch15](ch15-cli.md)

| Label | Condition |
|-------|-----------|
| ECM | `role == EcmSpecialist` |
| MVP / Co-MVP | survived in the most (or near-most) decisive wins |
| Solo carry | soloWins ≥ 3 AND ≥ 20% of swarm wins |
| All-in | survives only in wins (never survives a loss), ≥ 5 survivals |
| Survivor | survives ≥ 5 defeats |
| Hider | non-ECM, combat rate < 3.0/100t |
| Top attacker | highest energy-gain rate among non-ECM tanks |
| Glass cannon | top attacker with win-survival rate still below 33% |
| Expendable | present in wins but survives < 33% of them; not top attacker |
| Fragile | survival rate < 10% across ≥ 20 matches |
| Linchpin | swarm win rate drops > 55 pp when this tank is dead |
| Primary target | first-killed > 1.5× swarm average, ≥ 5 incidents |
| Balanced | ≥3 co-carriers, leader <50% of swarm wins |

---

## ECM Modes  →  [ch13](ch13-ecm-system.md)

| Mode | Cost/tick | Disables own radar/fire/radio | Effect on enemy |
|------|----------|-------------------------------|-----------------|
| `Off` | 0 | — | None |
| `Jam` | 0.5 | Yes | 50% scan corruption, radio blackout |
| `Spoof` | 0.8 | No | Injects 2 drifting ghost contacts |
| `JamAndSpoof` | 1.3 | Yes | Both effects simultaneously |
| `Burnthrough` | 0.3 | No | Reduces incoming corruption to 8%; filters ~70% of ghosts |

---

## Tank AI Command API  →  [ch05](ch05-tank-ai-framework.md)

| Method | Description |
|--------|-------------|
| `ctx.SetAhead(dist)` | Queue forward movement |
| `ctx.SetBack(dist)` | Queue backward movement |
| `ctx.SetTurnRight(deg)` | Queue body turn right |
| `ctx.SetTurnLeft(deg)` | Queue body turn left |
| `ctx.SetTurnGunRight(deg)` | Queue gun turn right |
| `ctx.SetTurnGunLeft(deg)` | Queue gun turn left |
| `ctx.SetTurnRadarRight(deg)` | Queue radar turn right |
| `ctx.SetTurnRadarLeft(deg)` | Queue radar turn left |
| `ctx.SetFire(power)` | Fire bullet (power 0.1–3.0) |
| `ctx.SetEcm(EcmMode)` | Set ECM mode for this tick |
| `ctx.Broadcast(SwarmMessage)` | Send message to all allies |

**Key state reads:** `ctx.State.Position`, `.Heading`, `.GunHeading`, `.RadarHeading`, `.Energy`, `.Velocity`  
**Arena reads:** `ctx.Arena.ArenaWidth`, `.ArenaHeight`, `.TickNumber`  
**Radar:** `ctx.RadarMap` — discard contacts where `TickNumber - contact.Timestamp > 30`

---

## TankConfiguration Fields  →  [ch09](ch09-builtin-tanks.md)

| Field | Type | Description |
|-------|------|-------------|
| `FormationSlot` | `int` | Leadership priority (0 = highest) |
| `MaxFirePower` | `double` | Maximum bullet power (0.1–3.0) |
| `PreferredRange` | `double` | Engagement distance in pixels |
| `HasEcm` | `bool` | Whether to use offensive ECM |
| `OffensiveEcmMode` | `EcmMode` | ECM mode for offensive use |
| `RetreatEnergyThreshold` | `double` | Energy level to trigger Scatter |

---

## Swarm Message Types  →  [ch06](ch06-swarm-communication.md)

| Type | Sent by | Purpose |
|------|---------|---------|
| `AllyPing` | Every 15 ticks | Heartbeat: slot + energy |
| `StrategyCommand` | Leader | Strategy + priority target |
| `VolleyFire` | Leader | Synchronized fire at future tick |
| `RadarShare` | Automatic | Broadcast every enemy contact |
| `EcmAlert` | Any tank | Enemy ECM detected |
| `Painted` | Automatic | Enemy radar swept this tank |

---

## Key ArenaConstants  →  [ch12](ch12-configuration.md)

| Constant | Value | Description |
|----------|-------|-------------|
| `BulletSpeed` | 15.0 | Pixels per tick |
| `MaxBulletPower` | 3.0 | Upper limit for `SetFire` |
| `EcmJamCostPerTick` | 0.5 | Energy drain for Jam mode |
| `EcmSpoofCostPerTick` | 0.8 | Energy drain for Spoof mode |
| `EcmBurnthroughCostPerTick` | 0.3 | Energy drain for Burnthrough mode |
| `StartingEnergy` | 100.0 | Each tank's starting energy |
| `WallCollisionCost` | varies | Energy lost hitting arena wall |
| `TankCollisionCost` | varies | Energy lost in tank-tank collision |

---

## Architecture at a Glance  →  [ch03](ch03-architecture.md)

```
ISwarmTank (shell)          — frozen; delegates to cortex via CortexFactory
  └── SwarmTankBase         — provides command API, RadarMap, Broadcast
        implements ITankContext passed to cortex

IAiCortex (cortex)          — all AI logic lives here; one per tank type
  └── RedCortexBase         — default OnTick / OnSwarmMessage for Red
  └── BlueCortexBase        — default OnTick / OnSwarmMessage for Blue
        both call → SwarmCoordinator + TankNavigation

SwarmCoordinator            — one per SwarmId; leader election, strategy, volley
TankNavigation              — stateless movement/firing helpers
```

**To add a new tank:** create a cortex (`RedCortexBase` subclass) + a shell (`SwarmTankBase` subclass + `ITankContext`) + register in `CortexFactory`. See [ch10](ch10-custom-tank.md).
