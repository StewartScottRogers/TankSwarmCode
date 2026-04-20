# Blue Engineering — Loop State

## Last Updated
2026-04-20 — Iteration 0 (new-arch baseline established)

## Current Iteration
**1** — pending

## Situation
**WE ARE WINNING.** New architecture baseline: **Blue 66% / Red 34%** (seed 1000, 200 matches, MaxFP=1.5, PR=150).

This is the starting point for Blue Engineering. All prior research (iters 1–83) was conducted against the old architecture and is invalid. The task now is to understand WHY we are winning and lock it in before Red adapts.

## New Architecture Battle Intelligence

From iter-84 baseline run (seed 1000, 200 matches):
- **Blue 132 (66%) / Red 67 (33%)** — heavily Blue-dominant
- BlueSharp: **MVP (117/132 = 89% WinSurv)** — our primary carry
- BlueGuard: Top attacker (rate 4.29) — driving kills, forcing Red into bad positions
- BlueEcm: Survivor, ECM, rate 1.83 — present but not a dominant contributor
- RedGhost: rate 0.18, NOT MVP, NOT Linchpin — their ECM specialist is nearly silent

## What We Know

- BlueSharp is the engine of our wins. Its survival at 89% of Blue wins means it consistently outlasts Red. Protect this.
- BlueGuard's high attack rate is creating the kills that let BlueSharp survive. The Guard → Sharp combo appears to be our core pattern.
- BlueEcm fires at rate 1.83 vs old-arch 8–12. The volley-fire composition approach fires less frequently. It is unclear how much ECM is contributing to wins vs just surviving passively.
- Red's Ghost is nearly silent — our radar / arena edge avoidance changes may have fundamentally broken their primary strategy. We should understand this before they fix it.

## Next Hypothesis (Iteration 1)

**BlueGuard (rate 4.29) and BlueSharp (89% WinSurv) are jointly responsible for most Blue wins, but we do not know the exact win combo breakdown. Hypothesis: a baseline characterization run at seed 2000 will confirm the Guard→Sharp win pattern is seed-stable, and reveal whether BlueEcm contributes to wins or is a passenger. Success: Blue win rate ≥55% at seed 2000, BlueSharp remains MVP.**

Success criteria:
- Blue win rate ≥55% at seed 2000
- BlueSharp WinSurv ≥75% at seed 2000
- Win combo data clarifies BlueEcm's actual contribution

---

## Iteration Log

### Iter 0 — Baseline established
**Date:** 2026-04-20
**Status:** New-arch baseline recorded. Research starting from scratch.
- Blue 66% / Red 34% (seed 1000, new arch, MaxFP=1.5, PR=150)
- BlueSharp MVP (89% WinSurv), BlueGuard top attacker (4.29 rate)
- BlueEcm rate 1.83 (dramatically reduced from old arch 8–12)
- All old-arch findings (PR curves, Linchpin thresholds, MaxFP balance fix) are invalid for current code.
