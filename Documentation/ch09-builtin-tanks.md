# Chapter 9: Built-in Tank AI Examples

[← Data Models](ch08-data-models.md) | [Table of Contents](TOC.md) | [Next: Building Your Own Tank →](ch10-custom-tank.md)

---

Two fully implemented swarms ship with the project. They demonstrate different cooperative strategies and serve as reference implementations when writing a custom AI.

---

## Red Swarm (SwarmId = 1) — Aggressive Assault

The Red Swarm is built around a fast-moving scout that floods the team with radar data, and two types of attacker that close in from different angles.

### RedScout

**File**: `TankSwarmCode.SwarmTanks.Red/RedScout.cs` (~102 lines)  
**Role**: Scout

**Strategy**:
- Spins the radar continuously at maximum rate (45°/tick), relying on automatic `RadarShare` to keep all allies informed.
- Moves in an S-curve pattern to avoid becoming an easy static target.
- Fires **low-power harassing shots** (0.5 power) — this is intentional; the scout's job is intelligence, not kill shots.
- Has no dedicated target-priority logic; fires at whatever its gun happens to face.

**Swarm contribution**: By keeping the radar sweeping at all times, `RedScout` ensures the attacker tanks always have a fresh target picture, even when their own radars are locked onto a specific target.

---

### RedAttacker (RedAlpha & RedBravo)

**File**: `TankSwarmCode.SwarmTanks.Red/RedAttacker.cs` (~169 lines)  
**Role**: Attacker  
**Instances**: Two tanks — `RedAlpha` and `RedBravo`

**Strategy**:
- Selects the freshest enemy contact from `RadarMap` (via `GetFreshestEnemy()`).
- Uses **linear prediction** to lead moving targets: calculates where the target will be when the bullet arrives, based on `VelocityVector` and bullet travel time.
- Fires at **2.5 power** — high damage, slower bullet; only fires when gun alignment is within 5°.
- Advances toward the target while tracking, closing the engagement distance.
- Broadcasts **`RequestBackup`** when energy drops below 25, allowing a more sophisticated swarm to converge on a low-health attacker.
- Shares its own `RadarMap` entries with allies each tick.

**Key technique — linear prediction**:

```
travelTime = distance / bulletSpeed
predictedPosition = target.Position + target.VelocityVector × travelTime
aimAngle = bearing to predictedPosition
```

The gun fires only when `|gunBearing - aimAngle| < 5°`.

---

### RedEcmJammer

**File**: `TankSwarmCode.SwarmTanks.Red/RedEcmJammer.cs`  
**Role**: EcmSpecialist  
**Cannon**: None

**Strategy**:
- Carries no cannon — all energy is reserved for electronic warfare.
- Default mode: **Spoof** — projects two ghost-tank echoes that drift around the arena, polluting every enemy's `RadarMap` with phantom contacts. Enemies who don't run Burnthrough will waste fire on these ghosts and misread the battlefield.
- Threat response: switches to **Jam** for 25 ticks when hit by a bullet, making it very difficult for the shooter to re-acquire it. Reverts to Spoof automatically.
- Orbits the arena centre so its ghost projections land in the contested mid-arena space where enemies are most active.
- Broadcasts `EcmAlert` when it takes fire, alerting allies that the enemy has located it.
- Broadcasts `EnemySpotted` for every radar contact so the swarm retains a common picture even though the jammer never fires.

**Key interaction**: If the enemy swarm does not include a Burnthrough-capable tank, `RedEcmJammer`'s ghosts will contaminate the entire enemy `RadarMap` indefinitely. Adding a `BlueEcmOperator` to the Blue side is the primary counter.

---

### RedFlank (RedWolf & RedFox)

**File**: `TankSwarmCode.SwarmTanks.Red/RedFlank.cs` (~135 lines)  
**Role**: Attacker  
**Instances**: Two tanks — `RedWolf` (+90° offset) and `RedFox` (−90° offset)

**Strategy**:
- Uses the same target-acquisition logic as `RedAttacker` but approaches from a fixed **bearing offset** relative to the straight line between the flanker and the target.
- `RedWolf` circles to the target's right (relative to the attacker-target axis); `RedFox` circles to the left.
- Together they create a **pincer manoeuvre**: the enemy faces fire from three directions simultaneously.
- Fires at **2.0 power** when aligned within 12° and within 300 px.

The flanking offset is applied by computing the desired position perpendicular to the attacker–target line, then steering toward that position while maintaining radar lock.

---

## Blue Swarm (SwarmId = 2) — Defensive Coordination

The Blue Swarm has a clear command hierarchy: `BlueCommander` designates targets and issues positional orders, while specialist tanks execute their roles based on received messages.

### BlueCommander

**File**: `TankSwarmCode.SwarmTanks.Blue/BlueCommander.cs` (~220 lines)  
**Role**: Attacker

**Strategy**:
- Holds a position near the arena centre.
- Scans the entire `RadarMap` each tick and selects the **lowest-energy enemy** as the priority target.
- Broadcasts `TargetLocked` with the chosen target name, causing the whole swarm to refocus.
- Broadcasts `FormationMove` with rally positions to reposition `BluePatrol` and `BlueWarden`.
- Fires adaptively: **2.0–3.0 power** depending on distance and target energy.
- Retreats toward a safe corner when energy drops below 28; resumes attacking when energy recovers above 55.

**Command flow**:

```
BlueCommander scans RadarMap
    → selects lowest-energy enemy
    → broadcasts TargetLocked("RedAlpha")

BluePatrol receives TargetLocked("RedAlpha")
    → updates _priorityTargetName = "RedAlpha"
    → steers toward RedAlpha's last-known position

BlueSniper receives TargetLocked("RedAlpha")
    → only acts if TargetLocked; otherwise holds corner position

BlueWarden receives TargetLocked("RedAlpha")
    → adjusts oscillation centre toward the designated target
```

---

### BluePatrol (two instances)

**File**: `TankSwarmCode.SwarmTanks.Blue/BluePatrol.cs` (~215 lines)  
**Role**: Defender  
**Instances**: Two tanks patrolling the left and right halves of the arena

**Strategy**:
- Patrols between two waypoints in its assigned arena half when no commander order is active.
- On `TargetLocked` message: abandons patrol route and steers toward the designated enemy.
- On `FormationMove` message: moves to the specified rally position.
- Fires **1.5–2.5 power** adaptively based on distance (closer = higher power).
- Maintains its own radar scan to supplement the team picture.

---

### BlueSniper (two instances)

**File**: `TankSwarmCode.SwarmTanks.Blue/BlueSniper.cs` (~217 lines)  
**Role**: Support  
**Instances**: Two tanks camped in opposite corners

**Strategy**:
- Camps in a designated corner and only moves when forced out.
- Only responds to `TargetLocked` orders from `BlueCommander` — ignores general radar contacts.
- Fires at **maximum power (3.0)** with strict alignment tolerance (3°).
- Performs **inbound bullet evasion**: detects bullets in `Arena.Bullets` aimed at its current position and sidesteps.
- Relocates to a new corner position if an enemy closes within 80 px.

**Bullet evasion logic**:

```
For each bullet in Arena.Bullets:
    if bullet heading points within 15° toward this tank's position
    and bullet is within 150 px:
        SetTurnRight(90)   // sidestep perpendicular to bullet travel
        SetAhead(40)
```

---

### BlueEcmOperator

**File**: `TankSwarmCode.SwarmTanks.Blue/BlueEcmOperator.cs`  
**Role**: EcmSpecialist  
**Cannon**: Light (max 2.0 power)

**Strategy**:
- Primary mission: **Burnthrough** — keeps its radar cleared of enemy jamming and ghost echoes, protecting the Blue swarm's radar picture.
- Detects `"Ghost-*"` contacts in `OnScannedTank`, broadcasts `EcmAlert`, and forces itself into Burnthrough mode for 40 ticks — ensuring it sees through the active spoof field.
- **Responds to ally `EcmAlert` messages**: immediately activates Burnthrough, coordinating ECCM coverage across the swarm.
- **Offensive Jam window**: when energy exceeds 70 and an enemy ECM tank has been confirmed, switches to Jam for 20 ticks, disrupting the enemy's Spoof pipeline and forcing them defensive.
- Falls back to `Off` when energy drops below 25, preserving survival.
- Patrols a figure-8 path to maintain arena coverage.
- Fires opportunistically at confirmed enemy contacts (not ghosts) at up to 2.0 power.

**Key interaction**: `BlueEcmOperator` directly counters `RedEcmJammer`. Its Burnthrough reduces ghost filter chance to ~70 % per sweep, and its `EcmAlert` broadcasts can trigger Burnthrough across the whole Blue swarm if other tanks implement `OnSwarmMessage`.

---

### BlueWarden

**File**: `TankSwarmCode.SwarmTanks.Blue/BlueWarden.cs` (~165 lines)  
**Role**: Defender

**Strategy**:
- Anchors near the arena centre and oscillates back and forth (like a pendulum) to be an unpredictable target.
- Performs a full radar sweep to ensure the team always has central-area coverage.
- Broadcasts `EnemySpotted` for every new radar contact.
- Fires **adaptively** (1.0–3.0 power based on distance and target energy).
- Responds to `Commander` orders: adjusts oscillation centre toward the designated target.

---

## Swarm Comparison

| | Red Swarm | Blue Swarm |
|---|-----------|------------|
| **Philosophy** | Aggressive, fast, offensive + EM deception | Coordinated, defensive, ECCM-protected |
| **Radar coverage** | Scout-driven + ECM ghost injection | Commander-driven + Operator Burnthrough |
| **Engagement range** | Close-to-medium | Medium-to-long |
| **Coordination type** | Loose (auto RadarShare, backup request, EcmAlert) | Tight (TargetLocked, FormationMove, EcmAlert response) |
| **Special tactics** | Flanking pincer, linear prediction, ghost spoofing | Corner camping, bullet evasion, priority targeting, ECCM |
| **ECM capability** | Jammer (Spoof → Jam on threat) | Operator (Burnthrough → Jam offensive) |
| **Tank count** | 6 (inc. ECM-Jammer) | 7 (inc. ECM-Operator) |

---

[← Data Models](ch08-data-models.md) | [Table of Contents](TOC.md) | [Next: Building Your Own Tank →](ch10-custom-tank.md)
