## Hypothesis

Encircle formation uses a hardcoded `OrbitRadius = 180.0` constant while Wolfpack uses
`config.PreferredRange = 160.0`. When Red transitions from Wolfpack to Encircle (allyCount ≥ 2×enemies
AND allyCount ≥ 3), tanks must re-orbit from 160 to 180, causing a momentary dispersal. Aligning
Encircle to use 160px should reduce transition lag and improve accuracy in cleanup scenarios.

Success criteria: Red 2-seed avg changes by ≥2pp vs 70.2% baseline; stop early if clearly negative.

## Code Changes

`SwarmCoordinator.ExecuteEncircle`: `OrbitRadius` → `config.PreferredRange` (160.0).
Reverted after 5-seed results showed net +0.3pp (not significant).

## Run Parameters

- Batch: 200 matches each seed
- Mode: `--parallel 8`
- Seeds: 1000, 2000, 3000, 4000, 5000 (ran all 5 — 2-seed was +1.5pp, borderline)
- Timeout rule: `--on-timeout energy`
- DLL: `net10.0/publish/`

## Raw Results

| Seed | Encircle@160 | Baseline (180) | Delta |
|------|-------------|----------------|-------|
| 1000 | 71.0% | 70.0% | +1.0pp |
| 2000 | 72.5% | 70.5% | +2.0pp |
| 3000 | 70.0% | 66.0% | +4.0pp |
| 4000 | 71.0% | 73.0% | -2.0pp |
| 5000 | 68.0% | 71.5% | -3.5pp |
| **Avg** | **70.5%** | **70.2%** | **+0.3pp** |

## Analysis

**REFUTED: Encircle at 160px gives net +0.3pp (noise) across 5 seeds.** 3/5 seeds positive,
2/5 seeds negative. The positive seeds (1000, 2000, 3000) gained substantially, but seeds 4000 and
5000 — which were the strongest in the MaxFP=1.0 run — regressed.

Why the mixed results:
1. **Encircle activates infrequently.** It requires allyCount ≥ enemies×2 AND allyCount ≥ 3 — this
   only happens in lopsided late-game states (e.g., 4 Red vs 1-2 Blue, or 3 Red vs 1 Blue). Most of
   the match is decided in Wolfpack mode. The Encircle orbit change has limited impact on match outcomes.

2. **180px gives more space for maneuvering.** When Red has overwhelming numbers, the wider 180px orbit
   lets tanks spread out and avoid friendly-fire/collision. At 160px, tanks may cluster tighter and
   interfere with each other's navigation.

3. **Seed variance dominates.** At 200 matches/seed, the ±4pp range observed across seeds (70.0-72.5%)
   is at the natural variance limit. A +0.3pp avg signal is indistinguishable from noise.

Insight: The Encircle strategy is a secondary formation that doesn't drive match outcomes at this Red
win rate level. The primary formation (Wolfpack at PR=160) is where >90% of match decisions occur.

## Key Findings

- **Encircle orbit radius change REFUTED: +0.3pp avg (5-seed, noise).**
- Code reverted to `OrbitRadius = 180.0` in Encircle. Wolfpack remains at 160.
- Encircle is a low-frequency cleanup formation — its orbit radius doesn't significantly affect
  match outcomes at the current competitive level.
- MaxFP and PR sweeps are exhausted. Encircle tuning is insufficient. The architecture ceiling is
  near: true parameter space is depleted within the current Wolfpack/orbit framework.
- **Next hypotheses to investigate:**
  1. Fallback strategy threshold (currently `sumEnergy / allyCount < 30.0` — may be miscalibrated
     for MaxFP=1.0 where tanks maintain higher energy)
  2. Ghost as tactical ECM disruptor (HasEcm was previously disaster due to energy drain; revisit
     now that we understand the mechanism better)
  3. Dynamic MaxFP: allow tanks to fire at MaxFP=1.0 normally but switch to MaxFP=2.0 for finisher
     shots on low-energy targets (if the API supports conditional fire power)

## Summary

REFUTED. Code reverted. Encircle orbit radius is not a meaningful lever at this competition level.
Architecture parameter space within the current framework is largely exhausted.
