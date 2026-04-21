## Hypothesis

Arrow, Trooper, Blade, and Hammer all showed improvement at MaxFP=1.5 vs 2.0 at PR=160. Ghost is the
last tank at MaxFP=2.0. Applying the same change should complete the sweep and add ~1pp.

Success criteria: Red 5-seed avg increases by ≥1pp (from 67.2% to ≥68.2%).

## Code Change

`TankSwarmCode.AiCortex.Red/RedGhostCortex.cs`: MaxFirePower 2.0 → 1.5 (reverted after refutation)

## Run Parameters

- Batch: 200 matches each seed
- Mode: `--parallel 8`
- Seeds: 1000, 2000, 3000, 4000, 5000
- Timeout rule: `--on-timeout energy`
- DLL: `net10.0/publish/`

## Raw Results

| Seed | Ghost=1.5 | Baseline (Ghost=2.0) | Delta |
|------|-----------|----------------------|-------|
| 1000 | 65.0% | 66.0% | -1.0pp |
| 2000 | 68.5% | 72.0% | -3.5pp |
| 3000 | 65.5% | 69.0% | -3.5pp |
| 4000 | 63.0% | 66.0% | -3.0pp |
| 5000 | 66.5% | 63.0% | +3.5pp |
| **Avg** | **65.7%** | **67.2%** | **-1.5pp** |

## Analysis

4/5 seeds show regression (-1pp to -3.5pp). Only seed 5000 is positive (+3.5pp). The 5-seed average
dropped from 67.2% to 65.7%, a clear -1.5pp regression.

Ghost does NOT benefit from MaxFP=1.5. Despite fighting at a moderate combat rate (~5-9/100t across
seeds), lowering Ghost's fire power hurts win rate. Ghost's role may differ: as the orbit slot that
sometimes targets from a wider angle or engages while already near energy limits, the higher damage
per shot (2.0) matters more for Ghost's effectiveness.

**Why Ghost is different from the other tanks:**
Ghost has HasEcm=false but is assigned the ECM role by the arena (observed in match output). Ghost
may fire opportunistically rather than sustained-burst, making per-shot damage more important than
bullet speed. At MaxFP=2.0, each shot deals more damage on an opportunistic hit. At 1.5, more shots
fire but at less impact per hit, which doesn't help Ghost's tactical role.

## Key Findings

- **Ghost MaxFP=1.5 REFUTED: -1.5pp avg (5-seed).** Ghost stays at MaxFP=2.0.
- The "faster bullets at PR=160" pattern applies to 4/5 tanks (Arrow, Trooper, Blade, Hammer).
  Ghost is the exception — MaxFP=2.0 is optimal for Ghost.
- The MaxFP sweep is now complete. Final config: all 5 tanks at 1.5 EXCEPT Ghost (2.0).
- New baseline confirmed at **67.2%** avg (5-seed).

## Summary

REFUTED. Ghost reverted to MaxFP=2.0. Baseline remains 67.2%. Next: strategic targeting changes.
