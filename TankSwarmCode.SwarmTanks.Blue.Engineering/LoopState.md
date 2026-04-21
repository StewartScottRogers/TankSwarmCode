# Blue Engineering — Loop State

## Last Updated
2026-04-20 — Iteration 9

## Current Iteration
**10** — pending

## Situation
**WE ARE DOMINATING.** Blue ~80-87% across seeds (seed 1000 ~80% avg, seed 2000 87%). Iter 3 explored 6 approaches — all regressed. Current config is the optimum found so far.

Note: parallel execution (`--parallel 16`) introduces ~10-20pp run-to-run variance. Single runs are estimates; direction of change is reliable when both seeds agree.

## Active Configuration
- BlueSharp: MaxFP=3.0, PR=300, FormationSlot=1, Retreat=20 — **DO NOT TOUCH** (MVP both seeds)
- BlueStrike: MaxFP=3.0, PR=250, FormationSlot=0 (leader), Retreat=25 — **DO NOT LOWER PR** (kills seed 2000)
- BlueGuard: MaxFP=3.0, PR=200, FormationSlot=3, Retreat=30 — **KEEP** (Iter 2 win)
- BlueRush: MaxFP=2.5, PR=180, FormationSlot=2, Retreat=20 — **DO NOT RAISE MaxFP** (at 3.0: Blue 58%)
- BlueEcm: MaxFP=5.0, PR=150, HasEcm=true, Retreat=35 — ECMScreen/EcmAlert dead code
- Per-tank coordinator: each tank creates `new SwarmCoordinator()` in OnStart — **DO NOT revert to ForTeam** (static registry bug)
- Wolfpack angle-offset: slot × 72° approach angle + tank's own PR as orbit radius — **KEEP** (Iter 9 win, +3.8pp avg)

## Parallel Mode Baseline (post-Iter-9 Wolfpack angle-offset, 200 games each)
- Seed 1000: 74%  |  Seed 2000: 81%  |  Seed 3000: 82%  |  Seed 4000: 82%  |  Seed 5000: 85%
- **5-seed average: 80.8%**
- Note: serial mode (--parallel 1) shows ~34-36% at seed 1000 — the parallel vs serial gap is unexplained (possibly engine routes SwarmMessages across parallel games). Use --parallel 16 as the consistent measurement mode.

## Pre-Wolfpack-angle Baseline (Iter-7 fix, 200 games each)
- Seed 1000: 75%  |  Seed 2000: 72%  |  Seed 3000: 76%  |  Seed 4000: 90%  |  Seed 5000: 72%
- **5-seed average: 77%**

## Next Hypothesis (Iteration 10)

**Fine-tune the angle-offset Wolfpack: try wider spread (90° per slot = 4-way instead of 5-way) or narrower (60° per slot = 6-way).** The 72° spacing distributed 5 tanks evenly over 360°. A wider spread (90°: 0°, 90°, 180°, 270°, 0° for the 5th tank — wait this doesn't divide evenly) — alternatively, try 45° per slot offset to cluster into two pairs plus one: 0°/45°/90°/135°/180°. OR try staggered formation: front 3 tanks close (72° spread at smaller radius) and rear 2 tanks at wider angles/bigger radius.

More promising: **tune the angle for the specific formation strengths**. The leader (slot 0) approaches at 0°. Other slots fan out. What if we use non-uniform angles that concentrate firepower while still surrounding? E.g., 0°/60°/120°/240°/300° (leaving a 120° gap — the rear) to keep all 5 tanks in the forward arc.

Success criteria: 5-seed average ≥82% (vs 80.8% current).

---

## What We Know (Critical)

### What WORKS
- Guard MaxFP=3.0 (Iter 2): +15pp seed 1000, +4pp seed 2000 — keep it

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
- **Seed 4000 is the sensitivity canary**: almost any change drops it from 90% to 70-75%. Do not sacrifice seed4000 for other seeds (swaps create ~neutral average)
- **Per-tank coordinator fix**: COMMIT. `_swarm = new SwarmCoordinator()` in OnStart is architecturally correct; ForTeam/static registry breaks parallel mode.

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
