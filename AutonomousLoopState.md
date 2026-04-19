# Autonomous Loop State

## Last Updated
2026-04-18 — Iteration 18 complete

## Current Iteration
**19** — pending

## Status
**BALANCE FIX CONFIRMED (iter-11).** BlueEcm MaxFirePower=1.0 achieves Red 51% / Blue 49%, triple-seed validated (seeds 1000/2000/3000).

Iter 18 confirmed MaxFP and PR effects are approximately additive (no nonlinear interaction). MaxFP=1.5 + PR=80 ≈ 52/48 balanced — the +5pp Red from MaxFP=1.5 and +7pp Blue from PR=80 cancel. Linchpin requires MaxFP=1.0 + PR=150 simultaneously.

**Awaiting human merge of iter-11 fix (BlueEcm MaxFirePower=1.0) to master.**

**Next hypothesis:** MaxFP effect is linear between 1.0 (51% Red, Linchpin) and 1.5 (56% Red, no Linchpin). Test MaxFP=1.25 with PR=150 — predict Red ~53–54%, likely no Linchpin. This probes the MaxFP curve shape and Linchpin threshold location in the 1.0–1.5 range.

---

## Iteration Log

### Iter 18 — `research/iter-000018-blueecm-maxfp15-pr80`
**Date:** 2026-04-18
**Status:** ✅ REFUTED — MaxFP=1.5 does not dominate PR=80; effects approximately cancel

**Hypothesis:** MaxFP=1.5 + PR=80 will tip toward Red-dominant (MaxFP=1.5 dominates over PR=80).

**Run:** 200 matches, seed 1000, `--on-timeout energy`, default arena (800×600), BlueEcm MaxFP=1.5 + PR=80

**Results:**
- Red 104 (52%) / Blue 96 (48%) — **near-balanced, same as iter-11/17**
- BlueEcm: MVP (50/96), **NO Linchpin**, survival 61/200 (dropped from 80/200)
- Effects are additive: MaxFP=1.5 (+5pp Red) + PR=80 (+7pp Blue) ≈ cancel to 52/48

**2×2 grid (MaxFP × PR):**

| Config               | Red%  | Blue% | Linchpin? |
|----------------------|-------|-------|-----------|
| MaxFP=1.5, PR=150    | 56%   | 44%   | No        |
| MaxFP=1.0, PR=150    | 51%   | 49%   | Yes       |
| MaxFP=1.0, PR=80     | 44%   | 56%   | No        |
| MaxFP=1.5, PR=80     | 52%   | 48%   | No        |

**Key finding:** Linchpin requires MaxFP=1.0 AND PR=150 simultaneously. Parameter effects are approximately additive — no interaction synergy.
**Code:** Reverted — PR=80 probe only.

---

### Iter 14 — `research/iter-000014-blueecm-fp-curve`
**Date:** 2026-04-18
**Status:** ✅ CONFIRMED — non-linearity is a sharp threshold between MaxFP=0.75 and 1.0

**Hypothesis:** MaxFP=0.75 (midpoint between 0.5 and 1.0) will reveal whether the balance curve is gradual or sharp.

**Run:** 200 matches, seed 1000, `--on-timeout energy`, default arena (800×600), BlueEcm MaxFP=0.75

**Results:**
- Red 82 (41%) / Blue 117 (58%) + 1 draw — **Blue dominant, similar to MaxFP=0.5 (60%)**
- RedGhost: MVP, **Linchpin** (alive:89%/dead:23%), solo carry 30/82 (37%) — Ghost critical to Red
- BlueEcm: MVP (83/117 = 71% WinSurv), **NOT Linchpin**, solo carry 32/117 (27%)
- BlueEcm is NOT Linchpin at 0.75 — Blue corps wins without it; ECM duel tilts to Ghost

**Curve table:**

| MaxFP | Red%  | Blue% | BlueEcm Linchpin? |
|-------|-------|-------|-------------------|
| 0.1   | 41%   | 59%   | No                |
| 0.5   | 40%   | 60%   | No                |
| 0.75  | 41%   | 58%   | No                |
| 1.0   | 51%   | 49%   | Yes (iter-11)     |
| 1.5   | 56%   | 44%   | Yes (baseline)    |

**Key finding:** Non-linearity is a single sharp threshold. The entire 0.1–0.75 range is one flat Blue-dominant regime. The Linchpin phase transition and balance crossover happen together between 0.75 and 1.0. MaxFP=1.0 is uniquely correct.
**Code:** Reverted — 0.75 is not a fix.

---

### Iter 13 — `research/iter-13-seed-3000-triple-validation`
**Date:** 2026-04-18
**Status:** ✅ CONFIRMED — Triple-seed validation complete (no code change)

**Hypothesis:** BlueEcm MaxFP=1.0 balance holds at seed 3000 (third independent seed).

**Run:** 200 matches, seed 3000, `--on-timeout energy`, default arena (800×600), BlueEcm MaxFP=1.0

**Results:**
- Red 101 (50.5%) / Blue 99 (49.5%) — ✅ closest to 50/50 of all three seeds
- Ghost: MVP, Linchpin (alive:96%/dead:22%), solo carry 48/101 = **48%** (highest across seeds)
- BlueEcm: MVP, Linchpin (alive:85%/dead:25%), solo carry 28/99 = **28%**
- Both ECM tanks simultaneously Linchpin — ECM duel at maximum tension, balance perfect

**Code:** No change — same iter-11 fix (BlueEcm MaxFP=1.0).

---

### Iter 12 — `research/iter-12-cross-seed-validation`
**Date:** 2026-04-18
**Status:** ✅ CONFIRMED — Balance fix seed-validated (no code change)

**Hypothesis:** BlueEcm MaxFP=1.0 balance holds at seed 2000 (not just seed 1000).

**Run:** 200 matches, seed 2000, `--on-timeout energy`, default arena (800×600), BlueEcm MaxFP=1.0

**Results:**
- Red 92 (46%) / Blue 108 (54%) — ✅ within 45–55% balance target
- Ghost: MVP, Linchpin (alive:92%/dead:21%), solo carry 39/92 = **42%**
- BlueEcm: MVP, solo carry 24/108 = **22%**

**Code:** No change — same iter-11 fix (BlueEcm MaxFP=1.0).

---

### Iter 11 — `research/iter-11-blueecm-fp-calibrate-0.5`
**Date:** 2026-04-18
**Status:** ✅ CONFIRMED — BALANCE FIX FOUND

**Hypothesis:** Binary search between MaxFP=0.1 (Blue59%) and 1.5 (Red56%) — try 0.5, then 1.0.

**Binary search runs:**
- MaxFirePower=0.5: Red 40%, Blue 60% — still Blue-dominated
- MaxFirePower=1.0: Red **51%**, Blue **49%** — ✅ BALANCED (target: 45–52%)

**Code change KEPT:** BlueEcm MaxFirePower 1.5 → 1.0

**Root cause:** Ghost (MaxFP=0.1) is a hider; BlueEcm (1.5) over-fires, drains energy, dies in Red wins. Fix: 1.5→1.0 balances the ECM duel.

**Awaiting human merge.**

---

### Iters 1–10
See prior branch history. Key findings:
- Red 56% baseline is structural (ECM parameter sweeps all return 56%)
- Ghost's hider role (MaxFP=0.1) worth +16pp (iter-8)
- BlueEcm MaxFP=0.1 over-corrects to Blue 59% (iter-10)
- Balance insensitive to ECM params, ECM modes, arena ECM geometry
