# Red Engineering — Loop State

## Last Updated
2026-04-20 — Iteration 0 (new-arch baseline established)

## Current Iteration
**1** — pending

## Situation
**WE ARE LOSING.** New architecture baseline: **Red 34% / Blue 66%** (seed 1000, 200 matches, MaxFP=1.5, PR=150).

This is the starting point for Red Engineering. All prior research (iters 1–83) was conducted against the old architecture and is invalid. The old balance fix (BlueEcm MaxFP=1.0) does not apply here.

## New Architecture Battle Intelligence

From iter-84 baseline run (seed 1000, 200 matches):
- **Red 67 (33%) / Blue 132 (66%)** — heavily Blue-dominant
- RedGhost: Survivor, ECM — NOT MVP, NOT Linchpin, NOT Solo carry. Rate/100t: 0.18 (nearly silent)
- BlueSharp: **Blue MVP (117/132 = 89% WinSurv)** — primary threat
- BlueGuard: Top attacker (rate 4.29) — driving kills
- BlueEcm: rate 1.83 — present but not dominant

## What We Know

- RedGhost's ECM carry role from the old architecture has collapsed. Rate 0.18 means it almost never fires. The new composition cortex is not using Ghost effectively.
- BlueSharp survives 89% of Blue's wins — it is the tank we must kill.
- BlueGuard is killing our tanks at rate 4.29.
- Red's defeat pattern is unknown — need to analyze win combos and first-kill data.

## Next Hypothesis (Iteration 1)

**RedGhost is nearly silent (rate 0.18) because the new composition cortex does not activate ECM effectively. The ECM that made Ghost a carry in the old arch is not firing. Hypothesis: directly tuning RedGhostCortex to activate ECM more aggressively (lower energy threshold for ECM engagement, higher ECM mode priority) will raise Ghost's rate to ≥2.0 and increase Red win rate by ≥5pp.**

Success criteria:
- RedGhost Rate/100t increases from 0.18 to ≥2.0
- Red win rate increases by ≥5pp (from 34% to ≥39%)

---

## Iteration Log

### Iter 0 — Baseline established
**Date:** 2026-04-20
**Status:** New-arch baseline recorded. Research starting from scratch.
- Red 34% / Blue 66% (seed 1000, new arch, MaxFP=1.5, PR=150)
- RedGhost silent (rate 0.18), BlueSharp dominant (89% WinSurv)
- All old-arch findings (PR curves, Linchpin thresholds, MaxFP fix) are invalid for current code.
