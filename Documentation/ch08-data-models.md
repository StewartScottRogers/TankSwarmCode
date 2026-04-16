# Chapter 8: Data Models Reference

[← Radar System](ch07-radar-system.md) | [Table of Contents](TOC.md) | [Next: Built-in Tank AI →](ch09-builtin-tanks.md)

---

All models live in `TankSwarmCode.SwarmTank.Interfaces/Models/`. They are **immutable records** (or classes with `init`-only properties) — AI code receives snapshots, never mutable engine objects.

---

## TankState

The complete snapshot of a single tank's state for a given tick. Available as `State` inside `SwarmTankBase`.

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Unique identifier (e.g., `"RedAlpha"`) |
| `SwarmId` | `int` | Swarm group; `0` = solo tank |
| `Role` | `TankRole` | Declared role: Scout, Attacker, Defender, Support, or EcmSpecialist |
| `Position` | `Vector2D` | Centre position in arena pixels |
| `Heading` | `double` | Body heading in degrees (0 = North, clockwise) |
| `GunHeading` | `double` | Absolute gun heading |
| `RadarHeading` | `double` | Absolute radar heading |
| `Velocity` | `double` | Current speed in px/tick; negative = reversing |
| `Energy` | `double` | Current energy (0–100); 0 = destroyed |
| `IsAlive` | `bool` | False once energy reaches 0 |
| `DestroyedAtTick` | `long` | Tick number when destroyed; 0 if still alive |
| `ActiveEcm` | `EcmMode` | ECM mode active this tick; visible to enemy scanners |

---

## TankCommand

The command buffer that AI code fills each tick. Not directly accessible; populated via the `Set*` helper methods on `SwarmTankBase`.

| Property | Type | Description |
|----------|------|-------------|
| `BodyTurnDegrees` | `double` | Net body rotation this tick (positive = clockwise) |
| `MoveDistance` | `double` | Distance to travel (positive = forward, negative = back) |
| `GunTurnDegrees` | `double` | Net gun rotation this tick |
| `RadarTurnDegrees` | `double` | Net radar rotation this tick |
| `FirePower` | `double` | `> 0` to fire; clamped to `[0.1, 3.0]` |
| `BroadcastMessages` | `List<SwarmMessage>` | Messages queued via `Broadcast()` |
| `EcmMode` | `EcmMode` | ECM mode to activate this tick (default `Off`); set via `SetEcm()` |

---

## Vector2D

A simple 2D position or direction vector.

| Property | Type | Description |
|----------|------|-------------|
| `X` | `double` | Horizontal component (pixels from left edge) |
| `Y` | `double` | Vertical component (pixels from top edge) |

Common operations expected from the consumer (not built into the struct — implement in your AI):

```csharp
// Distance between two positions
double dist = Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));

// Absolute bearing from 'from' to 'to' (degrees, 0 = North clockwise)
double bearing = Math.Atan2(to.X - from.X, to.Y - from.Y) * (180.0 / Math.PI);
```

---

## RadarContact

A snapshot of another tank as last seen by radar. Stored in `RadarMap` keyed by `Name`.

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Target's name |
| `EnemySwarmId` | `int` | Target's `SwarmId` |
| `IsAlly` | `bool` | `true` if same swarm as scanner |
| `Position` | `Vector2D` | Last-known centre position |
| `Heading` | `double` | Last-known body heading |
| `Velocity` | `double` | Last-known speed |
| `Energy` | `double` | Last-known energy |
| `Timestamp` | `long` | Tick number when this data was recorded |
| `SpottedBy` | `string` | Name of the tank that observed this contact |
| `VelocityVector` | `Vector2D` | Derived: direction × speed (for lead targeting) |

A contact becomes stale as ticks pass. Compare `Timestamp` to `Arena.CurrentTick` to gauge freshness.

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
| `Damage` | `double` | Derived: `4×Power + 2×(Power−1)` if Power > 1 |
| `EnergyReturn` | `double` | Derived: `3 × Power` (returned to shooter on hit) |

---

## SwarmMessage

Carries communication between swarm allies.

| Property | Type | Description |
|----------|------|-------------|
| `SenderName` | `string` | Originating tank name |
| `Type` | `SwarmMessageType` | Category of message |
| `Timestamp` | `long` | Tick when created |
| `TargetName` | `string?` | Name of a referenced tank (optional) |
| `Position` | `Vector2D?` | A position payload (optional) |
| `CustomData` | `string?` | Freeform text or serialised data (optional) |
| `RadarContact` | `RadarContact?` | Populated by engine for `RadarShare` messages |

---

## BuildingDefinition

An axis-aligned rectangular obstacle placed in the arena each round.

| Property | Type | Description |
|----------|------|-------------|
| `X` | `double` | Left edge (arena pixels) |
| `Y` | `double` | Top edge (arena pixels) |
| `Width` | `double` | Width in pixels |
| `Height` | `double` | Height in pixels |

Buildings are accessible via `Arena.Buildings` — a read-only list. Use them for path planning, cover selection, or corner-camping logic.

---

## IArenaContext

The read-only view of the simulation world exposed to every tank.

| Member | Type | Description |
|--------|------|-------------|
| `Width` | `double` | Arena width in pixels |
| `Height` | `double` | Arena height in pixels |
| `CurrentTick` | `long` | Tick counter (increments each simulation step) |
| `Tanks` | `IReadOnlyList<TankState>` | All tanks (alive and destroyed hulls) |
| `Bullets` | `IReadOnlyList<BulletState>` | All active bullets |
| `Buildings` | `IReadOnlyList<BuildingDefinition>` | All buildings for this round |

---

## TankRole Enum

| Value | Intended Use |
|-------|-------------|
| `Scout` | High mobility, radar coverage, intelligence gathering |
| `Attacker` | Direct combat, pursuit, high fire power |
| `Defender` | Territorial control, protective positioning |
| `Support` | Long-range fire, utility, secondary roles |
| `EcmSpecialist` | Electronic warfare; typically carries no cannon |

Roles are declared by the AI subclass and reported in `TankState.Role`. The engine imposes no mechanical differences — roles are purely advisory for swarm coordination logic.

---

## EcmMode Enum

Controls the ECM (Electronic Counter-Measures) system. Set via `SetEcm(EcmMode)` in `OnTick`. The engine deducts the energy cost before the radar phase each tick.

| Value | Effect | Energy cost/tick |
|-------|--------|-----------------|
| `Off` | No ECM active | 0 |
| `Jam` | Enemy radar scans targeting this tank have a 50 % drop chance and 30 % corrupt chance. Burnthrough reduces these to 8 %/8 %. | 0.5 |
| `Spoof` | Emits two drifting ghost contacts near this tank. Enemy scanners receive fake `OnScannedTank` events for each ghost in their sweep arc. Ghost names start with `"Ghost-"`. Burnthrough filters ~70 % of ghosts. | 0.8 |
| `Burnthrough` | Penetrates enemy jamming and filters enemy ghost contacts for this tank's own radar. | 0.3 |

---

## SwarmMessageType Enum

| Value | Description |
|-------|-------------|
| `RadarShare` | Automatic radar contact broadcast (engine-generated) |
| `EnemySpotted` | Manual sighting report |
| `TargetLocked` | Designate the swarm's priority target |
| `RequestBackup` | Signal for allies to assist |
| `FormationMove` | Order a positional manoeuvre |
| `FallBack` | Retreat order |
| `RoleChange` | Dynamic role reassignment |
| `Custom` | Application-defined; inspect `CustomData` |

---

[← Radar System](ch07-radar-system.md) | [Table of Contents](TOC.md) | [Next: Built-in Tank AI →](ch09-builtin-tanks.md)
