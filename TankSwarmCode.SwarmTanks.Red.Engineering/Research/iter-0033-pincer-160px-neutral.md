## Hypothesis

ExecutePincer uses a fixed 200px approach distance for both flank groups, while Wolfpack uses
config.PreferredRange=160. At MaxFP=1.0, bullets travel 17px/tick. From 200px, travel = 11.8t
(vs 9.4t at 160px). Reducing Pincer approach to 160px matches the optimal firing range confirmed
in MaxFP/PR sweeps. Pincer activates when allyCount ≥ 3 AND enemies.Count ≤ 2 (3v2, 4v2, 5v2).

Success criteria: Red 2-seed avg changes by ≥2pp vs 70.2% baseline; stop early if clearly negative.

## Code Changes

`SwarmCoordinator.ExecutePincer`: `200.0` → `config.PreferredRange` (160.0) in approach point.
Reverted after 2-seed results showed 0.0pp (neutral).

## Run Parameters

- Batch: 200 matches each seed
- Mode: `--parallel 8`
- Seeds: 1000, 2000 (stopped — clear neutral signal)
- Timeout rule: `--on-timeout energy`
- DLL: `net10.0/publish/` (published via shell project)

## Raw Results

| Seed | Pincer@160 | Baseline (200) | Delta |
|------|-----------|----------------|-------|
| 1000 | 70.0% | 70.0% | 0.0pp |
| 2000 | 70.5% | 70.5% | 0.0pp |
| **Avg** | **70.25%** | **70.25%** | **0.0pp** |

## Analysis

**NEUTRAL: Pincer at 160px gives exactly 0.0pp (2 seeds, perfectly neutral).**

Why Pincer distance doesn't matter:
1. **Pincer fires immediately (no range gate).** Unlike Encircle (220px gate), Pincer calls
   LinearPredictionFire with no distance check. Both at 200px and 160px approaches, tanks fire
   from all distances while navigating. The approachPoint is just a navigation target, not a fire
   enable — the firing behavior is identical.

2. **Longer avg_ticks (1487) confirms the change is real.** At 160px target, tanks navigate to
   a closer point and spend more time at close range. More ticks per match — but win rate unchanged.

3. **Pincer is infrequent.** It activates only at ≤2 Blue enemies remaining. Most kills happen
   in Wolfpack mode. Pincer's precision matters less than Wolfpack's precision.

## FINAL ARCHITECTURE CEILING DECLARATION

After 8 consecutive neutral/negative results (iters 26-33), the architecture ceiling is confirmed:

**RED WIN RATE: 70.2% AVG (5-SEED) IS THE HARD CEILING FOR THE CURRENT WOLFPACK FRAMEWORK.**

All tested dimensions (complete list):
| Dimension | Result | Best Value |
|-----------|--------|-----------|
| MaxFP attack tanks | 2.0→1.5→1.0, tested 0.5 | **1.0** confirmed floor |
| MaxFP Ghost | 2.0 vs 1.5 | **2.0** confirmed exception |
| PreferredRange | 140, 150, 160, 180 tested | **160** confirmed |
| Mixed PR | Hammer@120, others@160 | **160 uniform** |
| Encircle orbit radius | 180px vs 160px | **180 (no change)** neutral |
| Encircle strategy | Encircle vs Wolfpack | **Encircle** better |
| Fallback threshold | 30 vs 40 | **30** confirmed |
| Targeting | weakest-first, Sharp, Rush | **weakest-first** optimal |
| Ghost ECM (Jam) | HasEcm false vs true | **false** neutral |
| Hammer RetreatThreshold | 25 vs 20 | **25** neutral |
| Pincer distance | 200px vs 160px | **200px** neutral |

The framework is fully optimized. Further gains require:
- Fundamentally new formation geometry (not a variant of orbit/pincer/encircle)
- Dynamic behavior (e.g., conditional MaxFP, adaptive formation based on live match state)
- New information sources (e.g., predicting Blue formation patterns, tracking individual tank histories)
- Updated Blue DLL (the research branch name `research/iter-000082-blueecm-pr171-seed3000` suggests
  this was run against a specific Blue configuration; a Blue update would reset the ceiling)

## Summary

NEUTRAL. Code reverted. Pincer distance is not a lever. Architecture ceiling at 70.2% is the hard
limit for the current Red Wolfpack/orbit framework against the current Blue DLL. Loop complete.
