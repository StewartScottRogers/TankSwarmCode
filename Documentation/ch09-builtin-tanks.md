# Chapter 9: Built-in Tank AI Examples

[← Data Models](ch08-data-models.md) | [Table of Contents](TOC.md) | [Next: Building Your Own Tank →](ch10-custom-tank.md)

---

Two fully implemented swarms ship with the project. They demonstrate different cooperative strategies and serve as reference implementations when writing a custom AI.

---

## Red Swarm (SwarmId = 1) — Aggressive Assault

The Red Swarm is built around a fast-moving scout that floods the team with radar data, two attacker pairs that close in from different angles, and an ECM jammer that corrupts the enemy's picture.

**Roster (6 tanks total)**

| Tank | File | Role |
|------|------|------|
| RedScout | `RedScout.cs` | Scout |
| RedAlpha, RedBravo | `RedAttacker.cs` (×2) | Attacker |
| RedWolf, RedFox | `RedFlank.cs` (×2) | Attacker |
| ECM-Jammer | `RedEcmJammer.cs` | EcmSpecialist |

---

### RedScout

**Strategy**:
- Spins the radar continuously at maximum rate (45°/tick), relying on automatic `RadarShare` to keep all allies informed.
- Moves in an S-curve pattern — oscillates turn direction every 35 ticks — to avoid becoming an easy static target.
- Fires **low-power harassing shots** (0.5 power); the scout's job is intelligence, not kill shots.
- On bullet hit: perpendicular jink (±90° turn) + retreat 80 px.
- On wall hit: reverses oscillation direction and backs off 40 px.

**Swarm contribution**: By keeping the radar sweeping at all times, `RedScout` ensures the attacker tanks always have a fresh enemy picture even when their own radars are locked onto a specific target.

---

### RedAttacker (RedAlpha & RedBravo)

**Strategy**:
- Selects the freshest enemy contact from `RadarMap` (via `GetFreshestEnemy()`).
- Uses **linear prediction** to lead moving targets: projects the target's current `VelocityVector` by the bullet travel time to estimate where it will be.
- Fires at **2.5 power** — high damage, slower bullet — only when gun alignment is within 5°.
- Closes when target > 200 px away, backs off if < 100 px, holds position in between.
- Broadcasts `RequestBackup` when energy drops below 25.
- Retreats toward arena centre and keeps radar spinning when energy < 25; resumes attacking when energy recovers above 35.

**Key technique — linear prediction**:

```
travelTime       = distance / bulletSpeed
predictedPos     = target.Position + target.VelocityVector × travelTime
absoluteBearing  = Atan2(predictedPos.X - myX, predictedPos.Y - myY)
gunTurn          = NormalizeTo180(absoluteBearing - State.GunHeading)
fire if |gunTurn| < 5°
```

---

### RedFlank (RedWolf & RedFox)

**Strategy**:
- Uses the same target-acquisition logic as `RedAttacker` but approaches from a fixed **bearing offset** (±90°) relative to the attacker–target axis.
- `RedWolf` circles to the target's right; `RedFox` circles to the left, creating a **pincer manoeuvre**.
- Fires at **2.0 power** when aligned within 15° and within 300 px.
- Orbit distance: ~180 px from the target.

The flanking offset is computed by projecting the desired position perpendicular to the attacker–target line, then steering toward that position while maintaining radar lock.

---

### RedEcmJammer

**Role**: EcmSpecialist — **no cannon**

**Strategy**:
- Default mode: **Spoof** — projects two ghost-tank echoes that drift around the arena, polluting enemy `RadarMap` entries with phantom contacts.
- Threat response: switches to **Jam** for 25 ticks when hit by a bullet, making it very difficult for the shooter to re-acquire the jammer. Reverts to Spoof automatically.
- Orbits the arena centre at ~22 % of the arena's smaller dimension, placing ghosts in the contested mid-arena space.
- Broadcasts `EcmAlert` when it takes fire.
- Broadcasts `EnemySpotted` for every radar contact so the swarm retains a common picture even though the jammer never fires.

**Key interaction**: If the enemy swarm does not include a Burnthrough-capable tank, `RedEcmJammer`'s ghosts contaminate the entire enemy `RadarMap` indefinitely. `BlueEcmOperator` is the intended counter.

---

## Blue Swarm (SwarmId = 2) — Defensive Coordination

The Blue Swarm has a clear command hierarchy: `BlueCommander` designates targets and issues positional orders, while specialist tanks execute their roles based on received messages.

**Roster (7 tanks total)**

| Tank | File | Role |
|------|------|------|
| BlueCommand | `BlueCommander.cs` | Attacker |
| BluePatrol × 2 | `BluePatrol.cs` (×2) | Defender |
| BlueSniper × 2 | `BlueSniper.cs` (×2) | Support |
| BlueWarden | `BlueWarden.cs` | Defender |
| ECM-Operator | `BlueEcmOperator.cs` | EcmSpecialist |

---

### BlueCommander

**Strategy**:
- Holds a position near the arena centre (within 70 px).
- Scans `RadarMap` each tick and selects the **lowest-energy enemy** as the priority target (score = energy + distance × 0.1, lower is better).
- Broadcasts `TargetLocked` every 20 ticks with the chosen target name, causing the whole swarm to refocus.
- Broadcasts `FormationMove` with a rally position when a `RequestBackup` message is received.
- Fires adaptively: **3.0 power** when the target is within 150 px, **2.0 power** otherwise.
- Retreats toward a safe corner when energy < 28; resumes attacking when energy recovers above 55.

**Command flow**:

```
BlueCommander scans RadarMap → selects lowest-energy enemy
    → broadcasts TargetLocked("RedAlpha")

BluePatrol receives TargetLocked
    → updates _priorityTargetName; steers toward last-known position

BlueSniper receives TargetLocked
    → only fires if TargetLocked is set; otherwise holds corner position

BlueWarden receives TargetLocked
    → adjusts oscillation centre toward the designated target
```

---

### BluePatrol (two instances)

**Strategy**:
- Patrols a 4-point rectangle in its assigned arena half (left or right) when no commander order is active.
- On `TargetLocked`: abandons patrol and steers toward the designated enemy.
- On `FormationMove`: moves to the specified rally position for 60 ticks.
- Fires **2.5 power** at close range (< 150 px) and **1.5 power** at longer range.
- Retreats when energy < 20; resumes when energy > 30.

---

### BlueSniper (two instances)

**Strategy**:
- Camps in a designated corner (90 px from walls) and holds position.
- Only responds to `TargetLocked` orders from `BlueCommander` — ignores general radar contacts.
- Fires at **3.0 power** with tight alignment tolerance (5°).
- Performs **inbound bullet evasion**: scans `Arena.GetActiveBullets()` each tick; if a bullet is heading toward this tank (dot product check) and within 200 px, sidesteps perpendicular.
- Relocates to the opposite corner if an enemy closes within 80 px.

**Bullet evasion logic**:

```
For each bullet in Arena.GetActiveBullets():
    project bullet velocity vector toward this tank's position
    if dot product > 0 AND distance < 200 px:
        SetTurnRight(90)
        SetAhead(40)
```

---

### BlueWarden

**Strategy**:
- Anchors near the arena centre and oscillates back and forth every 25 ticks (±35 px) to be an unpredictable target.
- Performs a full radar sweep to ensure the team always has central-area coverage.
- Broadcasts `EnemySpotted` for every new radar contact.
- Fires **adaptively**: 3.0 power when target < 150 px, 2.0 power < 300 px, 1.0 power beyond.
- Responds to `TargetLocked` and `FormationMove` orders from `BlueCommander` — adjusts oscillation centre and rallies for up to 60 ticks.

---

### BlueEcmOperator

**Role**: EcmSpecialist — **light cannon (max 2.0 power)**

**Strategy**:
- Default mode: **Burnthrough** — protects the Blue swarm's radar picture from enemy Spoof fields.
- Inspects every `OnScannedTank` contact name. If it starts with `"Ghost-"`, broadcasts `EcmAlert` and forces Burnthrough for 40 ticks — ensuring it sees through the active Spoof field.
- **Responds to ally `EcmAlert` messages**: immediately activates Burnthrough, coordinating ECCM across the swarm.
- **Offensive Jam window**: when energy > 70 and an enemy ECM tank has been confirmed, switches to Jam for 20 ticks to disrupt the enemy's Spoof pipeline.
- Falls back to `Off` when energy < 25 to preserve survival.
- Patrols a figure-8 path to maintain arena coverage.
- Fires at confirmed enemy contacts (non-Ghost) when gun is within 6° and energy > 40.

**Key interaction**: `BlueEcmOperator` directly counters `RedEcmJammer`. Its Burnthrough filters ~70 % of ghosts per sweep, and its `EcmAlert` broadcasts can trigger Burnthrough across the whole Blue swarm.

---

## Swarm Comparison

| | Red Swarm | Blue Swarm |
|---|-----------|------------|
| **Philosophy** | Aggressive, fast, offensive + EM deception | Coordinated, defensive, ECCM-protected |
| **Radar coverage** | Scout-driven; ECM ghost injection pollutes enemy picture | Commander-driven; Operator Burnthrough clears own picture |
| **Engagement range** | Close-to-medium | Medium-to-long |
| **Coordination type** | Loose (auto RadarShare, backup requests, EcmAlert) | Tight (TargetLocked, FormationMove, EcmAlert response) |
| **Special tactics** | Flanking pincer, linear prediction, ghost spoofing | Corner camping, bullet evasion, priority targeting, ECCM |
| **ECM capability** | Jammer: Spoof by default → Jam on threat | Operator: Burnthrough by default → Jam offensive |
| **Tank count** | 6 (incl. ECM-Jammer) | 7 (incl. ECM-Operator) |

---

[← Data Models](ch08-data-models.md) | [Table of Contents](TOC.md) | [Next: Building Your Own Tank →](ch10-custom-tank.md)
