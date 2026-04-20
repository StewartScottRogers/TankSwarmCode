# Chapter 9: Built-in Tank AI Examples

[← Data Models](ch08-data-models.md) | [Table of Contents](TOC.md) | [Next: Building Your Own Tank →](ch10-custom-tank.md)

---

Two fully implemented swarms ship with the project. They serve as reference implementations and as balanced opponents for testing custom AI.

---

## Architecture: SwarmTankCortexCradleBase

All built-in tanks inherit from `SwarmTankCortexCradleBase` (in `TankSwarmCode.SwarmTank/SwarmTankCortexCradleBase.cs`), not directly from `SwarmTankBase`. `SwarmTankCortexCradleBase` is a concrete AI layer that sits on top of `SwarmTankBase` and provides a full team-coordination brain. Individual tanks configure it via a `TankConfiguration` record:

```csharp
protected internal abstract TankConfiguration TankConfig { get; }
```

### TankConfiguration Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `FormationSlot` | `int` | — | Priority rank within the swarm (0 = highest authority) |
| `MaxFirePower` | `double` | 2.0 | Maximum bullet power this tank will fire at |
| `PreferredRange` | `double` | 200.0 | Engagement distance (px) for navigation |
| `HasEcm` | `bool` | false | Whether this tank activates offensive ECM |
| `OffensiveEcmMode` | `EcmMode` | `Jam` | ECM mode used when the strategy calls for ECM |
| `RetreatEnergyThreshold` | `double` | 25.0 | Energy level at which this tank triggers the Scatter strategy |

---

### Leader Election

Every 40 ticks (`LeadershipEpochTicks`), the living tank with the **lowest `FormationSlot`** acts as leader for the epoch. Ties are broken alphabetically by name. The leader:

1. Picks the **priority target** — the lowest-energy enemy in `RadarMap`.
2. Selects a **strategy** (see below) and broadcasts a `StrategyCommand` message to all allies.
3. Schedules a **coordinated volley** (`VolleyFire` message) for Wolfpack and Pincer strategies when two or more allies are within 300 px of the target.

Non-leader tanks receive and apply these messages immediately. If the leader dies, the tank with the next-lowest slot takes over at the next epoch boundary.

---

### Epoch Strategies

| Strategy | When selected | Behaviour |
|----------|--------------|-----------|
| `Wolfpack` | Default; no enemies visible; no overriding condition | Converge on priority target; linear-prediction fire |
| `Encircle` | Allies ≥ 2× enemies AND ≥ 3 allies alive | Spread at equal angles around the target at 180 px orbit |
| `Pincer` | ≥ 3 allies, ≤ 2 enemies | Split into two approach groups (slots 0–1 from one side, 2+ from the other) |
| `ECMScreen` | Recent `EcmAlert` broadcast received | ECM tank jams; non-ECM tanks rush under cover at 130 px |
| `Fallback` | Average ally energy < 30 | Retreat to furthest arena corner; no firing |
| `Scatter` | Solo tank, energy < retreat threshold | Flee away from nearest known enemy; radio silence |

---

### Ally Ping Protocol

Every 15 ticks each tank broadcasts an `AllyPing` message containing its formation slot and current energy. The brain uses these pings to:

- Track which allies are still alive (entries older than 30 ticks are purged).
- Calculate the alive-ally count for strategy selection.
- Compute average ally energy for Fallback threshold.

---

### Wall-Aware Firing

`SwarmTankCortexCradleBase` never fires through a wall it knows about. Before every shot — both in `LinearPredictionFire` and in scheduled volley fire — the brain checks every `WallSegment` in `BuildingWallMap` against the line from the tank to the target (or predicted position). If any known wall segment intersects that line, the shot is suppressed for that tick. The gun continues tracking the target so the shot fires as soon as the line of fire is clear.

Walls accumulate in `BuildingWallMap` from both direct radar echoes and `BuildingEchoShare` messages relayed by allies. A tank that has never scanned a particular building face will not know to avoid shooting through it.

---

### ECM Handling

- **ECM-specialist tanks** (`HasEcm = true`): activate `OffensiveEcmMode` during `ECMScreen` strategy; switch to `Burnthrough` when an `EcmAlert` has been received within the last 20 ticks.
- **Non-specialist tanks**: activate `Burnthrough` when an `EcmAlert` has been received within the last 20 ticks; otherwise `Off`.

---

## Red Swarm (SwarmId = 1) — Aggressive Assault

**Default roster (4 tanks)**

| Tank | File | Slot | Role | Max Power | Range | ECM |
|------|------|------|------|-----------|-------|-----|
| RedHammer | `RedHammer.cs` | 0 | Attacker | 3.0 | 200 px | — |
| RedBlade | `RedBlade.cs` | 1 | Attacker | 2.5 | 160 px | — |
| RedArrow | `RedArrow.cs` | 2 | Scout | 1.5 | 220 px | — |
| RedGhost | `RedGhost.cs` | 3 | EcmSpecialist | 0.1 | 150 px | JamAndSpoof |

For NvN matches larger than 4 per side, additional `RedTrooper` instances fill slots 4, 5, …

---

### RedHammer (Slot 0 — Leader)

```csharp
new TankConfiguration { FormationSlot = 0, MaxFirePower = 3.0, PreferredRange = 200.0, HasEcm = false, RetreatEnergyThreshold = 25.0 }
```

RedHammer is the Red swarm's default leader. It fires maximum-power bullets (3.0) and engages at medium range (200 px). Because it holds slot 0, it issues `StrategyCommand` and `VolleyFire` orders whenever it is alive. When it falls, RedBlade (slot 1) takes over leadership.

**Key characteristics:**
- Highest damage output of any Red tank
- Default Wolfpack strategy converges the whole swarm on a single target
- Schedules coordinated volleys against priority targets within 300 px
- Retreats to Scatter when alone and below 25 energy

---

### RedBlade (Slot 1 — Aggressive Flanker)

```csharp
new TankConfiguration { FormationSlot = 1, MaxFirePower = 2.5, PreferredRange = 160.0, HasEcm = false, RetreatEnergyThreshold = 20.0 }
```

RedBlade closes to tighter range than RedHammer (160 px vs 200 px) and fires at 2.5 power — a good balance of speed and damage. It acts as both the secondary attacker and the fallback leader. Its closer preferred range makes it effective in cramped arenas and building-heavy maps.

---

### RedArrow (Slot 2 — Scout)

```csharp
new TankConfiguration { FormationSlot = 2, MaxFirePower = 1.5, PreferredRange = 220.0, HasEcm = false, RetreatEnergyThreshold = 20.0 }
```

RedArrow's `Role` is `Scout`. Its lighter fire power (1.5) is intentional — faster bullets are harder to dodge at long range (220 px). `SwarmTankCortexCradleBase`'s `MaintainRadar` continuously re-locks the radar on the priority target, giving the whole swarm frequent fresh contacts. RedArrow also contributes to `VolleyFire` schedules when in range.

---

### RedGhost (Slot 3 — ECM Specialist)

```csharp
new TankConfiguration { FormationSlot = 3, MaxFirePower = 0.1, PreferredRange = 150.0, HasEcm = true, OffensiveEcmMode = EcmMode.JamAndSpoof, RetreatEnergyThreshold = 40.0 }
```

RedGhost is an electronic warfare platform. It almost never fires (max power 0.1) and instead relies on `JamAndSpoof` to suppress the enemy:

- **Jam** — each tick RedGhost's jamming drops 50% of enemy radar sweeps that would otherwise hit a Red tank and corrupts a further 30%.
- **Spoof** — simultaneously projects two drifting ghost contacts into enemy `RadarMap` entries.
- **Combined effect** — during `ECMScreen` strategy, the entire enemy swarm fights through a degraded sensor picture while Red tanks rush at 130 px range under cover.

RedGhost has a high retreat threshold (40 energy) because once below that level its ECM costs (1.3/tick for JamAndSpoof) would accelerate its destruction. It retreats early to preserve its jamming capacity. Outside of `ECMScreen`, it falls back to `Burnthrough` if an `EcmAlert` is received, or `Off` otherwise.

---

### RedTrooper (Dynamic — NvN Filler)

`RedTrooper(int slot)` takes a runtime slot index. Its name is `Red{slot}` (e.g. `Red4`, `Red5`). All troopers are Attacker role with 2.5 power and 200 px range — identical generic fighters that fill out larger NvN rosters. Because their slot numbers are higher than all named tanks, they are never elected leader unless all named Red tanks are dead.

---

## Blue Swarm (SwarmId = 2) — Coordinated Pressure

**Default roster (5 tanks)**

| Tank | File | Slot | Role | Max Power | Range | ECM |
|------|------|------|------|-----------|-------|-----|
| BlueStrike | `BlueStrike.cs` | 0 | Attacker | 3.0 | 250 px | — |
| BlueSharp | `BlueSharp.cs` | 1 | Support | 3.0 | 300 px | — |
| BlueRush | `BlueRush.cs` | 2 | Attacker | 2.5 | 180 px | — |
| BlueGuard | `BlueGuard.cs` | 3 | Defender | 2.0 | 200 px | — |
| BlueEcm | `BlueEcm.cs` | 4 | EcmSpecialist | 1.5 | 150 px | Jam |

For NvN matches larger than 5 per side, additional `BlueTrooper` instances fill slots 5, 6, …

---

### BlueStrike (Slot 0 — Leader)

```csharp
new TankConfiguration { FormationSlot = 0, MaxFirePower = 3.0, PreferredRange = 250.0, HasEcm = false, RetreatEnergyThreshold = 25.0 }
```

BlueStrike is the Blue swarm's default leader. It fires at maximum power from longer range (250 px) than RedHammer, favouring a stand-off engagement style. It issues `StrategyCommand` and `VolleyFire` orders each epoch. When BlueStrike falls, BlueSharp (slot 1) takes over.

---

### BlueSharp (Slot 1 — Long-Range Support)

```csharp
new TankConfiguration { FormationSlot = 1, MaxFirePower = 3.0, PreferredRange = 300.0, HasEcm = false, RetreatEnergyThreshold = 20.0 }
```

BlueSharp holds the longest preferred range in either swarm (300 px). Its maximum fire power of 3.0 and wide engagement distance mean it keeps firing while other tanks close in. Linear prediction ensures the slow heavy bullets land despite the range. It is Blue's fallback leader after BlueStrike.

---

### BlueRush (Slot 2 — Close-Quarters Attacker)

```csharp
new TankConfiguration { FormationSlot = 2, MaxFirePower = 2.5, PreferredRange = 180.0, HasEcm = false, RetreatEnergyThreshold = 20.0 }
```

BlueRush is the Blue swarm's fast-closing attacker. Its tight preferred range (180 px) means the brain's `NavigateTo` drives it aggressively toward the target. During Encircle strategy, BlueRush occupies one of the orbit positions closest to the target, sustaining pressure from close range.

---

### BlueGuard (Slot 3 — Defender)

```csharp
new TankConfiguration { FormationSlot = 3, MaxFirePower = 2.0, PreferredRange = 200.0, HasEcm = false, RetreatEnergyThreshold = 30.0 }
```

BlueGuard's `Role` is `Defender`. It has a higher retreat threshold (30 energy) than the other non-specialist Blue tanks, retreating earlier to stay alive longer. Its moderate power (2.0) and 200 px range make it a reliable all-around fighter. During Fallback it retreats early, keeping the swarm's average energy up.

---

### BlueEcm (Slot 4 — ECM Specialist)

```csharp
new TankConfiguration { FormationSlot = 4, MaxFirePower = 1.5, PreferredRange = 150.0, HasEcm = true, OffensiveEcmMode = EcmMode.Jam, RetreatEnergyThreshold = 35.0 }
```

BlueEcm's offensive mode is `Jam` (not `JamAndSpoof`), so it fully suppresses enemy radar without injecting fake contacts. This makes it a reliable ECCM platform: when an `EcmAlert` is received, the brain switches it to `Burnthrough`, clearing enemy ghost contacts from the Blue swarm's `RadarMap`. Its light cannon (1.5 power) lets it contribute damage when jamming is not active. The high retreat threshold (35 energy) ensures its jamming capacity is preserved deep into a round.

**Key interaction with RedGhost**: BlueEcm's `Burnthrough` filters ~70% of RedGhost's ghost contacts per sweep. If BlueEcm is killed early, the Blue swarm fights through a badly degraded radar picture for the rest of the round.

---

### BlueTrooper (Dynamic — NvN Filler)

`BlueTrooper(int slot)` mirrors `RedTrooper` for the Blue side. Name is `Blue{slot}`. All troopers are Attacker role with 2.5 power and 200 px range.

---

## Swarm Comparison

| | Red Swarm | Blue Swarm |
|---|-----------|------------|
| **Default size** | 4 tanks | 5 tanks |
| **Philosophy** | Aggressive pack attack, heavy front-loaded damage | Coordinated pressure, layered range coverage |
| **Engagement range** | 150–220 px | 150–300 px |
| **Leader** | RedHammer (slot 0) | BlueStrike (slot 0) |
| **ECM tank** | RedGhost: JamAndSpoof — maximum deception | BlueEcm: Jam — maximum suppression; Burnthrough as ECCM |
| **ECM strategy** | ECMScreen: RedGhost jams + spoofs while others rush | ECMScreen: BlueEcm jams while others close; switches to Burnthrough to counter Red spoofing |
| **Strengths** | Overwhelming burst damage in Wolfpack volleys; ghost spoofing degrades enemy targeting | Long-range accuracy; ECM/ECCM flexibility; larger default roster |
| **Weaknesses** | RedGhost is a high-value target; poor radar if Ghost dies | BlueSharp at 300 px is slow to close when outnumbered; ECM advantage lost if BlueEcm dies early |

---

[← Data Models](ch08-data-models.md) | [Table of Contents](TOC.md) | [Next: Building Your Own Tank →](ch10-custom-tank.md)
