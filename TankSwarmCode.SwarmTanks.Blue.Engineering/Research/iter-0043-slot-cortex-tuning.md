# Iter 43 — Slot Cortex Tuning Sweep (REFUTED — both changes regressed)

## Hypothesis

With 29v28 established at ~55%, upgrading slot tank DPS and lowering Encircle threshold should
push Blue above 55%. Two changes tested independently:

1. BlueSlotCortex MaxFP: 1.0→2.5, PR: 160→200 (match named tank capability)
2. Encircle threshold: `enemies.Count * 2` → `enemies.Count + 2` (trigger with 2-tank lead)

## Code Changes

Tested but reverted:
- BlueSlotCortex.cs: MaxFP=2.5, PR=200.0 (reverted to 1.0/160)
- SwarmCoordinator.cs: `enemies.Count + 2` Encircle condition (reverted to `enemies.Count * 2`)

## Run Parameters

- 200 matches per seed, parallel 8, on-timeout energy
- Seeds: 1000, 2000, 3000, 5000

## Raw Results

### Baseline (iter-42, MaxFP=1.0, PR=160, enemies*2 Encircle):

| Seed | Blue % |
|------|--------|
| 1000 | 58%    |
| 2000 | 50%    |
| 3000 | 56%    |
| 5000 | 55%    |
| **Avg** | **54.75%** |

### MaxFP=2.5, PR=200 (slot cortex upgrade):

| Seed | Red % | Blue % | Delta |
|------|-------|--------|-------|
| 1000 | 43%   | 57%    | -1pp  |
| 2000 | 54%   | 46%    | -4pp  |
| 3000 | 52%   | 48%    | -8pp  |
| 5000 | 47%   | 53%    | -2pp  |
| **Avg** | | **51%** | **-4pp** |

### Encircle enemies+2 threshold:

| Seed | Red % | Blue % | Delta |
|------|-------|--------|-------|
| 1000 | 44%   | 56%    | -2pp  |
| 2000 | 50%   | 50%    | 0pp   |
| 3000 | 48%   | 52%    | -4pp  |
| 5000 | 48%   | 52%    | -3pp  |
| **Avg** | | **52.5%** | **-2.5pp** |

## Analysis

### MaxFP=2.5/PR=200 regresses (-4pp)
Higher MaxFP means slower bullets (12.5px/tick vs 17px/tick). At 200px range, flight time 
increases from ~11 ticks to ~16 ticks — much more prediction error. Red's own slot tanks use 
MaxFP=1.0/PR=160 for exactly this reason. Same conclusion applies to Blue.

### Encircle enemies+2 regresses (-2.5pp)
The `enemies*2` threshold (2:1 ratio) is the historical optimum. Lowering to `+2` triggers
Encircle when Blue has only 2 extra tanks. In 29v28 scenarios with ~equal numbers, premature
Encircle commits tanks to orbit positions before Blue has the kill-speed advantage needed.
This lets Red's Wolfpack concentrate fire while Blue tanks are navigating to orbit points.

### 55% is the current ceiling
Both parameter-level improvements failed. The 55% result at 29v28 with default configs is the
architectural ceiling for numerical-superiority-based approach.

## Key Findings

- **MaxFP=1.0/PR=160 is optimal for slot tanks** — even at 29v28, heavier bullets hurt more than they help
- **Encircle enemies*2 threshold is optimal** — requires true 2:1 ratio before committing to orbit
- **55% ceiling for 29v28 numerical approach** — no parameter-level improvement found

## Summary

**REFUTED: Both config changes regressed. 55% remains the ceiling for current tactical approach.**
Next: explore structural changes — different targeting priority, Pincer threshold adjustment,
or target Red's weakest known tank type rather than lowest-energy.
