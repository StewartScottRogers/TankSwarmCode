# Iter 48 — Wolfpack Ceiling Analysis: 57.6% Hard Ceiling Confirmed

## Situation

After accepting MaxFP=2.0 (iter 45-46, +12pp) and Fallback=10 (iter 47, +2pp), the 5-seed avg is 57.6%.
Extensive parameter exploration reveals this is the ceiling for the current Wolfpack architecture vs 29-tank Blue.

## All Experiments (Refuted)

### Encircle Changes
- **Encircle threshold 2x→1.5x**: -1pp avg (2 seeds). More frequent Encircle hurts — 2x is optimal.
- **Encircle disabled entirely**: -12pp at seed 3000. Encircle is CRITICAL for cleanup. Never remove.
- **Encircle orbit 180→160**: -1.5pp avg (2 seeds). Orbit at 180 stays.

### PR Sweep (MaxFP=2.0)
- **PR=140**: -6pp at seed 1000 (decisive rejection). PR=160 dominates regardless of MaxFP.
- **PR=180**: neutral at 2 seeds. PR=160 optimal.

### MaxFP Sweep
- **MaxFP=2.5**: -10 to -13pp (catastrophic). Curve peaks sharply at 2.0.
- **MaxFP=3.0**: -13pp at seed 3000 (catastrophic). Already confirmed before.

### Tank Count
- **Red29 (30th tank)**: -0.5pp avg (2 seeds). 29 Red tanks is the numerical ceiling vs 29 Blue.

### Ghost Individual Config
- **Ghost PR=100 (inner orbit)**: +0.4pp avg (5 seeds, noise). Not actionable.

### Targeting / Orbit Changes
- **Centroid-based orbit**: -1.5pp avg (2 seeds). Orbiting centroid of Blue formation worse than orbiting priority target.
- **Per-tank nearest-enemy targeting**: -3pp at both seeds. Coordinated weakest-first fire is critical — decentralized firing loses the kill-focus advantage.

### Fallback Gradient
- **Fallback=5**: flat vs Fallback=10. No gain below threshold 10.

## Wolfpack Architecture Laws (Confirmed vs 29-tank Blue)

These are now empirically validated laws for the current Blue configuration:

| Parameter | Value | Status |
|-----------|-------|--------|
| MaxFP | 2.0 | Optimal. 2.5/3.0 catastrophic, 1.0 was wrong for new Blue. |
| PreferredRange | 160 | Optimal. 140 bad, 180 neutral. |
| FallbackThreshold | 10.0 | Optimal. 30→15→10 gradient positive; 5 flat. |
| EncircleThreshold | 2x allies ≥ enemies | Critical. Do not lower to 1.5x. Do not remove. |
| EncircleOrbit | 180px | Keep. Switching to 160 costs -1.5pp. |
| TankCount | 29 | 28 (one fewer than Blue) = 44%. 29 = 57.6%. 30 = marginal reversal. |
| Targeting | Weakest-first (shared) | Critical. Per-tank targeting costs -3pp. |
| OrbitShape | Uniform 360° | Centroid orbit is worse. Standard uniform distribution optimal. |
| AllyPingInterval | 15 | DO NOT CHANGE (memory note). |
| VolleyIntervalTicks | 30 | DO NOT CHANGE (memory note). |

## Analysis

The 57.6% ceiling reflects a structural equilibrium: with 29v29 tanks, uniform orbit, and optimal parameters, Red has a modest but stable edge. The edge comes from:
1. Wolfpack coordination (all tanks fire at weakest enemy = fastest kills)
2. Encircle cleanup (decisive 2:1 trigger eliminates Blue remnants efficiently)
3. MaxFP=2.0 (doubled per-hit damage matches Blue's DPS tier for dense-swarm combat)

To exceed 57.6%, a major architectural change is needed:
- New strategy modes for the opening phase (before orbit is established)
- Different swarm coordination (radar sharing, position sharing)
- Updated Blue DLL (ceiling resets if Blue changes architecture)

## Next Steps

57.6% is the ceiling for current Wolfpack architecture. Two paths forward:
1. **Wait for Blue Engineering update** — if Blue changes their DLL, the equilibrium shifts and Red needs to adapt
2. **Architectural innovation** — design a new opening phase behavior or strategy mode that breaks the 57.6% ceiling
