# Blue Engineering — Loop State

## Last Updated
2026-04-20 — Iteration 6

## Current Iteration
**7** — pending

## Situation
**WE ARE DOMINATING.** Blue ~80-87% across seeds (seed 1000 ~80% avg, seed 2000 87%). Iter 3 explored 6 approaches — all regressed. Current config is the optimum found so far.

Note: parallel execution (`--parallel 16`) introduces ~10-20pp run-to-run variance. Single runs are estimates; direction of change is reliable when both seeds agree.

## Active Configuration
- BlueSharp: MaxFP=3.0, PR=300, FormationSlot=1, Retreat=20 — **DO NOT TOUCH** (MVP both seeds)
- BlueStrike: MaxFP=3.0, PR=250, FormationSlot=0 (leader), Retreat=25 — **DO NOT LOWER PR** (kills seed 2000)
- BlueGuard: MaxFP=3.0, PR=200, FormationSlot=3, Retreat=30 — **KEEP** (Iter 2 win)
- BlueRush: MaxFP=2.5, PR=180, FormationSlot=2, Retreat=20 — **DO NOT RAISE MaxFP** (at 3.0: Blue 58%)
- BlueEcm: MaxFP=5.0, PR=150, HasEcm=true, Retreat=35 — ECMScreen/EcmAlert dead code

## Next Hypothesis (Iteration 7)

**Establish true baseline variance with a 5-seed 200-batch run, then accept ceiling or try a structural change.** Every single-parameter tweak has regressed seed 2000 while seed 1000 varies ±20pp. We don't know if the true ceiling is 80% or 88%. Run seeds 1000/2000/3000/4000/5000 at 200 battles each to get a proper multi-seed baseline. If the true avg is ≥83%, declare ceiling reached. If ≤78%, there's still room — investigate what Red does in the losing games to find the structural weakness.

Structural change candidates not yet tried:
- Scout before Wolfpack: send one tank ahead to spot enemies before committing formation (reduces blind-spot exposure)
- Target closest enemy instead of lowest-energy (better hit rate at the cost of focus-fire suboptimality)
- Adaptive formation: tanks with lower energy back off to higher PR, creating dynamic spacing

Success criteria: establish ≥200-game baseline per seed for confident direction decisions.

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
