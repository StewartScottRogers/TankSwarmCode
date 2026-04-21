# Blue Engineering — Loop State

## Last Updated
2026-04-20 — Iteration 12

## Current Iteration
**13** — pending

## Situation
**DOMINATING AT NEW HIGH.** Blue 85.4% avg across 5 seeds (2 runs each). Red now has a 5th tank "Red4" (MVP, All-in). Iter 10 found 60° Wolfpack angle (+3.6pp). Iter 11 found Fallback threshold 30→20 (+1.5pp). Both committed.

Note: parallel execution (`--parallel 16`) introduces ~10-20pp run-to-run variance. Single runs are estimates; direction of change is reliable when MULTIPLE runs agree. Seed 4000 is the most volatile (78-96% range for same config in same session).

## Active Configuration
- BlueSharp: MaxFP=3.0, PR=300, FormationSlot=1, Retreat=20 — **DO NOT TOUCH** (MVP both seeds)
- BlueStrike: MaxFP=3.0, PR=250, FormationSlot=0 (leader), Retreat=25 — **DO NOT LOWER PR** (kills seed 2000)
- BlueGuard: MaxFP=3.0, PR=200, FormationSlot=3, Retreat=30 — **KEEP** (Iter 2 win)
- BlueRush: MaxFP=2.5, PR=180, FormationSlot=2, Retreat=20 — **DO NOT RAISE MaxFP** (at 3.0: Blue 58%)
- BlueEcm: MaxFP=5.0, PR=150, HasEcm=true, Retreat=35 — ECMScreen/EcmAlert dead code
- Per-tank coordinator: each tank creates `new SwarmCoordinator()` in OnStart — **DO NOT revert to ForTeam** (static registry bug)
- Wolfpack angle-offset: slot × 60° approach angle + tank's own PR as orbit radius — **KEEP** (Iter 10 win, +3.6pp avg)
- Fallback threshold: 20.0 (was 30.0) — **KEEP** (Iter 11 win, +1.5pp avg)
- BlueGuard FormationSlot=1 (was 3), BlueSharp FormationSlot=3 (was 1) — **KEEP** (Iter 12 win, +1.3pp avg)
- **Current slot layout**: Strike(0°,250px) → Guard(60°,200px) → Rush(120°,180px) → Sharp(180°,300px) → Ecm(240°,150px)

## Parallel Mode Baseline (post-Iter-12, 2×200 games each)
- Seed 1000: ~86.5% avg  |  Seed 2000: ~91% avg  |  Seed 3000: ~86% avg  |  Seed 4000: ~84% avg  |  Seed 5000: ~86% avg
- **5-seed average: ~86.7%** (avg of 2 full 5-seed sweeps)
- Note: 10-20pp run-to-run variance. Seed 4000 showed 78-96% range in same session. Require 2+ runs to confirm changes.

## Post-Iter-10 Baseline (60° Wolfpack, Fallback=30, 1 run)
- Seed 1000: 84%  |  Seed 2000: 86%  |  Seed 3000: 82%  |  Seed 4000: 84%  |  Seed 5000: 86%
- **5-seed average: 84.4%**

## Pre-Wolfpack-angle Baseline (Iter-7 fix, 200 games each)
- Seed 1000: 75%  |  Seed 2000: 72%  |  Seed 3000: 76%  |  Seed 4000: 90%  |  Seed 5000: 72%
- **5-seed average: 77%**

## Next Hypothesis (Iteration 13)

**Explore asymmetric radius tuning**: Now that slots are reordered (Guard at 1/60°, Sharp at 3/180°), BlueSharp approaches from 180° at 300px (the furthest and most rear-facing). This is already good. What about BlueGuard at 60°/200px vs BlueRush at 120°/180px — can we tune their radii?

Specific hypothesis: try increasing BlueRush PR from 180 to 200 (same as Guard). At 120°, BlueRush at 200px would form a tighter arc with Guard (60°/200px) and Strike (0°/250px). Previously BlueRush PR=160 was -8pp on seed4000 (Iter 8), but +1pp avg. Worth checking after slot reordering changes formation geometry.

Alternative: try BlueSharp PR change (currently 300px from 180°). At 180°, the "behind" approach at 300px might be more effective at 250px (matching Strike's front). Risk: previous tests of distance changes on Strike (not Sharp) caused regressions.

Success criteria: 5-seed average ≥88% (2+ run confirmation required).

---

## What We Know (Critical)

### What WORKS
- Guard MaxFP=3.0 (Iter 2): +15pp seed 1000, +4pp seed 2000 — keep it
- Wolfpack angle 60° (Iter 10): +3.6pp avg — keep it
- Fallback threshold 20.0 (Iter 11): +1.5pp avg (seeds 2000+5000 +5pp each) — keep it
- Guard/Sharp slot swap (Iter 12): +1.3pp avg, Guard(slot1=60°) Sharp(slot3=180°) — keep it

### Hard limits discovered
- **DO NOT** lower BlueStrike PR below 250 — all values (200, 230) devastate seed 2000 (-15 to -25pp)
- **DO NOT** raise BlueRush MaxFP above 2.5 — Rush becomes primary target, Blue collapses to 58%
- **DO NOT** use aggressive ECM changes — EcmAlert is never sent (dead code), changes just waste energy
- **DO NOT** extend priority target staleness beyond 30 ticks — tanks fire at dead contacts, drain energy in 25 ticks
- **DO NOT** lower Encircle threshold — early Encircle lets Red Hammer concentrate fire (68%)
- **DO NOT** use ECM jam-gap detection to trigger ECMScreen — false positives move all tanks to 130 standoff (56%)
- **DO NOT** reduce LeadershipEpochTicks below 40 — constant strategy churn, Red decisive wins spike (52%)
- **DO NOT** change BlueEcm RetreatThreshold from 35 — seed 2000 drops 23pp; seed 2000 is sensitive to BlueEcm behavior
- **DO NOT** expand VolleyRange beyond 300 — far tanks compute negative fire ticks, volley coordination breaks
- **DO NOT** add volley fire-tick correction in RunEpochLogic — self-message already corrects leader's fire tick; redundant fix causes double-fire conflicts
- **DO NOT** target highest-energy enemy — focus-fire on lowest-energy is correct; highest-energy extends time-to-first-kill, Red deals more damage (52% seed 2000)
- **DO NOT** change BlueStrike PR from 250 — seed 4000 drops 18pp at PR=220
- **DO NOT** change BlueEcm PR from 150 — PR=200 regresses all seeds (-17pp seed4000)
- **DO NOT** change BlueGuard Retreat from 30 — Retreat=25 gives -20pp seed4000, -10pp seed3000
- **DO NOT** change Wolfpack angle below 60° — 45° and 30° tested: same avg but higher seed variance
- **DO NOT** use center-seeking Scout — Blue clusters at center, Red exploits predictability (-3.6pp)
- **DO NOT** increase Scout radar spin above 45° — 90° tested: -3.4pp avg
- **Seed 4000 is extremely volatile**: up to 78-96% range in same session for identical config. Require 2+ runs. "DO NOT sacrifice seed4000" rule still applies but single runs unreliable.
- **Per-tank coordinator fix**: COMMIT. `_swarm = new SwarmCoordinator()` in OnStart is architecturally correct; ForTeam/static registry breaks parallel mode.

### Red's New 5th Tank (Red4)
Red added a 5th tank "4" (RedTrooper) around their iter 16. Key observations:
- Red4 is their MVP (72% WinSurv when Red wins) and All-in (only survives in wins)
- Red4 is Linchpin: Red win rate drops from 100% to 9% when Red4 is dead
- Blue kills Red4 last (14 first-kills vs Ghost 33, Arrow 31) — lowest priority from our focus-fire targeting
- Red4 frequently stalls timeout games (5+ wins via timeout at 5000 ticks with Red4 alive at 100E)

### ECM Dead Code (key insight)
EcmAlert SwarmMessage is never sent by Blue AI. Therefore IsEnemyEcmActive() is always false. Consequences:
- ECMScreen strategy NEVER activates
- BlueEcm NEVER jams (OffensiveEcmMode=Jam)
- Burnthrough NEVER activates (all tanks fire blind when Ghost jams)
- When Ghost jams: 50% drop rate on our scans (we miss Ghost half the time)
- Ghost in Jam mode CANNOT FIRE either (ArenaEngine.cs line 382)
- Ghost strategy: Jam to hide → pay 0.5 energy/tick → exit to fire → gain 3*power per hit → repeat

---

## Iteration Log

### Iter 0 — Baseline established
**Date:** 2026-04-20
- Blue 66% / Red 34% (seed 1000, new arch)
- BlueSharp MVP (89% WinSurv), BlueGuard top attacker (4.29 rate)

### Iter 1 — Seed 2000 characterization
**Date:** 2026-04-20
- Blue 83% at seed 2000. BlueSharp MVP (97%). BlueEcm: 9x wins without it.
- Strike rate 0.91 (Hider) — lead tank barely fires

### Iter 2 — BlueGuard MaxFP 2.0→3.0
**Date:** 2026-04-20
- **Code change:** BlueGuardCortex.cs MaxFirePower 2.0 → 3.0
- Seed 1000: ~81% avg (two runs: 71%, 91%) — baseline 66%
- Seed 2000: 87% — baseline 83%
- **COMMITTED**

### Iter 3 — Exploration (all reverted)
**Date:** 2026-04-20
- BlueRush MaxFP 2.5→3.0: FAILED (58% seed 1000, Rush becomes primary target)
- BlueEcm immediate-jam: FAILED (70% seed 1000, wrong semantics)
- Volley in ECMScreen: ambiguous (~70%, likely variance)
- Priority target staleness 30→60 ticks: CATASTROPHIC (tanks fire at dead contacts, die in 25 ticks)
- BlueStrike PR 250→230: FAILED (62% seed 2000, regression)
- Encircle threshold lowered: FAILED (68% seed 1000, Red Hammer concentrates fire)
- **Net result: No change. Guard MaxFP=3.0 config is the current optimum.**

### Iter 12 — Guard/Sharp slot swap: +1.3pp average (**COMMITTED**)
**Date:** 2026-04-20
- **Code change:** BlueGuardCortex.cs FormationSlot 3→1; BlueSharpCortex.cs FormationSlot 1→3
- New formation: Strike(0°,250px) → Guard(60°,200px) → Rush(120°,180px) → Sharp(180°,300px) → Ecm(240°,150px)
- Effect: BlueGuard (top attacker) approaches from front-right (60°) at 200px — more aggressive position. BlueSharp (long range) approaches from behind-left (180°) at 300px — harder for Red to target.
- Run 1: 87/96/84/84/84 = 87.0%  |  Run 2: 86/86/88/84/88 = 86.4%
- **Avg 86.7% vs 85.4% Fallback=20 baseline → +1.3pp (1.6 sigma / 2000 games)**
- Seed 4000 (canary) improved +4pp. Seed 5000 -2pp (within variance). 4/5 seeds improved or neutral.
- Failed during iter 12 exploration: 3-way Pincer (same avg, seed1000 -4pp), Scout-center (80.8%), Fallback=25 (82.8%), Scout radar=90° (81%)
- **COMMITTED**

### Iter 11 — Fallback threshold 30→20: +1.5pp average (**COMMITTED**)
**Date:** 2026-04-20
- **Code change:** `SelectStrategy` Fallback threshold: `sumEnergy/allyCount < 30` → `< 20`
- Effect: Blue stays in Wolfpack/Pincer/Encircle until more depleted; fights more aggressively at low energy instead of retreating to corner
- Run 1: Seed 1000: 84% | Seed 2000: 90% | Seed 3000: 86% | Seed 4000: 78% | Seed 5000: 88% = 85.2%
- Run 2: Seed 1000: 84% | Seed 2000: 88% | Seed 3000: 86% | Seed 4000: 82% | Seed 5000: 88% = 85.6%
- **Avg 85.4% vs 83.9% baseline (2 runs each) → +1.5pp (1.8 sigma / 2000 games)**
- Seed 2000 and 5000 consistently +5pp. Seed 4000 extremely volatile (78-96% in same session).
- Failed alternatives: Fallback=25 (82.8%, clear regression), Fallback=20+center-seeking Scout (80.8%, regression)
- **COMMITTED**

### Iter 10 — Wolfpack angle 72°→60°: +3.6pp average (**COMMITTED**)
**Date:** 2026-04-20
- **Code change:** `ExecuteWolfpack` approach angle: `slot × 72°` → `slot × 60°`
- Effect: 5 tanks spread over 240° arc (vs 360° at 72°) — front-heavy concentration; creates 120° gap at rear
- Seed 1000: 84% (+10pp vs 74% at 72°)  |  Seed 2000: 86% (+5pp)  |  Seed 3000: 82% (0pp)  |  Seed 4000: 84% (+2pp)  |  Seed 5000: 86% (+1pp)
- **5-seed avg: 84.4% vs 80.8% baseline → +3.6pp**
- Also tested: 45° (85.2% avg, within noise, mixed seeds), 30° (84.4% avg, same avg, higher variance) — both reverted
- Pattern: smaller angle = more front-concentrated; 60° is robust optimum
- **COMMITTED** (angle change made in git commit `00f7f08` before iter 10 was formalized)

### Iter 9 — Wolfpack angle-offset formation: +3.8pp average (**COMMITTED**)
**Date:** 2026-04-20
- **Code change:** `ExecuteWolfpack` computes `approachAngle = slot × 72°`, then navigates to `target.PolarOffset(approachAngle, PreferredRange)` instead of directly toward target
- Seed 1000: 74% (−1pp)  |  Seed 2000: 81% (+9pp)  |  Seed 3000: 82% (+6pp)  |  Seed 4000: 82% (−8pp)  |  Seed 5000: 85% (+13pp)
- **5-seed avg: 80.8% vs 77% baseline → +3.8pp**
- Significance: 807 wins / 1000 games vs expected 770 = 2.8 sigma
- Mechanism: distributes Blue tanks around the target at 72° intervals with their own PR as orbit radius, forcing Red to defend from 5 directions simultaneously
- **COMMITTED**

### Iter 8 — Re-validated Guard MaxFP=3.0; all other parameter sweeps failed
**Date:** 2026-04-20
- Guard MaxFP=2.0 re-test: 75.2% avg (5-seed) vs 77% baseline → Guard 3.0 confirmed better, especially seed4000 (+19pp). KEEP Guard 3.0.
- BlueRush PR=160: avg 76.6% vs 77% — neutral (seed4000 -8pp), reverted
- BlueStrike PR=220: avg dropped (seed4000 -18pp), reverted
- BlueEcm PR=200: all seeds regress (seed4000 -17pp), reverted
- BlueRush Retreat=25: avg 78% vs 77% — +1pp but not significant; seed3000 +15pp / seed4000 -16pp swap, reverted
- BlueGuard Retreat=25: avg 72.4% vs 77% — failed (seed3000 -10pp, seed4000 -20pp), reverted
- **Pattern: seed 4000 is an outlier at 90% baseline and drops -10 to -20pp on almost every single-parameter change. 77% average is the single-parameter ceiling.**
- **Net result: No change. Configuration unchanged.**

### Iter 7 — Fix parallel mode static registry bug + 5-seed baseline
**Date:** 2026-04-20
- **Critical discovery:** `SwarmCoordinator.Registry` is a static `ConcurrentDictionary` — in parallel mode, all games share one coordinator, corrupting AllyPings and strategy state across games. Same bug as Red research "PARALLEL MODE BROKEN" finding.
- **Fix:** Changed `OnStart` to `_swarm = new SwarmCoordinator()` (per-tank, per-game) instead of `ForTeam(swarmId)` (shared static instance).
- **5-seed 200-batch baseline after fix:** seed1000=75%, seed2000=72%, seed3000=76%, seed4000=90%, seed5000=72%. **Avg=77%.**
- **Serial mode (--parallel 1):** ~34-36% at seed 1000 — gap vs parallel mode is unexplained.
- Prior iters 3-6 ALL used the broken parallel mode. Direction of changes may still be valid but absolute numbers were unreliable.
- **COMMITTED** (static registry fix is a real bug fix regardless of win rate impact)

### Iter 6 — Target highest-energy enemy (reverted)
**Date:** 2026-04-20
- **Code change:** `RunEpochLogic` priority target: `OrderBy(Energy)` → `OrderByDescending(Energy)` — focus Red's top threat (Hammer) first
- Seed 1000: 73% (within noise of baseline)
- Seed 2000: 52% (severe regression, -35pp)
- Root cause: focusing the highest-energy tank extends time-to-first-kill; Red deals more total damage during the longer fight. Classic focus-fire theory holds — kill the weakest first.
- **REVERTED. No commit.**

### Iter 5 — Exploration of coordination/ECM knobs (all reverted)
**Date:** 2026-04-20
- ECM jam-gap detection (DetectJamGapAndAlert): FAILED (56% seed 1000) — false positives trigger ECMScreen, clusters tanks at 130-unit standoff where Red Hammer concentrates fire
- LeadershipEpochTicks 40→20: FAILED (52% seed 1000) — constant strategy churn, Red decisive wins spike 3x, tanks never settle on targets
- BlueEcm RetreatThreshold 35→20: FAILED (64% seed 2000) — seed 1000 within noise (78%) but seed 2000 drops -23pp; seed 2000 sensitive to BlueEcm behavior changes
- VolleyRange 300→400: FAILED (59%/65%) — far tanks compute negative scheduled fire ticks, volley coordination breaks, shots arrive out of sync
- Leader volley fire-tick correction: FAILED (77%/44%) — self-message already corrects leader's fire tick; the fix caused double-fire conflicts and added redundant shot
- **Net result: No change. Guard MaxFP=3.0 config remains optimum. ~80% seed 1000, ~80% seed 2000 (variance: 63-87%).**

### Iter 4 — 6th Blue tank (BlueTrooper, reverted)
**Date:** 2026-04-20
- **Code change:** Added `public BlueTrooper() : this(5) { }` parameterless constructor so CLI loads Blue5 as 6th tank
- Seed 1000: 78% (within noise of ~80% baseline — inconclusive)
- Seed 2000: 68% (severe regression from 87% baseline)
- Root cause: 6th tank disrupts seed 2000 geometry. Current 5-tank spread (PR: 150-180-200-250-300) is tuned; adding a 6th at PR=200/MaxFP=2.5 clusters Blue and concentrates Red fire.
- **REVERTED. No commit.**
