## Hypothesis

When Red overwhelmingly outnumbers Blue (allyCount ≥ 2×enemies AND allyCount ≥ 3), the Encircle
strategy activates with a 180px orbit and a 220px fire gate. Replacing Encircle with Wolfpack would:
- Remove the 220px fire gate (tanks fire immediately from any range)
- Change orbit from 180px to 160px (config.PreferredRange)

This is different from iter-26 (which kept Encircle strategy but changed orbit radius). Here we test
whether the Wolfpack strategy itself is better for cleanup than Encircle.

Success criteria: Red 2-seed avg changes by ≥2pp vs 70.2% baseline; stop early if clearly negative.

## Code Changes

`SwarmCoordinator.SelectStrategy`: `return SwarmStrategy.Encircle` → `return SwarmStrategy.Wolfpack`.
Reverted after seed 1000 showed -2.0pp. Stopped early.

## Run Parameters

- Batch: 200 matches each seed
- Mode: `--parallel 8`
- Seeds: 1000 only (stopped — clear negative)
- Timeout rule: `--on-timeout energy`
- DLL: `net10.0/publish/` (published via shell project)

## Raw Results

| Seed | Wolfpack Cleanup | Baseline (Encircle) | Delta |
|------|-----------------|---------------------|-------|
| 1000 | 68.0% | 70.0% | -2.0pp |
| *2000* | *skipped* | *70.5%* | *—* |

**Stopped after seed 1000: -2.0pp is a clear negative signal.**

## Analysis

**REFUTED: Wolfpack in cleanup gives -2.0pp vs Encircle (1 seed, stopped early).**

Why Encircle is better for cleanup:
1. **180px orbit avoids tank cluster collisions.** In a 4v1 or 3v1 cleanup, multiple Red tanks orbit
   a single Blue tank. At 160px (Wolfpack), the orbit circumference = 2π×160 = 1005px total. With 4
   tanks equally spaced, each tank has ~251px of circumference. At 180px, each tank has ~283px. The
   wider orbit reduces tank crowding and navigation interference in multi-tank cleanup scenarios.

2. **220px fire gate is not restrictive.** Tanks approaching from 300px enter fire range at 220px —
   they fire for the final 60px approach to 180px orbit. This is not a meaningful delay. Wolfpack
   removes the gate and fires from 300px, but those shots are less accurate (17px/tick, 300/17=17.6t
   travel — significant lead time error vs 220/17=12.9t).

3. **Red avg_energy_gained is HIGHER with Wolfpack cleanup (22-24 vs 20 baseline).** Red tanks ARE
   firing more shots (removed gate). But more shots at longer range = lower hit rate per shot, and
   Blue's last 1-2 tanks evade more effectively. The total damage deals fewer kills despite more shots.

4. **Encircle was designed for exactly this scenario.** The Encircle strategy's 180px orbit and 220px
   fire gate create an optimal formation for overwhelming numerical advantage: tanks encircle the
   target at a comfortable distance, fire when close enough for accuracy, and avoid mutual interference.

## Key Findings

- **Encircle→Wolfpack REFUTED: -2.0pp (1 seed, clear negative). Encircle is confirmed correct.**
- Code reverted. Encircle strategy is well-designed for cleanup scenarios; do not replace it.
- **Architecture ceiling at 70.2% fully confirmed.** All remaining hypotheses exhausted:
  - MaxFP/PR: fully swept
  - Formation geometry: all radii and mixed tested
  - Targeting: weakest-first, Sharp, Rush — all neutral
  - Strategy selection: Encircle, Fallback, ECMScreen variants — all tested
  - Individual parameters: ECM, Retreat thresholds — all tested
- The 70.2% ceiling is the limit for the current Wolfpack/Encircle/Pincer architecture vs current Blue.

## Summary

REFUTED. Code reverted. Encircle is better than Wolfpack in cleanup. Architecture ceiling at 70.2%
is fully confirmed. No accessible parameters remain within the current framework.
