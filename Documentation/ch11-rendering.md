# Chapter 11: Arena Rendering & UI

[← Building Your Own Tank](ch10-custom-tank.md) | [Table of Contents](TOC.md) | [Next: Configuration & Constants →](ch12-configuration.md)

---

## Overview

The renderer is implemented as `ArenaUserControl` (`TankSwarmCode/ArenaUserControl.cs`), a WinForms `UserControl` that hosts the `ArenaEngine` and paints each frame using GDI+. Rendering is decoupled from the simulation: the renderer reads **immutable snapshots** (`TankState`, `BulletState`) and never mutates engine state.

---

## ArenaUserControl

`ArenaUserControl` is responsible for:

1. Owning the `ArenaEngine` instance.
2. Driving the tick loop via a `System.Windows.Forms.Timer` set to the configured ticks/second rate.
3. Painting the arena (double-buffered GDI+ surface) after each tick.
4. Handling mouse input for the interactive inspection panel.

Double buffering is enabled so each completed frame is swapped in atomically — no flicker even at high tick rates.

---

## Visual Elements

### Tank Body

Each tank is drawn as a filled square (18 × 18 px by default) rotated to match `TankState.Heading`. Team colour is determined by `SwarmId`:

- SwarmId 1 (Red Swarm) → red body
- SwarmId 2 (Blue Swarm) → blue body
- SwarmId 0 or other → a neutral colour

### Cannon Barrel and Turret

The gun is rendered as a **filled 4 × 22 px rectangle** rotated to `GunHeading` using a GDI+ coordinate transform (rather than a manually computed line). This keeps the barrel crisp and sharp at every angle under anti-aliasing. The barrel extends from the tank centre outward.

A **10 px-diameter circular turret** is drawn on top of the hull body, anchoring the barrel visually and giving the classic tank silhouette. Both the barrel and turret use dark grey fills with a silver outline.

### Radar Beam and Sweep Trail

A short line (16 px) in the direction of `RadarHeading` represents the current radar pointing angle. A phosphor-decay sweep trail (arc history from the last 12 ticks) fans out behind it.

**Both the beam and trail are hidden when the tank is jamming** (`EcmMode.Jam` or `EcmMode.JamAndSpoof`). Since the radar is physically offline, no visual is shown. The trail history is also cleared when jamming begins, so the trail starts cleanly from zero when jamming ends — no ghost arc from before the jam.

### Energy Bar

A horizontal bar (36 × 4 px) above each tank shows current energy as a proportion of maximum:

- Green: energy > 50%
- Yellow: energy 25–50%
- Red: energy < 25%

### Radar Halo

When a scan detects a target, an expanding/fading circular pulse (sonar halo) is rendered at the scanning tank's position. Each halo lives for **10 ticks** (~1.7 s at the default speed), expanding outward and becoming more transparent as it ages. Multiple halos can be active simultaneously.

Not rendered for jamming tanks — since no scan is performed, no halo event is ever generated.

### Destroyed Tank Hull

When a tank's energy reaches 0, its body stops moving but remains visible as a darkened hull at `DestroyedAtTick` position. The hull persists until the round resets.

### Explosion Animation

Upon destruction, a 2.1-second burn animation plays at the tank's last position:

- Phase 1 (0–700 ms): bright orange/yellow expanding flash
- Phase 2 (700–1400 ms): contracting fire glow
- Phase 3 (1400–2100 ms): dissipating smoke (grey fading to transparent)

Multiple simultaneous explosions are each tracked independently.

### Bullets

Active bullets are drawn as arrowhead projectiles:

- **Arrowhead triangle** pointing in the travel direction, sized proportionally to power.
- **Tail shaft** extending behind the arrowhead for a velocity cue.
- **Glow halo** — a soft transparent ellipse behind the tip.
- Colour is the swarm colour of the firing tank, lightened for visibility.

#### Deflected (Ricochet) Bullets

When a bullet hits a tank without killing it, it enters a deflected state and is rendered differently for its remaining lifespan:

- Drawn as a **fading ember spark** (filled ellipse) instead of an arrowhead.
- A smaller **white hot core** overlays the ember; both shrink as speed decays.
- A short **trailing streak** shows the direction of travel, also fading.
- All elements fade to transparent as `CurrentSpeed` decays toward zero, then the bullet disappears.

#### Ricochet Flash

At the exact tick a non-lethal hit occurs, a single-frame **impact flash** is drawn at the collision point: an orange ring (~7 px radius) with a white filled centre. This is rendered from `ArenaEngine.RicochetFlashes` and lasts exactly one paint cycle.

### Buildings

Buildings are filled rectangles drawn in a muted grey/brown colour with a darker border. They are painted before tanks and bullets so they appear as background obstacles.

### ECM Auras

When a tank has an active ECM mode, a distinctive animated aura is drawn **on top of** the hull and radar trails each frame (toggle: **ECM effects**):

| Mode | Visual |
|------|--------|
| **Jam** | 24 small (3 × 3 px) orange and yellow dots scattered randomly in a ~14 px band around the hull. The entire cluster repositions ~16 times per second using a seeded hash, creating an analog-static interference look. |
| **Spoof** | A faint purple ghost copy of the tank hull that slowly orbits the tank on a tight circular path (one full orbit every 3 seconds), pulsing in opacity as it moves. Suggests the tank is projecting a decoy echo. |
| **JamAndSpoof** | Both the Jam static-dot burst **and** the Spoof orbiting ghost hull are drawn simultaneously, reflecting the combined nature of the mode. |
| **Burnthrough** | A short bright cyan arc centred on the **radar heading direction**, with a thin beam line extending from the hull edge to the arc. Both pulse in brightness on a 0.6-second cycle and rotate as the radar turns. |

### ECM Ghost Echoes

While a Spoof or JamAndSpoof tank is active, translucent phantom tank silhouettes appear at each ghost echo position (Layer 3.5 in the paint order, between bullets and live tanks). Each ghost is drawn:

- As a semi-transparent square in the spoofing swarm's colour.
- With a dashed X cross-hatch to distinguish it from real hulks.
- With a `?` label above it.
- With a flicker animation that prevents them from looking static.

Ghost echoes are rendered regardless of whether any enemy tank is actually being fooled — they show the deception field from a spectator's omniscient viewpoint.

---

## Interactive Features

### Click to Inspect

**Left-click** on any tank body to open the **info panel**, which displays:

- Tank name and swarm
- Role and ECM mode (Off / JAM ⚡ / SPOOF 👻 / JAM+SPOOF / ECCM 📶)
- Current energy
- Position (X, Y)
- Headings (body, gun, radar)
- Velocity
- Alive / Dead status

The panel updates live every tick while the tank is selected.

#### ECM Override Button

At the bottom of every attached panel is an **ECM cycle button**. Clicking it steps through:

```
Auto (AI)  →  OFF  →  JAM  →  SPOOF  →  JAM+SPOOF  →  ECCM  →  Auto (AI)  →  …
```

- **Auto (AI)** — the tank's own `OnTick` logic controls ECM (default).
- Any other value forces that mode regardless of what the tank AI requests, deducting the appropriate energy cost each tick.
- When an override is active the button is highlighted in purple with bold text.
- The override **persists** when the panel is closed — the tank keeps the forced mode until you cycle back to Auto or the arena is reset.

### Right-click to Pin

**Right-click** a tank to pin it — the info panel follows only that tank regardless of other clicks.

### Double-click to Follow

**Double-click** a tank to lock the viewport (if scrolling is enabled) on that tank.

---

## TankSwarmArena (Main Form)

`TankSwarmArena` (`TankSwarmCode/TankSwarmArena.cs`) is the root WinForms window. It hosts `ArenaUserControl` and provides the application shell.

### Controls

| Control | Function |
|---------|----------|
| **Add Tanks** menu | Select pre-built swarms or custom tank classes |
| **Start** button | Initialise a new round and begin the tick loop |
| **Stop** button | Pause the simulation without resetting |
| **Step** button | Advance exactly one tick (useful for debugging) |
| **Reset** button | Clear the arena and all tank instances |
| Speed slider | Adjust simulation rate (1 tick/s to unlimited) |
| Arena size fields | Set width × height; applied on next round start |

### Radio Log

A scrolling panel shows swarm messages as they are delivered. Each entry includes:

- Tick number
- Sender name (colour-coded by swarm)
- Message type
- Key fields (target name, position if present)

The log is capped at **300 entries**; it auto-trims to 250 when the limit is reached.

---

## ArenaConfigurationUserControl

A small embedded form (`TankSwarmCode/ArenaConfigurationUserControl.cs`) lets users set the arena **width** and **height** before starting a round. Values are validated to ensure a minimum usable arena size.

---

## ScreenWakeLock

`TankSwarmCode/ScreenWakeLock.cs` calls `SetThreadExecutionState` to prevent the Windows display from sleeping while a simulation is running. The lock is acquired when the tick loop starts and released when it stops.

---

## Render Settings

The following rendering options can be adjusted at runtime (via properties on `ArenaUserControl`):

| Setting | Default | Description |
|---------|---------|-------------|
| Antialiasing | On | Smooths tank and bullet edges |
| Radar reflections | On | Expanding wavefront arcs from scanner to contact |
| Radar sweep trails | On | Phosphor-decay arc history behind the radar beam (hidden while tank is jamming) |
| Scan halos | On | Point-flash at the moment radar contact is made |
| **ECM effects** | **On** | **Jam/Spoof/JamAndSpoof/Burnthrough auras and ghost echo silhouettes** |
| Bullets | On | Arrowhead projectiles with colour-coded tails |
| Explosions & flames | On | Burn/smoke sequences on destruction |
| Energy bars | On | Proportional health bar above each tank |
| Tank labels | On | Tank name above energy bar |
| HUD | On | Tick counter and swarm scoreboard |
| Info panels | On | Live state panel (attach via right-click context menu) |

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
    ├─ DrawBackground()          — black fill + border
    ├─ DrawAllRadarHalos()       — expanding wavefront arcs (if ShowRadarReflections)
    ├─ DrawHulkBodies()          — charred hulks (destroyed tanks)
    ├─ DrawBullets()             — arrowhead projectiles (live) or fading sparks (deflected)
    │     └─ DrawRicochetFlash() — one-tick impact ring at each non-lethal hit (if ShowBullets)
    ├─ DrawGhostEchoes()         — ECM Spoof/JamAndSpoof phantoms (if ShowEcmEffects)
    ├─ DrawTanks()               — for each alive tank:
    │     ├─ DrawScanHalo()      — point-flash at contact (if ShowScanHalos)
    │     ├─ Body + treads       — filled square rotated to Heading
    │     ├─ Cannon barrel       — 4×22 px filled rectangle at GunHeading (GDI+ rotated)
    │     ├─ Turret circle       — 10 px circle centred on hull, on top of barrel
    │     ├─ DrawRadarSweepTrail()  — phosphor decay + scan-arc flash (if ShowRadarSweepTrails AND not jamming)
    │     ├─ DrawEcmAura()       — Jam/Spoof/JamAndSpoof/Burnthrough aura (if ShowEcmEffects)
    │     └─ Energy bar + label  — (if ShowEnergyBars / ShowTankLabels)
    ├─ DrawBuildings()           — concrete-textured rectangles with windows
    ├─ DrawExplosions()          — blast flash, fireball, shockwave, debris, smoke
    ├─ DrawHud()                 — tick counter, swarm scoreboard (if ShowHud)
    └─ DrawAttachedPanels()      — live state info panels (if ShowInfoPanels)
```

All GDI+ objects (`Pen`, `Brush`, `SolidBrush`) are cached or created once per render settings change to avoid per-frame allocation.

---

[← Building Your Own Tank](ch10-custom-tank.md) | [Table of Contents](TOC.md) | [Next: Configuration & Constants →](ch12-configuration.md)
