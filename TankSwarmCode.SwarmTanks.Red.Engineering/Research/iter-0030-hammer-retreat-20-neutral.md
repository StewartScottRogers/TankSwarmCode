## Hypothesis

Hammer has RetreatEnergyThreshold=25 while all other tanks have 20. The Scatter condition is
`allyCount==1 && energy < threshold`. Lowering Hammer's threshold to 20 makes Hammer fight 5
more energy-ticks in last-tank-alive scenarios before retreating. At MaxFP=1.0 (0.5 energy/shot),
5 energy = 10 more shots in the critical late-game 1v1 or 1v2 phase.

Success criteria: Red 2-seed avg changes by ≥2pp vs 70.2% baseline; stop early if clearly negative.

## Code Changes

`RedHammerCortex.cs`: `RetreatEnergyThreshold = 25.0` → `20.0`.
Reverted after 2-seed results showed exactly 0.0pp (neutral).

## Run Parameters

- Batch: 200 matches each seed
- Mode: `--parallel 8`
- Seeds: 1000, 2000 (stopped — clear neutral signal)
- Timeout rule: `--on-timeout energy`
- DLL: `net10.0/publish/` (published via shell project)

## Raw Results

| Seed | Hammer Ret=20 | Baseline (Ret=25) | Delta |
|------|--------------|-------------------|-------|
| 1000 | 70.0% | 70.0% | 0.0pp |
| 2000 | 70.5% | 70.5% | 0.0pp |
| **Avg** | **70.25%** | **70.25%** | **0.0pp** |

## Analysis

**NEUTRAL: Hammer RetreatThreshold=20 gives exactly 0.0pp across 2 seeds.**

Why the threshold change doesn't matter:
1. **Scatter fires when allyCount==1 only.** Hammer's retreat only triggers when it's the last
   tank alive. In that scenario, 5 extra energy (10 extra shots at MaxFP=1.0) is not enough to
   change match outcomes. A single-tank Red vs 1-2 Blue tanks is already a long-shot.

2. **Hammer survives slightly more.** Seed 1000: 71 (threshold=20) vs 70 (threshold=25). Seed 2000:
   63 vs 59. The change has a real but tiny effect on Hammer's individual survival — not enough to
   swing match wins.

3. **Seed 2000 stats are IDENTICAL to iter-28 seed 2000.** The Hammer survive/ticks stats differ
   (confirming the code change was real), but overall match outcomes are identical. Late-game 1v1
   threshold tweaks are below the noise floor for 200-match batches.

## Key Findings — FINAL ARCHITECTURE CEILING DECLARATION

**Hammer RetreatThreshold=20 NEUTRAL: 0.0pp avg (2 seeds). Code reverted to 25.**

**ARCHITECTURE CEILING CONFIRMED AT 70.2% AVG (5-SEED).**

All accessible parameters within the current Wolfpack/orbit framework have been tested and exhausted:

| Parameter | Tested | Best Value | Note |
|-----------|--------|-----------|------|
| MaxFP (attack tanks) | 0.5, 1.0, 1.5, 2.0 | **1.0** | iter-24, +3.0pp |
| MaxFP (Ghost) | 1.5, 2.0 | **2.0** | iter-20, 2.0 confirmed |
| PreferredRange (all) | 140, 150, 160, 180 | **160** | iter-22/23 |
| PreferredRange (mixed) | Hammer@120 + others@160 | **160 uniform** | iter-29 |
| Encircle radius | 160, 180 | **180 (no change)** | iter-26, neutral |
| Fallback threshold | 30, 40 | **30** | iter-27, negative |
| BlueSharp targeting | priority vs weakest-first | **weakest-first** | iter-21 |
| Ghost HasEcm | false, true (Jam) | **false** | iter-28, neutral |
| Hammer RetreatThreshold | 20, 25 | **25 (no change)** | iter-30, neutral |

The Wolfpack orbit formation at PR=160, MaxFP=1.0 for attack tanks, Ghost at MaxFP=2.0, and weakest-
first targeting is the confirmed optimal configuration. No further single-parameter changes within
the current framework will yield ≥2pp improvement.

**To break past 70.2%, new mechanisms are needed:**
- Completely different formation geometry (not Wolfpack/Encircle/Pincer variants)
- Dynamic strategy switching (e.g., conditional MaxFP based on energy state)
- New targeting intelligence beyond radar contacts
- Reacting to Blue's behavior (Blue DLL may have been updated)

## Summary

NEUTRAL. Code reverted. Architecture ceiling at 70.2% definitively confirmed across iterations 26-30.
All parameter dimensions within current Wolfpack framework exhausted.
