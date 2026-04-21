# Red Engineering — Loop State

## Last Updated
2026-04-20 — Iteration 22 complete (PR=140 REFUTED; -4.5pp; PR=160 confirmed optimal)

## Current Iteration
**23** — pending

## Situation

**DOMINANT: 65.9% average win rate across 5 seeds against current Blue DLL (net10.0).**

**CRITICAL INFRASTRUCTURE NOTE:** Always use `net10.0/publish/` DLL paths, NOT `net9.0/publish/`.
The `net9.0` DLL is stale and missing Trooper (produces 4-tank Red, 43% win rate).
- Red: `TankSwarmCode.SwarmTanks.Red/bin/Release/net10.0/publish/TankSwarmCode.SwarmTanks.Red.dll`
- Blue: `TankSwarmCode.SwarmTanks.Blue/bin/Release/net10.0/publish/TankSwarmCode.SwarmTanks.Blue.dll`
- Build: `dotnet publish TankSwarmCode.SwarmTanks.Red/... -c Release`

**Current win rates (5-seed parallel, 200 matches each, 5 tanks vs current Blue):**
- Seed 1000: **66.0%**
- Seed 2000: **72.0%**
- Seed 3000: **69.0%**
- Seed 4000: **66.0%**
- Seed 5000: **63.0%**
- **5-seed average: 67.2%**

## What We Know (Current Blue DLL)

- **Parallel mode is VALID for Red.** Iter 17 fixed the static SwarmCoordinator registry bug.
  Use `--parallel 8` for all runs.
- **Faster bullets (MaxFP=1.5) beat higher power (2.0) at PR=160 — for 4 of 5 tanks.** Confirmed
  for Arrow (iter-14), Trooper (iter-18), Blade+Hammer (iter-19). Ghost is the exception: Ghost
  MaxFP=1.5 was refuted (-1.5pp). Ghost stays at 2.0. MaxFP sweep is complete.
- **5th tank (RedTrooper) is essential.** 5v5 parity where Red's coordination wins.
- **Do NOT change Pincer/Encircle to orbit-based positioning.** Strategy transition disruption.
- **Frequency constants (AllyPingInterval=15, VolleyIntervalTicks=30) must NOT be changed.**
- **BlueSharp linchpin targeting is NOT exploitable.** Priority-targeting Sharp (iter-21) gave -0.3pp.
  The correlation (Sharp alive → Blue wins) is observational; weakest-first targeting is confirmed
  optimal. The linchpin stat is informational only, not an actionable targeting strategy.

## Current Configuration

| Tank | Slot | MaxFP | PR | HasEcm | Retreat |
|------|------|-------|----|--------|---------|
| Hammer | 0 | 1.5 | 160 | false | 25 |
| Blade | 1 | 1.5 | 160 | false | 20 |
| Arrow | 2 | 1.5 | 160 | false | 20 |
| Ghost | 3 | **2.0** | 160 | false | 20 |
| **Trooper** | **4** | **1.5** | **160** | **false** | **20** |

**SwarmCoordinator.ExecuteWolfpack:** orbit-based (`slot % aliveCount * 360/aliveCount`, radius = `config.PreferredRange`)

## Next Hypothesis (Iteration 23)

**Primary options:**
1. PR=150 (midpoint — maybe 140 too close, 160 not close enough; test 150 as midpoint)
2. PR=180 (further orbit — less return fire, slower bullets, longer engagement range)
3. Ghost PR=140 inner orbit only (Ghost at MaxFP=2.0 at 140 → 10.0t travel, similar to others)
4. Retreat threshold tuning (Hammer=25 vs others=20; is aggressive retreat better/worse?)
5. All tanks PR=170 or 180 (further out, less return fire)

**Top pick:** PR=150 for all tanks. PR=160 is optimal and PR=140 is -4.5pp. PR=150 is the midpoint —
fast 3-seed test to confirm 160 is truly optimal or find a slight gain.

Success criteria: Red 3-seed avg changes by ≥2pp vs baseline; stop test if signal is clearly negative

---

## Iteration Log

### Iter 22 — PR=140 for All Tanks (REFUTED -4.5pp, 2 seeds)
**Date:** 2026-04-20
**Status:** REFUTED after 2 seeds. Seed 1000: -4.5pp, seed 2000: -4.5pp — consistent clear signal.
- Closer orbit at 140 hurts more than bullet speed gain helps; PR=160 confirmed optimal
- Orbit radius and MaxFP are coupled; disrupting one breaks the equilibrium
- See Research/iter-0022-pr-140-refuted.md for full details

### Iter 21 — BlueSharp Priority Targeting (REFUTED -0.3pp avg, neutral)
**Date:** 2026-04-20
**Status:** REFUTED. Targeting Sharp-first: 66.9% avg (5-seed) vs 67.2% baseline (-0.3pp, noise).
- Sharp correlation (alive=85% Blue wins) is observational, not causally exploitable via targeting
- Weakest-first targeting confirmed optimal; Sharp-first wastes DPS when Sharp has full energy
- See Research/iter-0021-bluesharp-priority-targeting.md for full details

### Iter 20 — Ghost MaxFP=1.5 (REFUTED -1.5pp avg)
**Date:** 2026-04-20
**Status:** REFUTED. Ghost MaxFP=1.5 = 65.7% avg (5-seed) vs 67.2% baseline (-1.5pp).
- 4/5 seeds negative; only seed 5000 positive (+3.5pp)
- Ghost is the exception to "faster bullets at PR=160" — MaxFP=2.0 remains optimal for Ghost
- MaxFP sweep complete: Arrow/Trooper/Blade/Hammer at 1.5; Ghost at 2.0 (confirmed separately)
- See Research/iter-0020-ghost-maxfp-1.5-refuted.md for full details

### Iter 19 — Blade+Hammer MaxFP 2.0→1.5 (ACCEPTED +1.3pp avg)
**Date:** 2026-04-20
**Status:** ACCEPTED. Red 67.2% avg (5-seed parallel) vs 65.9% baseline (+1.3pp).
- 4/5 seeds positive: seed 1000 +3.5pp, seed 2000 +1.5pp, seed 4000 +4.0pp
- Seeds 3000 and 5000 slightly negative (-1.5pp, -1.0pp) — within noise
- Pattern confirmed: Arrow+Trooper+Blade+Hammer all better at MaxFP=1.5. Ghost is last outlier.
- See Research/iter-0019-blade-hammer-maxfp-1.5.md for full details

### Iter 18 — Trooper MaxFP 2.0→1.5 (CONFIRMED +3.0pp avg)
**Date:** 2026-04-20
**Status:** CONFIRMED. Red 65.9% avg (5-seed parallel, net10.0 DLL) vs 62.9% baseline.
- Discovery: Prior runs used stale net9.0 DLL (4-tank Red, 43%). Fixed to net10.0/publish.
- MaxFP=1.5 5-seed results: 62.5/70.5/70.5/62.0/64.0 = **65.9% avg**
- Direct 3-seed A/B: MaxFP=1.5 = 67.8% vs MaxFP=2.0 = 63.7% (+4.1pp)
- Mechanism: faster bullets at PR=160 improve hit rate more than per-hit damage reduction costs
- Pattern now confirmed for Arrow (iter-14) and Trooper (iter-18). Blade+Hammer are next.
- See Research/iter-0018-trooper-maxfp-1.5.md for full details

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
