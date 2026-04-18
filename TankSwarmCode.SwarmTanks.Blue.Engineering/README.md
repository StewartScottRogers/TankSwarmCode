# Blue Swarm — Engineering Reference

This shared project documents the design and implementation of the **Blue Swarm AI** for TankSwarmCode — a 2D real-time tank battle simulator with swarm tactics, shared radar, and electronic counter-measures (ECM).

The Blue Swarm's philosophy is **defensive coordination with a clear command hierarchy**. Seven tanks share a unified brain, elect a leader each epoch, and execute synchronized tactical strategies.

---

## Tank Roster

| Tank | Slot | Role | Fire Power | Range | ECM |
|------|------|------|-----------|-------|-----|
| BlueStrike | 0 | Attacker | 3.0 | 250 px | — |
| BlueSharp | 1 | Support | 3.0 | 300 px | — |
| BlueRush | 2 | Attacker | 2.5 | 180 px | — |
| BlueGuard | 3 | Defender | 2.0 | 200 px | — |
| BlueEcm | 4 | EcmSpecialist | 1.5 | — | Jam / Burnthrough |
| BlueTrooper | 5 | Attacker | 2.5 | 220 px | — |

Slot numbers drive formation assignments (encircle angle, pincer group) and leadership election tiebreaking.

---

## Swarm Brain Architecture

All Blue tanks inherit from `SwarmBrainBase` in `TankSwarmCode.SwarmTank`. The brain handles:

### Leadership Election

Every **40 ticks** (one epoch), the tank with the lowest formation slot among living allies becomes the leader. Tiebreaks resolve alphabetically by tank name. The leader selects a strategy and broadcasts `StrategyCommand` to the swarm.

### Shared Radar Picture

Every enemy radar contact is automatically broadcast to allies as a `RadarShare` message. Each tank maintains a `RadarMap` — a merged picture of own scans plus all ally reports. Contacts older than 30 ticks are considered stale.

**Note:** A tank in `Jam` or `JamAndSpoof` mode cannot send or receive any messages, including `RadarShare`.

### Ally Health Tracking

Every 15 ticks each tank broadcasts an `AllyPing` containing its formation slot and current energy. This keeps the swarm's picture of ally health current for strategy decisions.

---

## Tactical Strategies

The leader selects one of six strategies each epoch based on ally energy, enemy count, and ECM state:

| Strategy | Trigger | Behavior |
|----------|---------|---------|
| `Wolfpack` | Default offensive | Converge all tanks on the lowest-energy enemy |
| `Encircle` | Enemy count ≤ 3 | Spread to equal angular intervals around target |
| `Pincer` | Enemy count ≥ 4 | Split into two groups, attack from opposite bearings |
| `ECMScreen` | BlueEcm alive, enemy grouped | BlueEcm jams; remaining tanks rush under cover |
| `Fallback` | Ally energy < 30% | Retreat to the furthest arena corner |
| `Scatter` | Emergency (all energy critical) | Individual flight, radio silence |

Key parameters:
- `OrbitRadius` = 180 px (Encircle formation)
- `VolleyRange` = 300 px (max range for coordinated volleys)
- `LeadershipEpochTicks` = 40
- `AllyStaleTicks` = 30
- `VolleyIntervalTicks` = 30

---

## Coordinated Volley Fire

The leader schedules a `VolleyFire` message specifying a target name and a future tick. All tanks that have the target in their `RadarMap` fire simultaneously at that tick using **linear prediction** — calculating the interception point by projecting the target's current velocity along the bullet's travel time.

---

## ECM

| Mode | Cost / tick | Effect |
|------|------------|--------|
| Off | 0 | No effect |
| Jam | 0.5 | Corrupts enemy scans; disables own radar, firing, radio |
| Spoof | 0.8 | Projects two drifting ghost contacts |
| Burnthrough | 0.3 | Pierces enemy jamming; filters ~70% of ghost contacts |
| JamAndSpoof | 1.3 | Full Jam + Spoof simultaneously |

**Blue's default**: `Burnthrough` (cheap, always-on protection). Switches to `Jam` for tactical suppression in `ECMScreen`. When an ally broadcasts `EcmAlert` (enemy jamming detected), all tanks immediately activate `Burnthrough`.

**BlueEcm** is the swarm's dedicated specialist: it leads ECMScreen pushes and is the only tank assigned `Jam` as a primary mode.

---

## Swarm Communication

| Message Type | Sent By | Purpose |
|-------------|--------|---------|
| `RadarShare` | Automatic | Broadcast every enemy radar contact |
| `Painted` | Automatic | Alert allies when an enemy radar sweeps this tank |
| `AllyPing` | Every 15 ticks | Heartbeat: slot + energy |
| `StrategyCommand` | Leader | Selected strategy + priority target |
| `VolleyFire` | Leader | Synchronized fire at target on a future tick |
| `EcmAlert` | Any tank | Enemy jamming or ghost contacts detected |

---

## Blue vs. Red

| | Blue Swarm | Red Swarm |
|-|-----------|----------|
| **Philosophy** | Defensive coordination, clear hierarchy | Aggressive assault, distributed |
| **Tank count** | 7 | 6 |
| **ECM specialist** | BlueEcm (Burnthrough-first) | RedGhost (Spoof-first) |
| **Engagement range** | Medium-to-long | Close-to-medium |
| **Key tactic** | Synchronized volleys + ECCM | Ghost spoofing + pincer flanking |

---

## Solution Structure

```
TankSwarmCode.SwarmTank.Interfaces   ← Public contracts (ISwarmTank, models, enums, events)
TankSwarmCode.SwarmTank              ← SwarmTankBase + SwarmBrainBase
TankSwarmCode.Arena.Interfaces       ← IArenaContext contract
TankSwarmCode.Arena                  ← Physics engine and runtime
TankSwarmCode.SwarmTanks.Blue        ← Blue Swarm implementation (this project links here)
TankSwarmCode.SwarmTanks.Red         ← Red Swarm implementation
TankSwarmCode.Gui                    ← WinForms renderer and interactive host
TankSwarmCode.Cli                    ← Headless batch runner (JSON/NDJSON/table/CSV)
Documentation/                       ← 15-chapter markdown reference
```

---

## Building and Running

**Prerequisites**: .NET SDK 10.0+, Windows 10+

```bash
# Interactive GUI
dotnet run --project TankSwarmCode.Gui/TankSwarmCode.Gui.csproj

# Headless batch run (100 matches, table output)
dotnet run --project TankSwarmCode.Cli/TankSwarmCode.Cli.csproj -- --batch 100 --format table
```

From Visual Studio: open `TankSwarmCode.slnx`, set startup project to **TankSwarmCode.Gui**, press F5.

---

## Further Reading

| Chapter | Topic |
|---------|-------|
| `Documentation/ch01-overview.md` | Project overview and core concepts |
| `Documentation/ch03-architecture.md` | Dependency graph and runtime data flow |
| `Documentation/ch05-tank-ai-framework.md` | Lifecycle hooks and command API |
| `Documentation/ch06-swarm-communication.md` | Messaging patterns and delivery timing |
| `Documentation/ch09-builtin-tanks.md` | Blue and Red swarm strategy details |
| `Documentation/ch13-ecm-system.md` | ECM modes, costs, and countermeasures |
| `Documentation/ch15-cli.md` | Headless runner reference |
