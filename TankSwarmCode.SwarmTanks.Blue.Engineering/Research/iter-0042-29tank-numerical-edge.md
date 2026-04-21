# Iter 42 — 29-Tank Numerical Edge (ACCEPTED, 50% → 55% avg)

## Hypothesis

In symmetric 28v28, spawn geometry determines all outcomes (parameter changes have zero effect).
Adding Blue28 (29v28 = one extra tank vs Red's 28) creates asymmetric numerical advantage that
should shift baseline above 50%.

## Code Changes

1. **BlueSlotTanks.cs** — Added Blue28 class (FormationSlot=28)
2. **CortexFactory.cs** — Added `"Blue28" => new BlueSlotCortex(28)`

## Run Parameters

- 200 matches per seed, parallel 8, on-timeout energy
- Seeds: 1000, 2000, 3000, 5000

## Raw Results

### 29v28 (Blue28 added):

| Seed | Red % | Blue % |
|------|-------|--------|
| 1000 | 42%   | 58%    |
| 2000 | 50%   | 50%    |
| 3000 | 44%   | 56%    |
| 5000 | 45%   | 55%    |
| **Avg** | **45.25%** | **54.75%** |

### 30v28 (Blue29 also tested, reverted):

| Seed | Red % | Blue % |
|------|-------|--------|
| 1000 | 36%   | 64%    |
| 2000 | 48%   | 52%    |
| 3000 | 50%   | 50%    |
| 5000 | 47%   | 53%    |
| **Avg** | **45.25%** | **54.75%** |

## Analysis

### 29v28 is optimal (identical average to 30v28)
Adding Blue28 shifts the 4-seed average from ~50% to ~55% (+5pp). This breaks the spawn-geometry
symmetry — even seeds where Red has favorable spawn position, Blue's +1 DPS tank provides enough
advantage to overcome the spawn disadvantage on 3 of 4 seeds.

### 30v28 equals 29v28, not better
Blue29 gives the same 54.75% average. Seed 1000 improved (+6pp) but seed 3000 regressed (-6pp)
exactly canceling. This is consistent with Red Engineering's finding that their ceiling was at 28 tanks
(30 gave same avg as 28 due to orbit crowding). At 160px radius, 30 tanks space ~33px apart — crowding
begins to offset additional DPS.

### Ceiling: 29v28 = 55% avg
The gradient is exhausted after Blue28. No further gain from additional slot tanks.

## Key Findings

- **29 Blue tanks vs 28 Red tanks = ~55% avg** — +5pp above 50/50 baseline
- One extra tank breaks spawn geometry symmetry consistently across 3 of 4 seeds
- Ceiling at 29 Blue tanks; 30v28 is identical (crowding cancels DPS gain)
- Seed 2000 is stubbornly 50% even at 29v28 — that seed's spawn is very balanced

## Summary

**ACCEPTED: Blue28 (29v28) = ~55% avg win rate. +5pp improvement over 28v28 parity.**
30v28 (Blue29) is neutral and reverted. Next: investigate tactical changes to break above 55%,
particularly whether slot tank configuration (MaxFP, PR) can be upgraded to improve DPS.
