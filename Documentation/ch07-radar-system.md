# Chapter 7: Radar System

[← Swarm Communication](ch06-swarm-communication.md) | [Table of Contents](TOC.md) | [Next: Data Models →](ch08-data-models.md)

---

## Overview

The radar is the primary sense organ of every tank. It sweeps an arc each tick and detects other tanks whose centre point falls within that arc **and** who are not occluded by a building.

**Important**: A tank's radar is offline whenever it is running `EcmMode.Jam` or `EcmMode.JamAndSpoof`. No scans are performed and no `OnScannedTank` events fire for that tank while jamming. See [Chapter 13: ECM System](ch13-ecm-system.md) for full details.

---

## Sweep Arc

Each tick the radar rotates by up to **45°** in the commanded direction. The engine records:

- `PrevRadarHeading` — the radar's heading at the start of the tick
- `RadarHeading` — the radar's heading at the end of the tick (after the turn is applied)

The sweep arc is the angular region between these two headings. Any tank whose absolute bearing from the scanning tank falls within this arc is a potential radar contact (subject to line-of-sight checks).

### Wraparound Handling

The arc correctly handles the 0°/360° wraparound. For example, a sweep from 350° to 10° (a 20° clockwise arc) detects targets at 355° and 5° without special-casing.

---

## Line-of-Sight Check

After the arc test, the engine performs a segment-rectangle intersection test (Liang–Barsky algorithm) between:

- The scanning tank's centre position
- The target tank's centre position

...against every building in the arena. If any building intersects this segment, the target is **not detected**.

This creates realistic occlusion: tanks can hide behind buildings, and scouting manoeuvres around buildings are meaningful.

---

## RadarContact

When a tank is detected, a `RadarContact` is created or updated:

```csharp
public class RadarContact
{
    public string   Name          { get; init; }  // target's Name
    public int      EnemySwarmId  { get; init; }  // target's SwarmId
    public bool     IsAlly        { get; init; }  // same SwarmId as scanner
    public Vector2D Position      { get; init; }  // last-known position
    public double   Heading       { get; init; }  // last-known body heading
    public double   Velocity      { get; init; }  // last-known speed
    public double   Energy        { get; init; }  // last-known energy
    public long     Timestamp     { get; init; }  // tick when last updated
    public string   SpottedBy     { get; init; }  // scanner's Name
    public Vector2D VelocityVector { get; }       // derived from Heading × Velocity
}
```

`VelocityVector` is computed from `Heading` and `Velocity` and is used for **linear prediction** (leading the target when firing).

---

## RadarMap

`SwarmTankBase` maintains a `Dictionary<string, RadarContact>` called `RadarMap`. Entries are added or refreshed whenever:

1. This tank's own radar arc detects a target.
2. An ally broadcasts a `RadarShare` message containing a newer contact (higher `Timestamp`).
3. An ally broadcasts a `Painted` message with an enemy scanner's position — that position can be used for targeting even without a direct radar lock.

The merge rule: **the newer timestamp wins**. If an ally spotted a target 2 ticks ago and your own radar spotted it 5 ticks ago, the ally's data replaces yours.

### Staleness

`RadarMap` entries are never automatically removed. An entry for a destroyed tank will remain with its last-known data. Always check `Timestamp` against `Arena.CurrentTick` to assess how stale a contact is. The helper `GetFreshestEnemy()` returns the entry with the highest `Timestamp` among non-ally contacts.

### Radar offline (jamming)

When a tank is jamming, no new contacts are added to its `RadarMap` from its own radar. Contacts already in the map become progressively stale. Because jamming also blocks incoming radio, no `RadarShare` or `Painted` messages from allies reach the jammer either. A tank that jams for many ticks will have an entirely stale intelligence picture when it comes back online.

---

## Firing Restriction

A tank whose radar is offline (due to `Jam` or `JamAndSpoof`) **cannot fire**. The engine suppresses the shot regardless of what `SetFire()` was called with. The only way for a jamming tank to have valid targeting data is through contacts that were acquired *before* jamming started — but those contacts age out quickly in a fast-moving battle.

---

## The `OnPainted` Event

Every successful (non-dropped) scan fires a callback on the **target** — the tank being swept over:

```csharp
protected virtual void OnPainted(PaintedEventArgs e)
```

| Field | Description |
|-------|-------------|
| `PainterName` | Name of the tank whose radar painted this tank |
| `PainterSwarmId` | Swarm the painter belongs to |
| `PainterPosition` | Arena position of the painter at the moment of the scan |

The base class implementation auto-broadcasts a `[PAINTED]` (`SwarmMessageType.Painted`) message to all swarm allies. This gives the entire swarm the scanner's position at no extra coding cost.

**Key implication**: aggressive radar use is a double-edged sword. Every scan that connects reveals your own position to the target and, through the `[PAINTED]` broadcast, to the target's entire swarm.

See [Chapter 6: Swarm Communication](ch06-swarm-communication.md) for the `[PAINTED]` message type and [Chapter 13: ECM System](ch13-ecm-system.md) for the full `OnPainted` API.

---

## Bearing Calculation

Bearings are expressed **relative to this tank's body heading**, in the range (−180, +180]:

```
RelativeBearing = NormalizeTo180(absoluteAngleToTarget − State.Heading)
```

- **Positive** values are to the right (clockwise).
- **Negative** values are to the left (counter-clockwise).

`OnScannedTank(e)` provides `e.BearingDegrees` in this relative form. To aim the gun at a scanned target:

```csharp
double gunTurn = e.BearingDegrees                   // bearing to target
               + State.GunHeading - State.Heading;  // offset: gun relative to body
SetTurnGunRight(gunTurn);
```

Normalise all angular differences to (−180, +180] before using them in turn commands to avoid spinning the wrong way around.

---

## Radar Strategies

### Continuous Spin

The simplest strategy: keep the radar spinning at maximum speed.

```csharp
// OnTick
SetTurnRadarRight(double.MaxValue);   // engine clamps to 45°/tick
```

Guarantees every tank in the arena will be scanned within 8 ticks.

### Lock-on Tracking

Narrow the radar to stay on a known target, refreshing the contact every tick:

```csharp
// OnTick
var target = _myTarget;
if (target != null)
{
    double absoluteBearingToTarget = Math.Atan2(
        target.Position.X - State.Position.X,
        target.Position.Y - State.Position.Y) * (180 / Math.PI);

    double radarTurn = NormalizeTo180(absoluteBearingToTarget - State.RadarHeading);
    SetTurnRadarRight(radarTurn * 2);  // × 2 to ensure the sweep crosses the target
}
```

Multiplying by 2 creates a small oscillation that keeps the arc sweeping across the target even if it moves.

### Wide-Sweep Scout

Set the radar turn to a large fixed value each tick to maintain a consistent wide arc:

```csharp
SetTurnRadarRight(180);   // sweeps 45° per tick, effectively spinning
```

Used by `RedScout`, which pairs this with `RadarShare` broadcasts to keep the whole swarm informed.

---

## Rendering

The renderer displays radar information in two ways:

1. **Radar beam** — a short line from the tank centre in the direction of the current `RadarHeading`. **Hidden when the tank is jamming** (`Jam` or `JamAndSpoof`), since the radar is physically offline.
2. **Radar halo** — when a scan detects a target, an expanding/fading sonar-like pulse is rendered at the scanner's position. The halo has a lifetime of 10 ticks. Not rendered for jamming tanks (no scan is performed).

The radar sweep trail (phosphor-decay arc history) is also suppressed while jamming and its history is cleared, so the trail restarts cleanly when jamming ends.

See [Chapter 11: Arena Rendering & UI](ch11-rendering.md) for visual details.

---

## Electronic Counter-Measures (ECM)

ECM is a per-tick energy expenditure that interferes with the radar pipeline described above. It is activated by calling `SetEcm(EcmMode)` from `OnTick`. Full details are in [Chapter 13: ECM System](ch13-ecm-system.md).

### How jamming intercepts the radar pipeline

The engine normally calls `OnScannedTank` for every target whose bearing falls in the sweep arc. When the **target** is running `EcmMode.Jam` or `EcmMode.JamAndSpoof`, the engine rolls a random number before delivering the event:

| Scanner ECM | Drop chance | Corrupt chance | Normal chance |
|-------------|-------------|----------------|---------------|
| Off | 50 % | 30 % | 20 % |
| Burnthrough | 8 % | 8 % | 84 % |

- **Dropped** — `OnScannedTank` is never called; the target is invisible this tick. `OnPainted` is also not fired on the target.
- **Corrupted** — `OnScannedTank` is called with randomised position, heading, velocity, and energy; the contact looks plausible but is entirely fabricated. `OnPainted` **is** fired on the target (the radar beam still reached it).
- **Normal** — the scan proceeds exactly as without ECM. `OnPainted` fires on the target.

### Ghost echoes (Spoof and JamAndSpoof modes)

When a tank runs `EcmMode.Spoof` or `EcmMode.JamAndSpoof`, the engine maintains two "ghost" positions that drift independently around the spoofing tank. For each ghost that falls within any enemy's sweep arc (and has clear LOS), the engine injects a fake `OnScannedTank` event with a name like `"Ghost-XXXX"`. Ghost contacts do **not** trigger `OnPainted` — only a real radar hit on a real tank does.

### ECCM: Burnthrough

`EcmMode.Burnthrough` is the defensive counter: it suppresses both jam and spoof effects for a cost of 0.3 energy/tick. When an ally broadcasts `SwarmMessageType.EcmAlert`, a well-designed ECCM tank can react by activating Burnthrough.

---

[← Swarm Communication](ch06-swarm-communication.md) | [Table of Contents](TOC.md) | [Next: Data Models →](ch08-data-models.md)
