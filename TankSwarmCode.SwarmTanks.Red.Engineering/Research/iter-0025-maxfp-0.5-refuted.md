## Hypothesis

MaxFP sweep: 1.0→0.5 — does the "faster bullets" gradient continue below 1.0?
At MaxFP=0.5: bullet speed = 20 - 3×0.5 = 18.5 px/tick. At PR=160: travel = 160/18.5 = 8.6 ticks.
Marginal speed improvement over 1.0 (9.4 ticks). Cost: 2 damage/shot vs 4 at 1.0 (-50%).
Ghost stays at MaxFP=2.0 (confirmed optimal).

Success criteria: Red 2-seed avg changes by ≥2pp vs 70.2% baseline; stop early if clearly negative.

## Code Changes

4 cortex files changed: `MaxFirePower = 1.0` → `0.5` for Arrow, Blade, Hammer, Trooper.
Ghost remains at MaxFirePower=2.0 (unchanged).

**Reverted after seed 1000 showed -6.0pp.** All files restored to MaxFP=1.0.

## Run Parameters

- Batch: 200 matches each seed
- Mode: `--parallel 8`
- Seeds: 1000 only (stopped early — clear negative)
- Timeout rule: `--on-timeout energy`
- DLL: `net10.0/publish/`

## Raw Results

| Seed | MaxFP=0.5 | Baseline (1.0) | Delta |
|------|-----------|----------------|-------|
| 1000 | 64.0% | 70.0% | -6.0pp |
| *2000* | *skipped* | *70.5%* | *—* |

**Stopped after seed 1000: -6.0pp is a clear, decisive negative signal.**

## Analysis

**REFUTED: MaxFP=0.5 is significantly worse than 1.0 at PR=160.** The "faster bullets" gradient
that was positive from 2.0→1.5→1.0 does NOT continue to 0.5.

Why the gradient reverses at 0.5:
1. **Diminishing speed returns.** The tick advantage decreases with each step:
   - 2.0→1.5: 14.0→15.5 px/tick (+1.5, travel 160/14=11.4t → 10.3t, saves 1.1t)
   - 1.5→1.0: 15.5→17.0 px/tick (+1.5, travel 10.3t → 9.4t, saves 0.9t)
   - 1.0→0.5: 17.0→18.5 px/tick (+1.5, travel 9.4t → 8.6t, saves 0.8t)
   The speed improvement per step is constant (+1.5 px/tick), but the absolute time savings shrinks.

2. **Damage halved again.** At MaxFP=0.5, damage/shot = 2. Compared to MaxFP=1.0 (4 damage/shot),
   Red needs exactly 2× as many hits to kill a full-energy Blue tank. At PR=160, the marginal 0.8t
   improvement in travel time is far below what's needed to compensate for the 50% damage cut.

3. **Tank stat evidence.** At MaxFP=0.5, Ghost (MaxFP=2.0) survival rate 20.5% while attack tanks
   (0.5) survive at 30-38%. Ghost absorbs disproportionate Blue fire (avg 121 damage taken vs ~94
   for attack tanks). This means attack tanks are not converting their speed advantage into kills
   fast enough — Blue lives longer and focuses Ghost down.

**MaxFP=1.0 is the confirmed floor for the bullet speed gradient at PR=160.**

Energy mechanics at MaxFP=0.5:
- Damage/shot: 2 (vs 4 at 1.0, -50%)
- Energy gained on hit: 1.5 (vs 3.0 at 1.0)
- Bullet speed: 18.5 px/tick (vs 17.0 at 1.0, +8.8%)
- The speed improvement (+8.8%) cannot compensate for the damage reduction (-50%).

## Key Findings

- **MaxFP=0.5 REFUTED: -6.0pp vs baseline (1 seed, stopped early).**
- MaxFP=1.0 is confirmed as the optimal floor for Arrow/Blade/Hammer/Trooper at PR=160.
- The faster-bullets gradient has a clear floor: it reverses once per-shot damage becomes too low
  to sustain combat pressure even with near-maximal bullet speed.
- Ghost at MaxFP=2.0 is structurally important: burst damage from Ghost + rapid fire from attack
  tanks is the winning combination. Weakening both simultaneously (attack tanks at 0.5 + Ghost
  absorbing fire) breaks the synergy.
- Next: architecture-level options — Encircle radius tuning (fixed 180px vs Wolfpack's 160px),
  RetreatEnergyThreshold sweep (Hammer=25→others=15 for more aggressive combat), or Ghost ECM.

## Summary

REFUTED. Code reverted to MaxFP=1.0. MaxFP=1.0 is the bullet speed optimum floor.
