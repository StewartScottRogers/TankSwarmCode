# Autonomous Loop State

## Last Updated
2026-04-18 — Iteration 2 complete

## Current Iteration
**3** — pending

## Next Hypothesis
Reducing RedGhost's jamming radius by 25% (ECM nerf) in the default arena (800×600) brings Red's win rate from 56% toward 45–52%, drops Ghost's solo carry rate from 30% to ≤20%, while Ghost retains MVP status.

**Branch:** `research/iter-3-ghost-ecm-nerf`

---

## Iteration Log

### Iter 2 — `research/iter-2-ghost-cramped-arena`
**Date:** 2026-04-18
**Status:** Inconclusive — direction confirmed, magnitude missed

**Hypothesis:** Cramped arena (350×250) reduces RedGhost solo carry rate by ≥50% (30% → ≤15%) by limiting ECM spoofing effectiveness.

**Run:** 200 matches, seed 1000, `--on-timeout energy`, cramped arena (350×250)

**Results:**
- Red 69 (34%) / Blue 131 (66%)
- RedGhost WinSurv: 33/69 = **48%**
- RedGhost solo carry: 12/69 = **17%** (reduction 43%, threshold ≥50%)
- RedGhost Rate/100t: **2.41** (near 0 — confirmed)
- RedGhost All-in: **0 loss-survivals** (regained All-in status in cramped arena)
- RedGhost Linchpin: **alive → 100% Red wins; dead → 22% Red wins**

**Inconclusive:** Direction confirmed (solo carry fell 30%→17%), threshold not met (≥50% reduction, ≤15%)
**Surprises:**
- Ghost became Linchpin in cramped arena — more critical, not less
- Red collapsed from 56% → 34%; Blue's blitz (76t median decisive) overwhelms Red's grind strategy in small map
- Blade+Ghost combo at 16% of Red wins — Ghost enables Blade in cramped quarters rather than soloing
- Ghost regained All-in status (5 loss-survivals in default → 0 in cramped)

**BlueEcm note:** BlueEcm still MVP at 74/131 = 57% WinSurv, now also a combat unit in cramped (Rate/100t = 23.85).

---

### Iter 1 — `research/iter-1-redghost-solo-carry`
**Date:** 2026-04-18
**Status:** Partially Confirmed — needs human merge decision

**Hypothesis:** RedGhost carries Red wins via ECM outlasting (timeout), not combat — solo carry rate ≥35% of Red wins, combat rate near 0, All-in role, WinSurv 60–80%.

**Run:** 200 matches, seed 1000, `--on-timeout energy`, default arena (800×600)

**Results:**
- Red 112 (56%) / Blue 88 (44%)
- RedGhost WinSurv: 65/112 = **58%** (baseline 60–80%, just below)
- RedGhost solo carry: 34/112 = **30%** (threshold ≥35% — not met)
- RedGhost Rate/100t: **2.16** (near 0 — confirmed)
- RedGhost solo breakdown: **25 decisive + 9 timeout** (74% decisive)
- RedGhost loss-survivals: **5** (not All-in)

**Confirmed:** MVP role, ECM role, near-zero combat rate
**Refuted:** "Via timeout outlasting" claim; "All-in" claim from prior baseline
**New finding:** Ghost wins decisive solos at near-zero damage → Blue self-eliminates via ECM spoofing misdirection

**Baseline corrections:**
- Prior baseline claimed solo carry 35–50% via timeout → actual is 30% with majority decisive
- Prior baseline claimed All-in → actual has 5 loss-survivals

**BlueEcm note:** BlueEcm is Blue's MVP (53/88 WinSurv = 60%), using burnthrough + jam. Blue is 8% below parity (44%).
