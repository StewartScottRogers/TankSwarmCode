# Chapter 11: Arena Rendering & UI

[← Building Your Own Tank](ch10-custom-tank.md) | [Table of Contents](TOC.md) | [Next: Configuration & Constants →](ch12-configuration.md)

---

## Overview

The renderer is implemented as `ArenaUserControl` (`TankSwarmCode/ArenaUserControl.cs`), a WinForms `UserControl` that hosts the `ArenaEngine` and paints each frame using GDI+. Rendering is decoupled from the simulation: the renderer reads **immutable snapshots** (`TankState`, `BulletState`) and never mutates engine state.

---

## ArenaUserControl

`ArenaUserControl` is responsible for:

1. Owning the `ArenaEngine` instance.
2. Driving the tick loop via a `System.Windows.Forms.Timer` at the configured ticks/second rate.
3. Painting the arena (double-buffered GDI+ surface) after each tick.
4. Handling mouse input for the interactive inspection panel.

Double buffering ensures each completed frame is swapped in atomically — no flicker even at high tick rates.

---

## Visual Elements

### Tank Body

Each tank is drawn as a filled square (18 × 18 px) rotated to match `TankState.Heading`. Team colour is determined by `SwarmId`:

| SwarmId | Colour |
|---------|--------|
| 0 (solo) | Silver |
| 1 (Red Swarm) | OrangeRed |
| 2 (Blue Swarm) | DodgerBlue |
| 3 | Gold |
| 4 | MediumOrchid |
| 5 | LimeGreen |
| 6+ | Coral / Chartreuse cycling |

### Cannon Barrel and Turret

The gun is rendered as a **filled 4 × 22 px rectangle** rotated to `GunHeading` using a GDI+ coordinate transform (not a manually computed line) — this keeps the barrel sharp at every angle under anti-aliasing. The barrel extends from the tank centre outward.

A **10 px-diameter circular turret** is drawn on top of the hull body, anchoring the barrel visually and giving the classic tank silhouette. Both the barrel and turret use dark grey fills with a silver outline.

### Radar Beam and Sweep Trail

A short line (16 px) in the direction of `RadarHeading` represents the current radar pointing angle. A phosphor-decay sweep trail (arc history from the last 12 ticks) fans out behind it.

**Both the beam and trail are hidden when the tank is jamming** (`EcmMode.Jam` or `EcmMode.JamAndSpoof`) — the radar is physically offline. The trail history is cleared when jamming begins so the trail restarts cleanly when jamming ends.

### Energy Bar

A horizontal bar (36 × 4 px) above each tank shows current energy as a proportion of maximum:

- Green: energy > 50 %
- Yellow: energy 25–50 %
- Red: energy < 25 %

### Radar Halo

When a scan detects a target, an expanding/fading circular pulse (sonar halo) is rendered at the scanning tank's position. Each halo lives for **10 ticks**, expanding outward and becoming more transparent as it ages. Multiple halos can be active simultaneously. Not rendered for jamming tanks.

### Scan Reflection Arc

When the radar sweeps across a detected tank, a wavefront arc is drawn from the scanner toward the contact — a bright expanding crescent that fades over several ticks. Toggle: **Radar reflections**.

### Destroyed Tank Hull

When a tank's energy reaches 0, its body stops moving but remains visible as a darkened hull at the `DestroyedAtTick` position. The hull persists until the round resets.

### Explosion Animation

Upon destruction, a 2.1-second burn animation plays:

- Phase 1 (0–400 ms): bright orange/yellow expanding flash
- Phase 2 (400–1200 ms): fireball ramps in, flames and debris expand
- Phase 3 (1200–2100 ms): dissipating smoke (grey fading to transparent)

Multiple simultaneous explosions are each tracked independently.

### Bullets

Active bullets are drawn as arrowhead projectiles:

- **Arrowhead triangle** pointing in the travel direction, scaled proportionally to power.
- **Tail shaft** extending behind the arrowhead for a velocity cue.
- **Glow halo** — a soft transparent ellipse behind the tip.
- Colour is the swarm colour of the firing tank, lightened for visibility.

#### Deflected (Ricochet) Bullets

When a non-lethal hit occurs, the bullet enters a deflected state and is rendered differently:

- Drawn as a **fading ember spark** (filled ellipse) rather than an arrowhead.
- A smaller **white hot core** overlays the ember; both shrink as speed decays.
- A short **trailing streak** shows the direction of travel, also fading.
- All elements fade to transparent as `BulletState.CurrentSpeed` decays toward zero.

#### Ricochet Flash

At the exact tick a non-lethal hit occurs, a single-frame **impact ring** is drawn at the collision point — an orange ring (~7 px radius) with a white filled centre, rendered from `ArenaEngine.RicochetFlashes`.

### Buildings

Buildings are filled rectangles drawn in a muted grey/brown with a darker border and window details. Painted before tanks and bullets so they appear as background obstacles.

### ECM Auras

When a tank has an active ECM mode, a distinctive animated aura is drawn on top of the hull (toggle: **ECM effects**):

| Mode | Visual |
|------|--------|
| **Jam** | 24 small (3 × 3 px) orange and yellow dots scattered randomly in a ~14 px band around the hull. The cluster repositions ~16 times per second using a seeded hash, creating an analog-static interference look. |
| **Spoof** | A faint purple ghost copy of the tank hull that slowly orbits the tank on a tight circular path (one full orbit every 3 seconds), pulsing in opacity as it moves. |
| **JamAndSpoof** | Both the Jam static-dot burst **and** the Spoof orbiting ghost hull simultaneously. |
| **Burnthrough** | A short bright cyan arc centred on the radar heading direction, with a thin beam line extending from the hull. Both pulse in brightness on a 0.6-second cycle and rotate as the radar turns. |

### ECM Ghost Echoes

While a Spoof or JamAndSpoof tank is active, translucent phantom tank silhouettes appear at each ghost echo position (rendered between bullets and live tanks). Each ghost is drawn:

- As a semi-transparent square in the spoofing swarm's colour.
- With a dashed X cross-hatch to distinguish it from real hulks.
- With a `?` label above it.
- With a flicker animation to prevent them from looking static.

Ghost echoes are visible to the spectator regardless of whether any scanner is actually being fooled.

---

## Interactive Features

### Left-click to Inspect

**Left-click** on any tank body to open the **info panel**, which displays:

- Tank name, swarm, and role
- ECM mode indicator (Off / JAM ⚡ / SPOOF 👻 / JAM+SPOOF / ECCM 📶)
- Current energy
- Position (X, Y)
- Body, gun, and radar headings
- Velocity
- Alive / Dead status

The panel updates live every tick while the tank is selected.

### Hold Left Button — Sensor View

**Hold left mouse button** on a tank to enter **sensor view** for that tank. The normal arena is replaced with a representation of what that tank knows:

- **Ghost contacts** — every entry in the tank's `RadarMap` is drawn as a semi-transparent hull at its last-known position. Contacts fade over 50 ticks as they grow stale. Direct scans (made by this tank) show a pulsing radar halo; relayed contacts (received via `RadarShare` from an ally) do not.
- **Building wall echoes** — every entry in the tank's `BuildingWallMap` is drawn as a coloured line segment. Actual building rectangles are **not** shown — only what the tank's radar has detected.
  - **Cyan** — echo recorded directly by this tank's radar.
  - **Teal (MediumAquamarine)** — echo received from an ally via `BuildingEchoShare`.
  - Both fade over 100 ticks of staleness. A small crosshair marks the nearest echo return point on each wall.
- **Focused tank** — drawn in full detail (or as a hulk/explosion if destroyed).
- All other tanks, bullets, and effects are hidden — the view shows only what the selected tank knows.

A banner at the top of the screen identifies the focused tank and whether it is held or pinned (double-click to pin).

**Double-click** to pin the sensor view to a tank so it persists without holding the button.

#### ECM Override Button

At the bottom of every attached panel is an **ECM cycle button** that steps through:

```
Auto (AI)  →  OFF  →  JAM  →  SPOOF  →  JAM+SPOOF  →  ECCM  →  Auto (AI)  →  …
```

- **Auto (AI)** — the tank's own `OnTick` logic controls ECM (default).
- Any other value forces that mode regardless of the tank AI, deducting the appropriate energy cost.
- When an override is active the button is highlighted in purple with bold text.
- The override **persists** after the panel is closed — the tank stays in the forced mode until you cycle back to Auto or the arena is reset.

### Right-click to Pin

**Right-click** a tank to pin it — the info panel follows only that tank regardless of other clicks.

---

## TankSwarmArena (Main Form)

`TankSwarmArena` (`TankSwarmCode/TankSwarmArena.cs`) is the root WinForms window. It hosts `ArenaUserControl` and provides the application shell.

| Control | Function |
|---------|----------|
| **Add Tanks** menu | Select pre-built swarms |
| **Start** button | Initialise a new round and begin the tick loop |
| **Stop** button | Pause without resetting |
| **Step** button | Advance exactly one tick (useful for debugging) |
| **Reset** button | Clear the arena and all tank instances |
| Speed slider | Adjust simulation rate |
| Arena size fields | Set width × height; applied on next round start |

### Radio Log

A scrolling panel shows swarm messages as they are delivered. Each entry shows the tick number, sender name (colour-coded by swarm), message type, and key fields. The log is capped at **300 entries** and auto-trims to 250 when the limit is reached.

---

## ArenaConfigurationUserControl

An embedded panel (`TankSwarmCode/ArenaConfigurationUserControl.cs`) lets users set the arena **width** and **height** before starting a round. Values are validated to ensure a minimum usable arena size.

---

## ScreenWakeLock

`TankSwarmCode/ScreenWakeLock.cs` calls `SetThreadExecutionState` to prevent the Windows display from sleeping while a simulation is running. The lock is acquired when the tick loop starts and released when it stops.

---

## Render Settings

| Setting | Default | Description |
|---------|---------|-------------|
| Antialiasing | On | Smooths tank and bullet edges |
| Radar reflections | On | Wavefront arcs from scanner toward contact |
| Radar sweep trails | On | Phosphor-decay arc history (hidden while jamming) |
| Scan halos | On | Expanding pulse at moment of radar contact |
| ECM effects | On | Jam/Spoof/JamAndSpoof/Burnthrough auras and ghost echo silhouettes |
| Bullets | On | Arrowhead projectiles with colour-coded tails |
| Explosions & flames | On | Burn/smoke sequences on destruction |
| Energy bars | On | Proportional health bar above each tank |
| Tank labels | On | Tank name above energy bar |
| HUD | On | Tick counter and swarm scoreboard |
| Info panels | On | Live state panel (attach via left-click) |

---

## Rendering Cycle

```
Timer fires
    │
    ▼
ArenaEngine.Tick()               — one physics step; publishes new TankState snapshots
    │
    ▼
ArenaUserControl.Invalidate()    — marks control dirty
    │
    ▼
OnPaint(PaintEventArgs e)
    │
    ├─ [Normal view — no focused tank]
    │     ├─ DrawBackground()             — black fill + border
    │     ├─ DrawAllRadarHalos()          — expanding wavefront arcs (ShowRadarReflections)
    │     ├─ DrawHulkBodies()             — charred hulks (destroyed tanks)
    │     ├─ DrawBullets()                — arrowhead projectiles (live) or fading sparks (deflected)
    │     │     └─ DrawRicochetFlash()    — one-tick impact ring at each non-lethal hit (ShowBullets)
    │     ├─ DrawGhostEchoes()            — ECM Spoof/JamAndSpoof phantoms (ShowEcmEffects)
    │     ├─ DrawTanks()                  — for each alive tank:
    │     │     ├─ DrawScanHalo()         — point-flash at contact moment (ShowScanHalos)
    │     │     ├─ Body + treads          — filled square rotated to Heading
    │     │     ├─ Cannon barrel          — 4×22 px rectangle at GunHeading (GDI+ rotation)
    │     │     ├─ Turret circle          — 10 px circle centred on hull
    │     │     ├─ DrawRadarSweepTrail()  — phosphor decay + arc flash (ShowRadarSweepTrails, hidden while jamming)
    │     │     ├─ DrawEcmAura()          — Jam/Spoof/JamAndSpoof/Burnthrough aura (ShowEcmEffects)
    │     │     └─ Energy bar + label     — (ShowEnergyBars / ShowTankLabels)
    │     ├─ DrawBuildings()              — concrete-textured rectangles with windows
    │     └─ DrawExplosions()             — blast flash, fireball, shockwave, debris, smoke
    │
    ├─ [Sensor view — focused tank held/pinned]
    │     ├─ DrawGhostContacts()          — RadarMap entries; cyan halo = direct, no halo = relayed; fade 50 ticks
    │     ├─ DrawBuildingWallEchoes()     — BuildingWallMap entries; cyan = direct, teal = relayed; fade 100 ticks
    │     └─ DrawFocusedTank()            — full tank or hulk/explosion
    │
    ├─ DrawHud()                    — tick counter, swarm scoreboard (ShowHud)
    └─ DrawAttachedPanels()         — live state info panels (ShowInfoPanels)
```

All GDI+ objects (`Pen`, `Brush`, `SolidBrush`) are cached or created once per render settings change to avoid per-frame allocation.

---

[← Building Your Own Tank](ch10-custom-tank.md) | [Table of Contents](TOC.md) | [Next: Configuration & Constants →](ch12-configuration.md)
