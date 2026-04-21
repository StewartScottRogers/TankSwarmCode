# Red Engineering — Loop State

## Last Updated
2026-04-20 — Iteration 17 complete (parallel mode bug fix — static SwarmCoordinator registry)

## Current Iteration
**18** — pending

## Situation

**DOMINANT: 62.9% average win rate across 5 seeds against current Blue DLL.**

**IMPORTANT: The LoopState baseline of 64.2% from iter-14 is STALE.** Blue improved their DLL
with two commits:
1. Wolfpack angle-offset formation for Blue (+3.8pp Blue perspective) — `871c931`
2. Blue orbit angle step changed from 72° to 60° — `00f7f08`

**True baseline (4 tanks vs NEW Blue):** ~50.2% avg (seeds 1000/2000/3000: 50.5/55.5/44.5%)

**Current win rates (5-seed serial, 200 matches each, 5 tanks vs current Blue):**
- Seed 1000: **63.5%** (400-match validation: **65.8%**)
- Seed 2000: **60.5%**
- Seed 3000: **70.0%**
- Seed 4000: **62.5%**
- Seed 5000: **58.0%**
- **5-seed average: 62.9%**

## What We Know (Current Blue DLL)

- **Parallel mode is NOW VALID for Red.** Iter 17 fixed the static SwarmCoordinator registry bug
  (`ForTeam()` → `new SwarmCoordinator()`). Parallel results now match serial. Use `--parallel 8`.
- **Blue improved significantly between iter-14 and iter-16.** Two Blue commits to their
  SwarmCoordinator brought them from losing 64% to approximately 50/50 against our 4-tank config.
  Always re-baseline before comparing results.
- **5th tank (RedTrooper) is the breakthrough vs new Blue.** +12.7pp. With Blue running 5 well-
  coordinated tanks using 60° orbit angle steps, our 4-tank config faced a structural numerical
  disadvantage. Adding Trooper creates 5v5 parity where Red's coordination wins.
- **RedTrooper was already in the project.** Just needed a parameterless constructor to be
  auto-instantiated by the arena engine.
- **All iter-15 findings remain valid for 4-tank config.** The 64.2% ceiling was against OLD Blue.
  Against new Blue, the ceiling for 4 tanks is ~50%.
- **Arrow MaxFP=1.5 is optimal.** Confirmed in iter-14. Faster bullets at PR=160 improve hit rate.
- **PR=160 orbit radius is optimal for all tanks.** Not re-tested for 5th tank, assumed same.
- **Do NOT change Pincer/Encircle to orbit-based positioning.** Strategy transition disruption.
- **Frequency constants (AllyPingInterval=15, VolleyIntervalTicks=30) must NOT be changed.**
- **BlueSharp is Blue's Linchpin** (alive: 85% Blue wins, dead: 24% Blue wins). Killing Sharp
  early collapses Blue's win rate. This is the highest-value targeting opportunity.
- **BlueEcm is All-in** (never survives a loss). ECM suppression hurts Red but BlueEcm dies in
  every Red win — ECM alone is not why Blue wins.

## Current Configuration

| Tank | Slot | MaxFP | PR | HasEcm | Retreat |
|------|------|-------|----|--------|---------|
| Hammer | 0 | 2.0 | 160 | false | 25 |
| Blade | 1 | 2.0 | 160 | false | 20 |
| Arrow | 2 | 1.5 | 160 | false | 20 |
| Ghost | 3 | 2.0 | 160 | false | 20 |
| **Trooper** | **4** | **2.0** | **160** | **false** | **20** |

**SwarmCoordinator.ExecuteWolfpack:** orbit-based (`slot % aliveCount * 360/aliveCount`, radius = `config.PreferredRange`)

## Next Hypothesis (Iteration 18)

**Primary options:**
1. Trooper MaxFP=1.5 (match Arrow's confirmed-better power level; 5 tanks all at faster bullets)
2. Trooper PR tuning (is 160 still optimal for 5-tank formation? 130/180 not tested for 5 tanks)
3. Orbit angle step for 5-tank formation (72° equal vs 60° Blue-style offset)
4. Ghost PR=150 inner orbit (bringing Ghost closer for higher DPS while others hold 160)
5. Target BlueSharp first (Sharp is Blue's Linchpin — priority targeting change in SwarmCoordinator)

**Top pick:** Trooper MaxFP=1.5 (iter-17 plan, still untested). Arrow confirmed better at 1.5;
same logic applies to Trooper.

Success criteria: Red win rate increases by ≥3pp at seed 1000 (from 63.5% to ≥66.5%)

---

## Iteration Log

### Iter 0 — Serial Baseline Established
**Date:** 2026-04-20
**Status:** True serial baseline: 38.2% avg (39/34/41.5% seeds 1000/2000/3000)
- NOTE: All prior iters 0–13 used invalid parallel mode. Their findings are discarded.
- Ghost MaxFP=0.1, JamAndSpoof, proactive ECM — ECM was causing continuous drain death

### Iter 14 — Serial Rebase + Wolfpack Orbit (CONFIRMED +26pp total vs original baseline)
**Date:** 2026-04-20
**Status:** DOMINANT vs OLD Blue. Red 64.2% avg (5-seed serial) vs OLD Blue DLL.
- Phase 1: Ghost ECM removal + MaxFP=2.0: +7pp → 45.2% avg
- Phase 2: Formation tightening to PR=160 + Blade/Hammer MaxFP=2.0: +2.4pp → 47.6% avg
- Phase 3: Wolfpack orbit positioning: +16.6pp → 64.2% avg
- 400-match validation seed 1000: 62.2% (249/400)
- See Research/iter-0014-serial-rebase-and-wolfpack-orbit.md for full details

### Iter 15 — Post-14 Tuning Exhaustion (ALL REFUTED vs old Blue)
**Date:** 2026-04-20
**Status:** All tested changes negative vs iter-14 baseline (64.2% avg vs OLD Blue).
- Leader volley timing fix: 62.6% avg (-1.6pp)
- Ghost PR=150 inner orbit: 62.9% avg (-1.3pp)
- Velocity-led orbit prediction: 60.5% avg (-3.7pp)
- Fixed-slot orbit: 63.3% avg (-0.9pp)
- Rank-based orbit distribution: 62.0% avg (-2.2pp)
- Orbit-based Pincer/Encircle: 59.3% avg (-4.9pp)
- **Conclusion (vs OLD Blue):** 64.2% was ceiling for old Blue config family.

### Iter 17 — Fix Parallel Mode Static Registry Bug (CONFIRMED +47.5pp apparent, restored true baseline)
**Date:** 2026-04-20
**Status:** CONFIRMED. Red 63.5% (127/200) at seed 1000 in parallel mode. Matches serial baseline.
- Discovery: Baseline run in parallel mode showed Red 16% — 47pp below LoopState's 63.5%.
- Root cause: `RedCortexBase.OnStart` called `SwarmCoordinator.ForTeam(swarmId)` which uses a
  static `ConcurrentDictionary` registry, sharing ONE coordinator across all parallel games.
- Fix: `_swarm = new SwarmCoordinator()` (per-game, same fix Blue Engineering did in their iter 7).
- Post-fix seed 1000: 127/200 = 63.5% — identical to serial-mode baseline. Fix confirmed.
- BlueSharp identified as Blue's Linchpin (alive: 85%, dead: 24%) — new targeting intelligence.
- See Research/iter-0017-fix-parallel-mode-static-registry.md for full details

### Iter 16 — 5th Tank (5v5 Parity) vs Improved Blue (CONFIRMED +12.7pp)
**Date:** 2026-04-20
**Status:** DOMINANT. Red 62.9% avg (5-seed) vs new Blue DLL.
- Discovery: LoopState baseline was stale. New true baseline: ~50.2% avg (old Blue had improved).
- Blue's 60° angle orbit + BlueGuard MaxFP=3.0 created structural 5v5 disadvantage for 4-tank Red.
- Fix: Added parameterless `RedTrooper()` constructor → 5th tank deploys automatically.
- Trooper config: MaxFP=2.0, PR=160, FormationSlot=4(dynamic), Retreat=20
- 400-match validation seed 1000: 65.8% (263/400)
- See Research/iter-0016-5th-tank-5v5-parity.md for full details
