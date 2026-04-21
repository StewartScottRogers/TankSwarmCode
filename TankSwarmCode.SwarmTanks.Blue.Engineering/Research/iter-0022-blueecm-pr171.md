## Hypothesis
BlueEcm PreferredRange 150 → 171 will show a different result on seed 3000 than the 165 failure (-2.7pp in iter 20). The branch was generated with this specific target.

Success criteria: seed 3000 ≥ 90% (old baseline ~90.5%)

## Code Change
`BlueEcmCortex.cs`: `PreferredRange = 150.0` → `PreferredRange = 171.0`

## Run Parameters
- Seed 3000, 200 games, --parallel 16, --on-timeout energy
- Reverted to 150 after run; second run confirms 150 baseline

## Raw Results
**With PR=171 (seed 3000):** Red 133 (66%) / Blue 67 (34%)  
**With PR=150 baseline (seed 3000, run 1):** Red 124 (62%) / Blue 76 (38%)  
**With PR=150 baseline (seed 3000, run 2):** Red 124 (62%) / Blue 76 (38%)

Additional baseline runs (PR=150, fixed Red):
- Seed 1000: Blue 36%
- Seed 2000: Blue 35%
- Seed 5000: Blue 38%

## Analysis
**PR=171 is a regression.** The -4pp drop on seed 3000 is consistent with the -2.7pp seen for PR=165 in iter 20. Any departure from PR=150 hurts BlueEcm's formation geometry. This constraint holds regardless of Red's state.

**CRITICAL DISCOVERY: Red fixed their parallel mode bug.**

Commits `d9cb5c0` and `203c944` on this branch are Red's iter-17, which applied the exact same fix Blue used in iter-7: replacing `SwarmCoordinator.ForTeam()` with `new SwarmCoordinator()`. Red's parallel mode win rate jumped from 16% to 63.5%.

All of Blue's prior research (iters 7-21, the 77% → 88.3% journey) was conducted against a Red team winning only 16% in parallel mode due to a broken static coordinator. The "DO NOT" constraints and baselines were calibrated against that broken Red.

**True competitive baseline:** Blue ~36% vs fixed Red ~64%. The 88.3% figure is completely obsolete.

## Key Findings
1. PR=171 failed as expected (~-4pp vs PR=150 baseline on seed 3000)
2. Red fixed their parallel mode bug — Blue baseline resets from 88.3% to ~36%
3. The ~36% Blue win rate matches Red's stated 63.5% win rate after their fix
4. Many "DO NOT" constraints from prior iterations were calibrated against broken Red and may not hold against fixed Red
5. BlueEcm is Red's #1 first-kill target across all seeds (25-30 first kills per 200 games)
6. Blue gets first kill more often than Red (54-60% vs 40-46%) but wins only 21-31% of those 5v4 fights vs Red winning 47-51% of 4v5 fights — Blue's post-first-kill performance is the critical deficit
7. Red Arrow and Hammer are consistently MVP/Co-MVP with fixed coordination

## Summary
PR=171 is REFUTED (-4pp, consistent with all ECM PR changes hurting). No code committed.

More critically: Red has improved from 16% to 63.5% win rate by fixing their parallel mode bug. Blue's 88.3% ceiling is gone. The new baseline is ~36% Blue. All prior "DO NOT" constraints were found against a weaker Red and should be treated as provisional until re-tested.

Next iteration (23): Test BlueGuard MaxFP 3.0 → 4.0 or 5.0. The biggest single gain in Blue's history was Guard MaxFP 2.0→3.0 (+15pp seed 1000). Extending this to 4.0 or 5.0 is the most promising unexplored direction, given Guard is consistently top attacker.
