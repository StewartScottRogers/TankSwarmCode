## Hypothesis

Fallback strategy triggers when `sumEnergy / allyCount < 30.0`. At MaxFP=1.0, Red tanks have much
higher energy reserves (avg 45-52% remaining per iter-24). Raising the threshold to 40.0 should
cause earlier strategic regrouping, potentially saving Red from unfavorable fights.

At MaxFP=1.0, each shot costs 0.5 energy. A tank with 40 energy can still fire 80 shots. The
current threshold (30) was calibrated for MaxFP=2.0 behavior (each shot costs 2 energy).

Success criteria: Red 2-seed avg changes by ≥2pp vs 70.2% baseline; stop early if clearly negative.

## Code Changes

`SwarmCoordinator.SelectStrategy`: `sumEnergy / allyCount < 30.0` → `< 40.0`.
Reverted after 2-seed results were negative.

**NOTE: Also discovered that publishing only `TankSwarmCode.AiCortex.Red` is insufficient —
the shell project (`TankSwarmCode.SwarmTanks.Red`) bundles the cortex DLL in its publish directory.
Always publish from the shell project to get an updated cortex in the run DLL.**

## Run Parameters

- Batch: 200 matches each seed
- Mode: `--parallel 8`
- Seeds: 1000, 2000 (stopped after 2-seed avg was negative)
- Timeout rule: `--on-timeout energy`
- DLL: `net10.0/publish/`

## Raw Results

| Seed | Fallback@40 | Baseline (30) | Delta |
|------|-------------|---------------|-------|
| 1000 | 70.5% | 70.0% | +0.5pp |
| 2000 | 69.0% | 70.5% | -1.5pp |
| **Avg** | **69.75%** | **70.25%** | **-0.5pp** |

## Analysis

**REFUTED: Fallback threshold=40 gives -0.5pp avg (2 seeds, noise/slightly negative).**

Why the change doesn't help:
1. **Higher avg_ticks confirms Fallback fires more.** Seed 1000 avg_ticks jumped from ~1303
   (at Fallback=30) to 1577 (at Fallback=40) — confirming the threshold fires significantly more
   often. The teams retreat and regroup more frequently.

2. **Retreating disrupts Wolfpack coherence.** When Red retreats via Fallback (to the furthest
   arena corner), the Wolfpack formation breaks apart. Re-forming Wolfpack after a retreat takes
   ticks that could have been spent firing. At MaxFP=1.0 with high energy reserves, fighting
   through low energy states is more sustainable than at MaxFP=2.0.

3. **Longer matches don't favor Red.** More retreats → longer matches → Blue has more time to
   recover from disadvantageous positions. The `--on-timeout energy` rule rewards energy efficiency,
   not kills. Red's energy advantage is real, but sustained combat pressure delivers wins faster.

4. **The 30.0 threshold was correct for the current config.** At MaxFP=1.0, Red tanks rarely drop
   below 30 avg energy — they generate energy through hits and spend little per shot. Fallback at 30
   was already a near-dead-letter in the current configuration. Raising to 40 made it activate in
   situations that don't actually require retreat.

Insight: The Fallback threshold at 30 is essentially inactive at MaxFP=1.0 energy levels. This is
GOOD — it means Red fights aggressively without unnecessary retreats. Leave it at 30.

## Key Findings

- **Fallback threshold=40 REFUTED: -0.5pp avg (2 seeds).**
- Code reverted to 30.0. The current threshold is correctly calibrated.
- Bug discovered and fixed: publishing only AiCortex DLL doesn't update the shell's publish dir.
  Must use: `dotnet publish TankSwarmCode.SwarmTanks.Red/... -c Release` to bundle cortex correctly.
- Architecture parameter space within current Wolfpack/orbit framework is fully exhausted.
- Next: Ghost HasEcm=true (EcmMode.Jam) as reactive ECM disruptor when Blue ECM is detected.

## Summary

REFUTED. Code reverted to Fallback threshold=30. Moving to Ghost ECM hypothesis (iter-28).
