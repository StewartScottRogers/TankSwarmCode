# Blue Engineering — Loop State

## Last Updated
2026-04-20 — Iteration 2

## Current Iteration
**3** — pending

## Situation
**WE ARE DOMINATING.** BlueGuard MaxFP 2.0→3.0 improved both test seeds. New baselines: seed 1000 ~81%, seed 2000 ~87%.

Note: parallel execution (`--parallel 16`) introduces ~10-20pp run-to-run variance. Single runs are estimates; direction of change is reliable when both seeds agree.

## Active Configuration
- BlueSharp: MaxFP=3.0, PR=300, FormationSlot=1, Retreat=20
- BlueStrike: MaxFP=3.0, PR=250, FormationSlot=0 (leader), Retreat=25
- BlueGuard: **MaxFP=3.0**, PR=200, FormationSlot=3, Retreat=30
- BlueRush: MaxFP=2.5, PR=180, FormationSlot=2, Retreat=20
- BlueEcm: MaxFP=5.0, PR=150, HasEcm=true, Retreat=35

## Next Hypothesis (Iteration 3)

**BlueRush MaxFP 2.5→3.0 will produce the same per-shot damage boost that Guard MaxFP improvement delivered.** Rush is a medium-range fighter (PR=180) with consistent radar contact. Higher damage per shot costs more energy per fire but each hit does more damage. Success: Blue ≥80% at seed 1000, ≥85% at seed 2000. If either seed regresses >3pp, revert and try a different lever.

Success criteria:
- Blue win rate ≥80% at seed 1000
- Blue win rate ≥85% at seed 2000
- No tank loses MVP/Co-MVP status that held it in Iter 2

---

## Iteration Log

### Iter 0 — Baseline established
**Date:** 2026-04-20
**Status:** New-arch baseline recorded. Research starting from scratch.
- Blue 66% / Red 34% (seed 1000, new arch, MaxFP configs as shipped)
- BlueSharp MVP (89% WinSurv), BlueGuard top attacker (4.29 rate)
- BlueEcm rate 1.83 (dramatically reduced from old arch 8–12)
- All old-arch findings invalid for current code.

### Iter 1 — Seed 2000 characterization
**Date:** 2026-04-20
**Hypothesis:** Baseline characterization at seed 2000 confirms Guard→Sharp pattern is seed-stable.
**Result:** Blue 83% / Red 17% at seed 2000. BlueSharp MVP (161/166 = 97% WinSurv). BlueEcm contributes (9x wins without it). Win pattern is full-team dominant (80% of wins). Hypothesis confirmed.
**Key finding:** Variance is high between seeds (66% at seed 1000 vs 83% at seed 2000). BlueStrike (FormationSlot=0, leader) has rate 0.91 (Hider) — it's the leader but barely fires at PR=250.

### Iter 2 — BlueGuard MaxFP 2.0→3.0
**Date:** 2026-04-20
**Hypothesis:** Raising BlueGuard MaxFP from 2.0 to 3.0 increases damage per shot for our medium-range attacker. Seed-agnostic change (no position changes). Expected uniform improvement.
**Code change:** `BlueGuardCortex.cs`: MaxFirePower 2.0 → 3.0
**Also tested:** BlueStrike PR 250→200 (REVERTED — improved seed 1000 by +26pp but regressed seed 2000 by -15pp; too seed-sensitive).
**Results:**
- Seed 1000: Two runs → 71% and 91%, avg ~81% (baseline 66%) — improvement
- Seed 2000: 87% (baseline 83%) — improvement
**Analysis:** BlueGuard rate at seed 1000 increased to 3.69 (was 4.29 at baseline, up significantly). Guard is Co-MVP in seed 1000 wins. Both seeds improved.
**Status:** COMMITTED. Guard MaxFP=3.0 is the new baseline.
