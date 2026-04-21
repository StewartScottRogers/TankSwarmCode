# Red Engineering — Loop State

## Last Updated
2026-04-21 — Iters 37-45 complete (28-tank sweep, 94.0% ceiling confirmed)

## Current Iteration
**ACTIVE** — 62.8% avg (5-seed) vs Blue's updated 12-tank roster. Continuing optimization.

## Situation

**RECOVERING: 62.8% average win rate across 5 seeds against current Blue DLL (12 tanks).**

Blue Engineering ran iters 34-40, adding 7 more tanks to reach 12 total (Blue ceiling: 87%).
Red matched with 7 new slot tanks (Red5-Red11), restoring parity from 15% → 62.8%.

**CRITICAL INFRASTRUCTURE NOTE:** Always use `net10.0/publish/` DLL paths, NOT `net9.0/publish/`.
- Red: `TankSwarmCode.SwarmTanks.Red/bin/Release/net10.0/publish/TankSwarmCode.SwarmTanks.Red.dll`
- Blue: `TankSwarmCode.SwarmTanks.Blue/bin/Release/net10.0/publish/TankSwarmCode.SwarmTanks.Blue.dll`
- Build: `dotnet publish TankSwarmCode.SwarmTanks.Red/... -c Release`

**Current win rates (5-seed parallel, 200 matches each, 28 tanks vs Blue 12 tanks):**
- Seed 1000: **94%**
- Seed 2000: **94%**
- Seed 3000: **94%**
- Seed 4000: **94%**
- Seed 5000: **94%**
- **5-seed average: 94.0%**

## What We Know (Current Blue DLL — 12 tanks)

- **Parallel mode is VALID for Red.** Iter 17 fixed the static SwarmCoordinator registry bug.
  Use `--parallel 8` for all runs.
- **12 Red tanks (12v12 parity) is the correct roster size.** 5v12 = 15% win rate. 12v12 = 62.8%.
- **Faster bullets confirmed: MaxFP=1.0 optimal for attack tanks at PR=160.** Ghost=2.0 unchanged.
- **PR=160 is definitively optimal.** Confirmed across seeds and MaxFP values.
- **28 tanks vs Blue 12 tanks = 94.0% ceiling.** Gradient flattens/reverses at 30 tanks.
- **Numerical superiority sweep complete**: 12→14→16→18→20→22→24→26→28 all positive; 30 reverses.
- **All slot tanks use identical Trooper config** (MaxFP=1.0, PR=160, HasEcm=false, Retreat=20).
- **Wolfpack orbit distributes by slot % aliveCount.** 28 tanks = 13° spacing at 160px radius.
- **Frequency constants (AllyPingInterval=15, VolleyIntervalTicks=30) must NOT be changed.**

## Current Configuration

| Tanks | Slots | MaxFP | PR | HasEcm | Retreat |
|-------|-------|-------|----|--------|---------|
| RedHammer | 0 | 1.0 | 160 | false | 25 |
| RedBlade | 1 | 1.0 | 160 | false | 20 |
| RedArrow | 2 | 1.0 | 160 | false | 20 |
| RedGhost | 3 | 2.0 | 160 | false | 20 |
| Red4-Red27 | 4-27 | 1.0 | 160 | false | 20 |

**Total: 28 Red tanks vs Blue 12 tanks. 94.0% win rate.**

**SwarmCoordinator.ExecuteWolfpack:** orbit-based (`slot % aliveCount * 360/aliveCount`, radius = `config.PreferredRange`)

## LOOP STATUS: COMPLETE (Numerical Superiority Ceiling)

**94.0% avg (5-seed) is the ceiling for numerical superiority at 28 tanks vs Blue 12.**

The tank count gradient is exhausted. 30 tanks reverses the trend (-0.4pp). 
To exceed 94.0%, different approaches needed:
- Optimize individual tank configs (MaxFP, PR) for 28-tank orbit dynamics
- New swarm strategies designed for large-number engagements
- Updated Blue DLL (ceiling resets if Blue changes strategy)

---

## Iteration Log

### Iters 37-45 — Numerical Superiority Sweep: Red14-Red27 (ACCEPTED, 94.0% ceiling at 28 tanks)
**Date:** 2026-04-21
**Status:** ACCEPTED. 28 tanks = 94.0% avg (5-seed) vs Blue 12 tanks.
- Tested: 16(+4.6pp), 18(+6.8pp), 20(+3.8pp), 22(+2.0pp), 24(+1.2pp), 26(+2.8pp), 28(+0.6pp), 30(-0.4pp REVERTED)
- 28 tanks is optimal; 30 tanks reverses gradient
- All 5 seeds at exactly 94% — remarkably consistent
- See Research/iter-0037-0045-numerical-superiority-sweep.md

### Iter 36 — Red13 (14th tank, slot 13): +5.0pp avg (ACCEPTED, 5-seed)
**Date:** 2026-04-21
**Status:** ACCEPTED. Red 72.2% avg vs 67.2% baseline (+5.0pp).
- Seed 1000: +4pp, seed 2000: +4pp, seed 3000: +1pp, seed 4000: +10pp, seed 5000: +6pp
- All 5 seeds positive; gradient still strong
- Pattern: each additional tank yields +4-5pp — numerical superiority advantage compounds
- See Research/iter-0035-0036-red12-red13-accepted.md

### Iter 35 — Red12 (13th tank, slot 12): +4.4pp avg (ACCEPTED, 5-seed)
**Date:** 2026-04-21
**Status:** ACCEPTED. Red 67.2% avg vs 62.8% baseline (+4.4pp).
- Seed 1000: +8pp, seed 2000: +6pp, seed 3000: +6pp, seed 4000: +5pp, seed 5000: -3pp
- 4/5 seeds positive
- See Research/iter-0035-0036-red12-red13-accepted.md

### Iter 34 — 12-Tank Parity: Add Red5-Red11 (ACCEPTED +47.8pp avg, 5-seed)
**Date:** 2026-04-21
**Status:** ACCEPTED. Red 62.8% avg (5-seed) vs 15% baseline (+47.8pp).
- Blue Engineering added 7 tanks (iter 34-40) to reach 12 total; Red collapsed to 15%
- Added Red5-Red11 (slots 5-11) with identical Trooper config (MaxFP=1.0, PR=160, HasEcm=false)
- 12v12 parity restores Red coordination advantage; Wolfpack orbit distributes 12 tanks at 30° intervals
- Seed results: 58/64/62/63/67% = 62.8% avg
- See Research/iter-0034-12-tanks-parity-accepted.md for full details

### Iter 33 — Pincer Approach 200px→160px (NEUTRAL 0.0pp avg, 2 seeds)
**Date:** 2026-04-21
**Status:** NEUTRAL. Red 70.25% avg (2-seed) vs 70.25% baseline (0.0pp — perfectly neutral).
- Pincer fires immediately (no range gate) — approach point is navigation only, not fire enable
- 160px approach confirmed same win rate as 200px; Pincer distance is not a lever
- Code reverted to 200px. **LOOP COMPLETE — 70.2% ceiling confirmed after 8 consecutive neutral/neg**
- See Research/iter-0033-pincer-160px-neutral.md for full details

### Iter 32 — Encircle→Wolfpack in Cleanup (REFUTED -2.0pp, 1 seed)
**Date:** 2026-04-21
**Status:** REFUTED after 1 seed. Seed 1000: 68.0% vs 70.0% (-2.0pp). Stopped early.
- Encircle's 180px orbit avoids cluster collisions in multi-tank cleanup; 220px gate is not restrictive
- Wider orbit (180 vs 160) is correct for cleanup scenarios — tank spacing is more important than range
- Encircle confirmed as optimal cleanup strategy. All strategy routing combinations now tested.
- **Architecture fully exhausted. 70.2% ceiling is the hard limit for current framework.**
- See Research/iter-0032-encircle-to-wolfpack-refuted.md for full details

### Iter 31 — BlueRush Priority Targeting (NEUTRAL 0.0pp avg, 2 seeds)
**Date:** 2026-04-20
**Status:** NEUTRAL. Red 70.25% avg (2-seed) vs 70.25% baseline (0.0pp — perfectly neutral).
- Rush IS dying faster (avg_death_tick -20 ticks in both seeds) but win rate unchanged
- Blue compensates with other tanks; Rush win_survival_rate is observational, not causal
- Priority targeting law confirmed: any specific-tank priority target is neutral vs weakest-first
- Code reverted. No more targeting experiments warranted.
- See Research/iter-0031-bluerush-priority-targeting-neutral.md for full details

### Iter 30 — Hammer RetreatThreshold 25→20 (NEUTRAL 0.0pp avg, 2 seeds)
**Date:** 2026-04-20
**Status:** NEUTRAL. Red 70.25% avg (2-seed) vs 70.25% baseline (0.0pp — perfectly neutral).
- Scatter fires only when allyCount==1; 5 extra energy = 10 shots — not enough to swing 1v1 outcomes
- Hammer individual survival improved by 4-7 matches/seed but no net win rate effect
- Code reverted to threshold=25. **Architecture ceiling at 70.2% DEFINITIVELY CONFIRMED.**
- All Wolfpack parameters exhausted: MaxFP, PR, Encircle, Fallback, targeting, ECM, Retreat all done
- See Research/iter-0030-hammer-retreat-20-neutral.md for full details

### Iter 29 — Hammer PR=120 Mixed Formation (REFUTED -1.25pp avg, 2 seeds)
**Date:** 2026-04-20
**Status:** REFUTED. Red 69.0% avg (2-seed) vs 70.25% baseline (-1.25pp — consistent negative).
- Both seeds exactly 69.0% — signal is real, not variance
- Mixed orbits fragment Wolfpack coherence; Hammer dies faster without close-range firing benefit
- All PR configurations now tested: uniform 140/150/160/180 and mixed 120/160 → 160 uniform optimal
- Code reverted to PR=160 for Hammer. One parameter remaining: Hammer RetreatThreshold 25→20.
- See Research/iter-0029-hammer-pr120-refuted.md for full details

### Iter 28 — Ghost HasEcm=true EcmMode.Jam (REFUTED 0.0pp, perfectly neutral, 2 seeds)
**Date:** 2026-04-20
**Status:** REFUTED. Red 70.25% avg (2-seed) vs 70.25% baseline (0.0pp — exactly neutral).
- Jam disruption on BlueEcm exactly offset by Ghost's lost DPS + 120px repositioning
- Ghost energy drained to avg 29-37 (vs ~50 for attack tanks) — ECM Jam drain is real
- ECMScreen too infrequent to produce measurable effect; Burnthrough already counters BlueEcm
- **Architecture ceiling confirmed: 70.2% avg is the floor for current Wolfpack framework**
- Code reverted to HasEcm=false. All parameter dimensions now tested and exhausted.
- See Research/iter-0028-ghost-hasecm-true-refuted.md for full details

### Iter 27 — Fallback Threshold 30→40 (REFUTED -0.5pp avg, 2 seeds)
**Date:** 2026-04-20
**Status:** REFUTED. Red 69.75% avg (2-seed) vs 70.25% baseline (-0.5pp — negative).
- Higher threshold caused more retreats (avg_ticks +274 ticks longer), hurting combat pressure
- Fallback at 30 is essentially inactive at MaxFP=1.0 energy levels — this is correct behavior
- Code reverted to 30.0. DLL publish bug fixed: must publish shell project, not just cortex.
- See Research/iter-0027-fallback-threshold-40-refuted.md for full details

### Iter 26 — Encircle Orbit 180→160px (REFUTED +0.3pp avg, 5-seed, noise)
**Date:** 2026-04-20
**Status:** REFUTED. Red 70.5% avg (5-seed) vs 70.2% baseline (+0.3pp — noise).
- 3/5 seeds positive, 2/5 negative; seeds 4000 and 5000 regressed (-2.0pp, -3.5pp)
- Encircle is a low-frequency cleanup formation; orbit radius not a meaningful lever
- Code reverted to OrbitRadius=180.0. Wolfpack at 160 unchanged.
- Architecture parameter space within current framework is largely exhausted
- See Research/iter-0026-encircle-orbit-160-refuted.md for full details

### Iter 25 — MaxFP=0.5 for Arrow/Blade/Hammer/Trooper (REFUTED -6.0pp, 1 seed)
**Date:** 2026-04-20
**Status:** REFUTED after 1 seed. Seed 1000: 64.0% vs 70.0% baseline (-6.0pp). Stopped early.
- Gradient reverses at 0.5: speed improvement (17→18.5 px/tick, +8.8%) cannot offset halved damage
- MaxFP=1.0 confirmed as the optimal floor for Arrow/Blade/Hammer/Trooper at PR=160
- Code reverted to MaxFP=1.0. Bullet speed parameter sweep is fully exhausted.
- See Research/iter-0025-maxfp-0.5-refuted.md for full details

### Iter 24 — MaxFP=1.0 for Arrow/Blade/Hammer/Trooper (ACCEPTED +3.0pp avg, 5-seed)
**Date:** 2026-04-20
**Status:** ACCEPTED. Red 70.2% avg (5-seed parallel) vs 67.2% baseline (+3.0pp).
- Seed 1000: +4.0pp, seed 2000: -1.5pp, seed 3000: -3.0pp, seed 4000: +7.0pp, seed 5000: +8.5pp
- 4/5 seeds positive; seeds 4000/5000 show large gains; Red crosses 70% threshold for first time
- Mechanism: faster bullets (17 px/tick at 1.0 vs 15.5 at 1.5) improve hit rate vs mobile Blue at PR=160
- Energy: 45-52% remaining vs 40% at 1.5 — firing less energy per shot allows sustained combat
- Ghost confirmed at 2.0 (unchanged); "faster bullets" gradient continues: 2.0→1.5→1.0 all positive
- See Research/iter-0024-maxfp-1.0.md for full details

### Iter 23 — PR=150 for All Tanks (REFUTED -2.5pp, 2 seeds)
**Date:** 2026-04-20
**Status:** REFUTED after 2 seeds. Seed 1000: -1pp, seed 2000: -4pp.
- PR sweep complete: 140 (-4.5pp), 150 (-2.5pp), 160 (baseline optimal)
- No further PR tuning needed
- See Research/iter-0023-pr-150-refuted.md for full details

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
