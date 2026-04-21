# iter-0014 — Serial Rebase + Wolfpack Orbit Breakthrough

## Context

All iterations 0–13 were measured in parallel mode (`--parallel` > 1). Subsequent investigation
confirmed that `SwarmCoordinator.Registry` is a **static `ConcurrentDictionary`** shared across all
parallel matches. `OnStart` calls `Reset()` which wiped coordinator state mid-game for any
concurrently running match. All parallel measurements are **invalid and discarded**.

This iteration re-establishes a true serial baseline, fixes the ECM drain issue, and discovers the
major Wolfpack orbit breakthrough.

---

## Phase 1 — True Serial Baseline

**Run Parameters:** `--batch 200 --parallel 1 --on-timeout energy`

**Config:** iter-0 values (Ghost MaxFP=0.1, HasEcm=true, JamAndSpoof, RetreatThreshold=40, PR=150;
Arrow PR=180; Blade MaxFP=2.5/PR=160; Hammer MaxFP=3.0/PR=200)

| Seed | Win Rate |
|------|---------|
| 1000 | 39% |
| 2000 | 34% |
| 3000 | 41.5% |
| **Avg** | **38.2%** |

**Key Finding:** True baseline is 38.2%, not 48% (the parallel-contaminated value from iter-12).

---

## Phase 2 — ECM Root Cause + Ghost Rewrite

**Hypothesis:** Ghost's JamAndSpoof ECM is actually hurting the team. Analysis: JamAndSpoof
disables Ghost's own radar, but allies continuously broadcast `RadarShare` messages. Ghost's
`RadarMap` stays fresh → `hasEnemy` stays permanently `true` → Ghost jams continuously →
1.3 energy/tick drain → Ghost dies at tick ~76–86. MaxFP=0.1 contributes zero combat value.

**Code Change:**
- Removed Ghost's proactive `OnTick` ECM override
- Ghost: MaxFP=0.1 → **2.0**, HasEcm=true → **false**, RetreatThreshold=40 → **20**, PR stays 150

**Results (3-seed serial):**

| Seed | Baseline | Ghost MaxFP=2.0 | Δ |
|------|----------|-----------------|---|
| 1000 | 39.0% | 47.0% | +8pp |
| 2000 | 34.0% | 43.0% | +9pp |
| 3000 | 41.5% | 45.5% | +4pp |
| **Avg** | **38.2%** | **45.2%** | **+7pp** |

**CONFIRMED BREAKTHROUGH: +7pp**

---

## Phase 3 — Config Tuning (all serial, seed 1000 scout + multi-seed confirm)

| Change | S1000 | Result |
|--------|-------|--------|
| Ghost PR=180 | 39.5% | Regression -7.5pp — REVERTED |
| Ghost MaxFP=2.5 | 41.5% | Regression -5.5pp (energy burn) — REVERTED |
| Hammer MaxFP=2.0 | 47.0% | Neutral/slight +0.6pp avg (Hammer survival +9pp) — KEPT |
| Blade MaxFP=2.0 | 49.0% | Seed-dependent, avg +0.8pp total — KEPT |
| Arrow PR=220→160 + tight formation | 45.5%/46.5%/54.5% | Avg 48.8% (3-seed) — KEPT |
| Arrow MaxFP=2.0 | 41.0% | Regression — REVERTED |
| All PR=150 | 44.1% avg | Regression — REVERTED |

**Formation tightening result (5-seed, all PR=160 except Ghost=150, all MaxFP=2.0 except Arrow=1.5):**

| Seed | Win Rate |
|------|---------|
| 1000 | 45.5% |
| 2000 | 46.5% |
| 3000 | 54.5% |
| 4000 | 48.0% |
| 5000 | 43.5% |
| **Avg** | **47.6%** |

---

## Phase 4 — Wolfpack Orbit Positioning (MAJOR BREAKTHROUGH)

**Hypothesis:** In Wolfpack mode, all Red tanks navigate directly toward the target and stop at
their PreferredRange. This causes all tanks to approach from the same direction, clustering on one
side of the target. Blue's 5 tanks can concentrate fire on the clustered Red group. If Red tanks
spread 90° apart around the target, they create crossfire from multiple angles simultaneously.

**Code Change:** `SwarmCoordinator.ExecuteWolfpack` — change from direct navigation to orbit-point
navigation:

```csharp
// Before:
TankNavigation.NavigateTo(ctx, target.Position, config.PreferredRange);

// After:
int aliveCount = GetAliveAllyCount() + 1;
int mySlot = config.FormationSlot % aliveCount;
double orbitAngleDeg = mySlot * (360.0 / aliveCount);
Vector2D orbitPoint = target.Position.PolarOffset(orbitAngleDeg, config.PreferredRange);
TankNavigation.NavigateTo(ctx, orbitPoint, 0);
```

**Config at test time:** All tanks PR=160, Arrow MaxFP=1.5, others MaxFP=2.0

**Results (5-seed serial, 200 matches each):**

| Seed | Pre-orbit | Post-orbit | Δ |
|------|-----------|------------|---|
| 1000 | 45.5% | **66.0%** | +20.5pp |
| 2000 | 46.5% | **67.0%** | +20.5pp |
| 3000 | 54.5% | **57.5%** | +3pp |
| 4000 | 48.0% | **65.5%** | +17.5pp |
| 5000 | 43.5% | **65.0%** | +21.5pp |
| **Avg** | **47.6%** | **64.2%** | **+16.6pp** |

400-match validation at seed 1000: **62.2%** (249/400) — confirmed stable.

**CONFIRMED BREAKTHROUGH: +16.6pp, total improvement from baseline = +26pp**

---

## Phase 5 — Orbit Radius Tuning

All with orbit-based Wolfpack. Testing PR variations:

| PR | 5-seed avg |
|----|-----------|
| 130 | 60.2% |
| **160** | **64.2%** (baseline) |
| 180 | 61.0% |

PR=160 confirmed optimal. Too close (130) increases damage taken; too far (180) reduces DPS.

## Phase 6 — Orbit Slot Assignment Variants

Testing alternative angle assignment methods:

| Method | 5-seed avg |
|--------|-----------|
| `slot % aliveCount` (original) | **64.2%** |
| Fixed 4-slot 90° spacing | 63.3% |
| Rank-based among alive allies | 62.0% |

Dynamic modulo (`slot % aliveCount`) is empirically best. Fixed-slot and rank-based methods both
performed worse, suggesting the collision behavior in specific tank-death scenarios is benign or
even helpful.

## Phase 7 — Pincer/Encircle Orbit Alignment (REFUTED)

Updated Pincer and Encircle to also use orbit-based positioning (matching Wolfpack). Avg 59.3% —
regression vs Wolfpack-orbit-only (64.2%). Strategy transitions between modes cause tank
repositioning disruption that hurts performance. Original Pincer and Encircle behavior restored.

---

## Final Configuration

| Tank | Slot | MaxFP | PR | HasEcm | Retreat |
|------|------|-------|----|--------|---------|
| Hammer | 0 | 2.0 | 160 | false | 25 |
| Blade | 1 | 2.0 | 160 | false | 20 |
| Arrow | 2 | 1.5 | 160 | false | 20 |
| Ghost | 3 | 2.0 | 160 | false | 20 |

**SwarmCoordinator.ExecuteWolfpack:** orbit-based (slot % aliveCount, radius = config.PreferredRange)

**5-seed win rates: 66.0% / 67.0% / 57.5% / 65.5% / 65.0% = avg 64.2%**

---

## Key Findings

1. **Parallel mode is permanently broken for this codebase.** Static registry shared across matches.
   All results from iters 0–13 are invalid.

2. **Ghost ECM was self-destructive.** JamAndSpoof + RadarShare = infinite jam loop = energy drain
   death at tick ~80. Removing ECM and enabling combat (MaxFP=2.0) was +7pp.

3. **Wolfpack orbit positioning is the dominant improvement.** +16.6pp. Distributing Red tanks
   90° apart around the priority target creates crossfire that Blue cannot escape.

4. **Arrow MaxFP=1.5 is the unique role.** Faster bullets (speed 15.5 vs 14) at close range
   provide better hit rate. Arrow MaxFP=2.0 was a regression in all tests.

5. **Orbit radius PR=160 is optimal.** Both 130 and 180 are worse. Balance between accuracy
   and survivability.

6. **Do NOT change Pincer/Encircle to orbit.** Strategy transitions cause repositioning disruption.
   Only Wolfpack uses orbit; other strategies retain original behavior.
