# Chapter 8: Data Models Reference

[← Radar System](ch07-radar-system.md) | [Table of Contents](TOC.md) | [Next: Built-in Tank AI →](ch09-builtin-tanks.md)

---

All models live in `TankSwarmCode.SwarmTank.Interfaces/Models/`. They are **immutable records** with `init`-only properties — AI code always receives snapshots, never mutable engine objects.

---

## TankState

The complete snapshot of a single tank's state for a given tick. Available as `State` inside `SwarmTankBase`.

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Unique display identifier |
| `SwarmId` | `int` | Swarm group; `0` = solo tank |
| `Role` | `TankRole` | Declared role: None, Scout, Attacker, Defender, Support, or EcmSpecialist |
| `Position` | `Vector2D` | Centre position in arena pixels |
| `Heading` | `double` | Body heading in degrees (0 = North, clockwise) |
| `GunHeading` | `double` | Absolute gun heading |
| `RadarHeading` | `double` | Absolute radar heading at end of tick |
| `PrevRadarHeading` | `double` | Radar heading at start of tick; with `RadarHeading` defines the exact arc swept |
| `Velocity` | `double` | Current speed in px/tick; negative = reversing |
| `Energy` | `double` | Current energy (0–100); 0 = destroyed |
| `IsAlive` | `bool` | False once energy reaches 0 |
| `DestroyedAtTick` | `long` | Tick number when destroyed; 0 if still alive |
| `ActiveEcm` | `EcmMode` | ECM mode active this tick; visible to enemy scanners |

---

## TankCommand

The command buffer that AI code fills each tick via `Set*` helper methods on `SwarmTankBase`. Not directly accessible to AI authors.

| Property | Type | Description |
|----------|------|-------------|
| `BodyTurnDegrees` | `double` | Net body rotation this tick (positive = clockwise) |
| `MoveDistance` | `double` | Distance to travel (positive = forward, negative = back) |
| `GunTurnDegrees` | `double` | Net gun rotation this tick |
| `RadarTurnDegrees` | `double` | Net radar rotation this tick |
| `FirePower` | `double` | `> 0` to fire; clamped to `[0.1, 3.0]` |
| `BroadcastMessages` | `IReadOnlyList<SwarmMessage>` | Messages queued via `Broadcast()` |
| `EcmMode` | `EcmMode` | ECM mode to activate this tick; set via `SetEcm()` |

---

## Vector2D

A 2D position or direction vector. Declared as a `readonly record struct`.

| Member | Type | Description |
|--------|------|-------------|
| `X` | `double` | Horizontal component (pixels from left edge) |
| `Y` | `double` | Vertical component (pixels from top edge) |
| `Length` | `double` | Magnitude of the vector |
| `DistanceTo(Vector2D other)` | `double` | Euclidean distance to `other` |
| `BearingTo(Vector2D other)` | `double` | Absolute bearing to `other` in degrees (0 = North, clockwise) |

Common arithmetic patterns:

```csharp
// Distance between two positions
double dist = State.Position.DistanceTo(target.Position);

// Absolute bearing from this tank to a target
double bearing = State.Position.BearingTo(target.Position);

// Manual distance if preferred
double dx = target.Position.X - State.Position.X;
double dy = target.Position.Y - State.Position.Y;
double dist = Math.Sqrt(dx * dx + dy * dy);
```

---

## ScanResult

Data returned by `OnScannedTank` — an instantaneous snapshot of a detected tank.

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Display name of the scanned tank |
| `SwarmId` | `int` | Swarm of the scanned tank (compare to `this.SwarmId` for ally check) |
| `Bearing` | `double` | Bearing relative to scanner's body heading, (−180, 180] |
| `Distance` | `double` | Distance from scanner to target in pixels |
| `Heading` | `double` | Absolute body heading of the scanned tank |
| `Velocity` | `double` | Current speed of the scanned tank |
| `Energy` | `double` | Remaining energy of the scanned tank |
| `Position` | `Vector2D` | Absolute arena position of the scanned tank |

---

## RadarContact

A persistent sighting stored in `RadarMap`, keyed by tank name. Merged from own scans and ally `RadarShare` broadcasts. Covers both enemies and allies (`IsAlly` flag distinguishes them).

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Target's name |
| `EnemySwarmId` | `int` | Target's `SwarmId` |
| `IsAlly` | `bool` | `true` if same swarm as the observing tank |
| `Position` | `Vector2D` | Last-known centre position |
| `Heading` | `double` | Last-known body heading |
| `Velocity` | `double` | Last-known speed |
| `Energy` | `double` | Last-known energy |
| `Timestamp` | `long` | Tick number when this data was recorded |
| `SpottedBy` | `string` | Name of the tank that made this observation |
| `VelocityVector` | `Vector2D` | Derived: velocity decomposed into 2D arena vector (for linear-prediction firing) |

A contact becomes stale as ticks pass. Compare `Timestamp` to `Arena.TickNumber` to gauge freshness. `GetFreshestEnemy(staleAfterTicks)` handles this automatically.

---

## BulletState

An immutable snapshot of an active bullet.

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `Guid` | Unique bullet identifier |
| `OwnerName` | `string` | Name of the tank that fired this bullet |
| `Position` | `Vector2D` | Current centre position |
| `Heading` | `double` | Travel direction in degrees |
| `Power` | `double` | Firing power `[0.1, 3.0]` |
| `Speed` | `double` | Derived: `20 − 3 × Power` |
| `Damage` | `double` | Derived: `4 × Power + (Power > 1 ? 2 × (Power − 1) : 0)` |
| `EnergyReturn` | `double` | Derived: `3 × Power` (credited to shooter on hit) |
| `IsDeflected` | `bool` | `true` after a non-lethal hit; bullet is decelerating visually |
| `CurrentSpeed` | `double` | Current travel speed; equals `Speed` for live bullets; decays for deflected bullets |

---

## SwarmMessage

Carries communication between swarm allies.

| Property | Type | Description |
|----------|------|-------------|
| `SenderName` | `string` | Originating tank name |
| `Type` | `SwarmMessageType` | Category of message |
| `Timestamp` | `long` | Tick when created (`Arena.TickNumber`) |
| `TargetName` | `string?` | Name of a referenced tank (optional) |
| `Position` | `Vector2D?` | A position payload (optional) |
| `CustomData` | `string?` | Freeform text or serialised data (optional) |
| `RadarContact` | `RadarContact?` | Populated by the engine for `RadarShare` messages |

---

## BuildingDefinition

An axis-aligned rectangular obstacle placed in the arena each round.

| Property | Type | Description |
|----------|------|-------------|
| `X` | `double` | Left edge (arena pixels) |
| `Y` | `double` | Top edge (arena pixels) |
| `Width` | `double` | Width in pixels |
| `Height` | `double` | Height in pixels |

Buildings are accessible via `Arena.Buildings`. Use them for path planning, cover selection, or line-of-sight calculations.

---

## IArenaContext

The read-only view of the simulation world exposed to every tank via the `Arena` property.

| Member | Type | Description |
|--------|------|-------------|
| `ArenaWidth` | `double` | Arena width in pixels |
| `ArenaHeight` | `double` | Arena height in pixels |
| `TickNumber` | `long` | Current tick counter (starts at 1) |
| `LivingTankCount` | `int` | Total living tanks across all swarms |
| `GetSwarmSize(int swarmId)` | `int` | Living tanks in the specified swarm |
| `GetActiveBullets()` | `IReadOnlyList<BulletState>` | All currently active bullets (for evasion logic) |
| `Buildings` | `IReadOnlyList<BuildingDefinition>` | All buildings in this round |

---

## TankRole Enum

| Value | Intended Use |
|-------|-------------|
| `None` | No role declared; solo or unclassified combatant |
| `Scout` | High mobility, radar coverage, intelligence gathering |
| `Attacker` | Direct combat, pursuit, high fire power |
| `Defender` | Territorial control, protective positioning |
| `Support` | Long-range fire, utility, secondary roles |
| `EcmSpecialist` | Electronic warfare; typically carries reduced or no cannon |

Roles are declared by the AI subclass and reported in `TankState.Role`. The engine imposes no mechanical differences — roles are advisory for swarm coordination logic.

---

## EcmMode Enum

Controls the ECM system. Set via `SetEcm(EcmMode)` in `OnTick`.

| Value | Effect | Energy cost/tick |
|-------|--------|-----------------|
| `Off` | No ECM active | 0 |
| `Jam` | Enemy scans have 50 % drop / 30 % corrupt chance; own radar, firing, and radio disabled | 0.5 |
| `Spoof` | Projects two drifting ghost contacts into enemy radar sweeps; own radar and radio remain active | 0.8 |
| `Burnthrough` | Reduces jam drop/corrupt to 8 % each; filters 70 % of ghost contacts | 0.3 |
| `JamAndSpoof` | Full Jam + full Spoof simultaneously; own radar, firing, and radio all disabled | 1.3 |

See [Chapter 13: ECM System](ch13-ecm-system.md) for full details.

---

## SwarmMessageType Enum

| Value | Description |
|-------|-------------|
| `RadarShare` | Auto-sent by base class; carries enemy `RadarContact` |
| `Painted` | Auto-sent by base class when an enemy radar sweeps this tank |
| `EnemySpotted` | Manual sighting report |
| `TargetLocked` | Designate the swarm's priority target |
| `RequestBackup` | Signal for allies to assist |
| `FormationMove` | Order a positional manoeuvre |
| `FallBack` | Retreat order |
| `RoleChange` | Dynamic role reassignment |
| `EcmAlert` | Enemy ECM or ghost contacts detected; allies should activate Burnthrough |
| `Custom` | Application-defined; inspect `CustomData` |

---

[← Radar System](ch07-radar-system.md) | [Table of Contents](TOC.md) | [Next: Built-in Tank AI →](ch09-builtin-tanks.md)
