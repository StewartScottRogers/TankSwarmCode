# Blue Engineering — Loop State

## Last Updated
2026-04-20 — Iteration 4

## Current Iteration
**5** — pending

## Situation
**WE ARE DOMINATING.** Blue ~80-87% across seeds (seed 1000 ~80% avg, seed 2000 87%). Iter 3 explored 6 approaches — all regressed. Current config is the optimum found so far.

Note: parallel execution (`--parallel 16`) introduces ~10-20pp run-to-run variance. Single runs are estimates; direction of change is reliable when both seeds agree.

## Active Configuration
- BlueSharp: MaxFP=3.0, PR=300, FormationSlot=1, Retreat=20 — **DO NOT TOUCH** (MVP both seeds)
- BlueStrike: MaxFP=3.0, PR=250, FormationSlot=0 (leader), Retreat=25 — **DO NOT LOWER PR** (kills seed 2000)
- BlueGuard: MaxFP=3.0, PR=200, FormationSlot=3, Retreat=30 — **KEEP** (Iter 2 win)
- BlueRush: MaxFP=2.5, PR=180, FormationSlot=2, Retreat=20 — **DO NOT RAISE MaxFP** (at 3.0: Blue 58%)
- BlueEcm: MaxFP=5.0, PR=150, HasEcm=true, Retreat=35 — ECMScreen/EcmAlert dead code

## Next Hypothesis (Iteration 5)

**Fix the ECM dead code by sending EcmAlert when we detect Ghost in Jam mode.** When a RadarContact for an enemy flickers (present tick N, absent tick N+1 to N+k, then reappears), that gap is consistent with Jam — Ghost disappears from radar during Jam window. If we broadcast EcmAlert on detection of this contact-gap pattern, then `IsEnemyEcmActive()` becomes true, `ECMScreen` activates, and Burnthrough fires for all non-ECM tanks. This gives us an active counter to Ghost's Jam strategy instead of eating the 50% scan drop passively.

The detection mechanism: track each enemy RadarContact's `Timestamp`. If an enemy was seen within 30 ticks but is missing from RadarMap this tick, that's a potential Jam gap. If this happens while other enemies are still visible (so it's not just blind-spot), broadcast EcmAlert.

Implementation target: `BlueCortexBase.cs` or `SwarmCoordinator.cs` — add Jam-gap detection logic to the tick handler. `SwarmCoordinator` already has `EnemyEcmAlertTick` and `HandleEcmAlert()` — we just need a code path that calls it without relying on the incoming SwarmMessage.

Success criteria:
- Blue win rate ≥83% at seed 1000 (up from ~80% avg)
- Blue win rate ≥87% at seed 2000 (maintain)
- BlueEcm actually Jams at least once per game (observable from rate stats)

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

### Iter 4 — 6th Blue tank (BlueTrooper, reverted)
**Date:** 2026-04-20
- **Code change:** Added `public BlueTrooper() : this(5) { }` parameterless constructor so CLI loads Blue5 as 6th tank
- Seed 1000: 78% (within noise of ~80% baseline — inconclusive)
- Seed 2000: 68% (severe regression from 87% baseline)
- Root cause: 6th tank disrupts seed 2000 geometry. Current 5-tank spread (PR: 150-180-200-250-300) is tuned; adding a 6th at PR=200/MaxFP=2.5 clusters Blue and concentrates Red fire.
- **REVERTED. No commit.**
