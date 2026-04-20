# Chapter 7: Radar System

[← Swarm Communication](ch06-swarm-communication.md) | [Table of Contents](TOC.md) | [Next: Data Models →](ch08-data-models.md)

---

## Overview

The radar is the primary sense organ of every tank. It sweeps an arc each tick and detects other tanks whose centre point falls within that arc **and** who are not occluded by a building.

**Important**: A tank's radar is offline whenever it is running `EcmMode.Jam` or `EcmMode.JamAndSpoof`. No scans are performed and no `OnScannedTank` events fire for that tank while jamming. See [Chapter 13: ECM System](ch13-ecm-system.md) for full details.

---

## Sweep Arc

Each tick the radar rotates by up to **45°** in the commanded direction. The engine records:

- `PrevRadarHeading` — the radar's heading at the **start** of the tick (before the turn)
- `RadarHeading` — the radar's heading at the **end** of the tick (after the turn)

The sweep arc is the angular region between these two headings, traversed in the commanded direction. Any tank whose absolute bearing from the scanning tank falls within this arc is a potential radar contact, subject to line-of-sight.

### Wraparound Handling

The arc correctly handles the 0°/360° wraparound. A sweep from 350° to 10° (a 20° clockwise arc) correctly detects targets at 355° and 5°.

---

## Line-of-Sight Check

After the arc test, the engine performs a segment-rectangle intersection test (Liang–Barsky algorithm) from the scanning tank's centre to the target's centre against every building in the arena. If any building intersects this segment, the target is **not detected**.

This creates realistic occlusion: tanks can hide behind buildings, and scouting manoeuvres around obstacles are tactically meaningful.

---

## Building Echo Detection

In the same sweep pass, the engine also checks every building to see if its nearest face falls within the sweep arc. When a building is hit the radar pulse reflects off the **facing wall faces only** — the scanner never learns about walls hidden behind the building.

For each building in the sweep arc:

1. The engine clamps the scanner's position onto the building's AABB to find the **nearest surface point**.
2. The absolute bearing to that point is checked against `AngleInSweep`.
3. If the bearing is inside the arc, the engine determines which of the four wall faces are visible from the scanner (1 face for a direct hit, 2 faces for a corner hit).
4. A `BuildingEcho` is assembled from only those faces and delivered to the scanner via `OnScannedBuilding`.

The shadow behind the building is implicit — no `OnScannedBuilding` is fired for walls the radar cannot see, and any tank hidden in that shadow is not detected by `OnScannedTank` either.

### Building echo vs tank scan

| | Tank scan | Building echo |
|---|---|---|
| **Event** | `OnScannedTank` | `OnScannedBuilding` |
| **Data** | Full tank state (heading, velocity, energy) | Wall segments + distance + bearing |
| **Affected by ECM** | Yes — drop/corrupt chances apply | No — buildings do not jam |
| **Auto-broadcast** | `RadarShare` to all allies | `BuildingEchoShare` to all allies |
| **Stored in** | `RadarMap` (keyed by tank name) | `BuildingWallMap` (keyed by wall endpoints) |

---

## Scan Events

A successful tank scan fires events on both participants:

- **Scanner** receives `OnScannedTank(ScannedTankEventArgs e)` with `e.Result` (`ScanResult`).
- **Target** receives `OnPainted(PaintedEventArgs e)` with the painter's name and position.
- **Scanner** additionally receives `OnScannedBuilding(ScannedBuildingEventArgs e)` for each building whose face falls in the sweep arc.

Scans are filtered for ECM effects before either event fires. A dropped scan fires neither event. A corrupted scan fires `OnScannedTank` with false data on the scanner and **does** fire `OnPainted` on the target (the beam still reached it physically). Building echoes are never affected by ECM.

---

## ScanResult vs RadarContact

| | `ScanResult` | `RadarContact` |
|---|---|---|
| **Delivered by** | `OnScannedTank` — instantaneous scan event | `RadarMap` — persistent record |
| **Key fields** | `Name`, `SwarmId`, `Bearing`, `Distance`, `Heading`, `Velocity`, `Energy`, `Position` | same state fields + `IsAlly`, `Timestamp`, `SpottedBy`, `VelocityVector` |
| **Ally flag** | Check `SwarmId == this.SwarmId` manually | `IsAlly` property |
| **Freshness** | Always current (just scanned) | Compare `Timestamp` to `Arena.TickNumber` |
| **Source** | Only this tank's own radar | Own scans merged with ally `RadarShare` broadcasts |

---

## RadarMap

`SwarmTankBase` maintains `IReadOnlyDictionary<string, RadarContact>` called `RadarMap`, keyed by tank name. Entries are added or refreshed whenever:

1. This tank's own radar arc detects a target.
2. An ally broadcasts a `RadarShare` message containing a newer contact (higher `Timestamp`).

**Merge rule**: the newer `Timestamp` wins. An ally's fresher data always replaces a stale own observation.

### Staleness

`RadarMap` entries are never automatically removed — an entry for a destroyed tank persists with its last-known data. Always compare `contact.Timestamp` to `Arena.TickNumber` to assess freshness. The helper `GetFreshestEnemy()` filters by both staleness and ally status.

### Radar offline (jamming)

While jamming, no new contacts are added from this tank's own radar. Because jamming also blocks incoming radio, no `RadarShare` or `Painted` messages from allies arrive either. A tank that jams for many ticks will have an entirely stale intelligence picture when it comes back online.

---

## BuildingWallMap

`SwarmTankBase` maintains `IReadOnlyDictionary<string, BuildingEcho>` called `BuildingWallMap`, keyed by wall face identity (endpoint coordinates as a string). Entries are added or refreshed whenever:

1. This tank's own radar sweep reflects off a building face (`OnScannedBuilding`).
2. An ally broadcasts a `BuildingEchoShare` message containing a newer echo (higher `Timestamp`).

**Merge rule**: same as `RadarMap` — the newer `Timestamp` wins.

**Reset on round start**: buildings regenerate each round at new positions, so `BuildingWallMap` is cleared automatically by `Initialize` before each round. Tank contacts in `RadarMap` are not cleared.

### Key format

The map key is the wall endpoints joined as `"x0,y0-x1,y1|..."`. Because wall endpoints are derived directly from the building's AABB, the same wall face always produces the same key regardless of which tank echoed it or from which angle.

---

## Bearing Calculation

Bearings are expressed **relative to this tank's body heading**, in the range (−180, +180]:

```
RelativeBearing = NormalizeTo180(absoluteAngleToTarget − State.Heading)
```

- **Positive** values are to the right (clockwise).
- **Negative** values are to the left (counter-clockwise).

`OnScannedTank` provides `e.Result.Bearing` in this relative form. To aim the gun at a scanned target:

```csharp
public override void OnScannedTank(ScannedTankEventArgs e)
{
    base.OnScannedTank(e);   // record in RadarMap

    // Gun offset: gun heading relative to body heading
    double gunOffset = State.GunHeading - State.Heading;
    double gunTurn   = NormalizeAngle(e.Result.Bearing - gunOffset);
    SetTurnGunRight(gunTurn);
    SetFire(2.0);
}
```

Normalise all angular differences to (−180, +180] before using them in turn commands to avoid spinning the wrong way around.

---

## Radar Strategies

### Continuous Spin

The simplest strategy: keep the radar spinning at maximum speed.

```csharp
public override void OnTick(TickEventArgs e)
{
    SetTurnRadarRight(double.MaxValue);   // engine clamps to 45°/tick
}
```

Guarantees every tank in the arena will be scanned within 8 ticks. Used by `RedArrow` (Role = Scout) to keep the whole Red swarm's `RadarMap` current.

### Lock-on Tracking

Narrow the radar to stay on a known target, refreshing the contact every tick:

```csharp
public override void OnTick(TickEventArgs e)
{
    var target = GetFreshestEnemy();
    if (target != null)
    {
        double absoluteBearing = Math.Atan2(
            target.Position.X - State.Position.X,
            target.Position.Y - State.Position.Y) * (180.0 / Math.PI);

        double radarTurn = NormalizeAngle(absoluteBearing - State.RadarHeading);
        SetTurnRadarRight(radarTurn * 2);   // ×2 keeps arc sweeping across the target
    }
}
```

Multiplying by 2 creates a small oscillation that keeps the arc crossing the target even as it moves.

### Wide-Sweep Scout

Fixed large radar turn every tick maintains a consistent wide arc:

```csharp
SetTurnRadarRight(180);   // sweeps 45°/tick, effectively a full spin
```

---

## Firing Restriction while Jamming

A tank whose radar is offline (`Jam` or `JamAndSpoof`) **cannot fire**. The engine suppresses the shot regardless of what `SetFire()` was called with. The only valid targeting data available to a jammer is whatever was in `RadarMap` before jamming started — but those contacts age out quickly in a fast-moving battle.

---

## Electronic Counter-Measures (ECM)

ECM is a per-tick energy expenditure that interferes with the radar pipeline. It is activated by calling `SetEcm(EcmMode)` from `OnTick`. Full details are in [Chapter 13: ECM System](ch13-ecm-system.md).

### How jamming intercepts the radar pipeline

When the **target** is running `EcmMode.Jam` or `EcmMode.JamAndSpoof`, the engine rolls a random number before delivering `OnScannedTank`:

| Scanner ECM | Drop chance | Corrupt chance | Normal chance |
|-------------|-------------|----------------|---------------|
| Off | 50 % | 30 % | 20 % |
| Burnthrough | 8 % | 8 % | 84 % |

- **Dropped** — `OnScannedTank` is never called; the target is invisible this tick. `OnPainted` is also not fired.
- **Corrupted** — `OnScannedTank` fires with randomised position, heading, velocity, and energy. `OnPainted` **is** fired on the target.
- **Normal** — the scan proceeds unmodified. `OnPainted` fires on the target.

### Ghost echoes (Spoof and JamAndSpoof)

When a tank runs `EcmMode.Spoof` or `EcmMode.JamAndSpoof`, the engine maintains two drifting "ghost" positions near the spoofing tank. For each ghost whose position falls within an enemy's sweep arc (and has clear line-of-sight), the engine injects a fake `OnScannedTank` with a name like `"Ghost-XXXX"`. Ghost contacts do **not** trigger `OnPainted` — only real tanks do.

A scanner running Burnthrough has a 70 % chance of recognising and discarding each ghost before it reaches `OnScannedTank`.

---

## Rendering

The renderer shows radar information in two ways:

1. **Radar beam** — a short line (16 px) from the tank centre in the direction of `RadarHeading`. **Hidden when jamming** (`Jam` or `JamAndSpoof`).
2. **Radar halo** — when a scan detects a target, an expanding/fading sonar-like pulse is rendered at the scanner's position, lasting 10 ticks.

The phosphor-decay sweep trail (last 12 ticks of radar heading history) is also suppressed while jamming and cleared when jamming begins, so the trail restarts cleanly.

See [Chapter 11: Arena Rendering & UI](ch11-rendering.md) for visual details.

---

[← Swarm Communication](ch06-swarm-communication.md) | [Table of Contents](TOC.md) | [Next: Data Models →](ch08-data-models.md)
