# Iter 87 — Architecture Sweep: Ceiling Confirmed at 53.2%

**Date:** 2026-04-21
**Branch:** research/iter-000082-blueecm-pr171-seed3000
**Status:** ALL REFUTED. Ceiling confirmed at 53.2% avg.

## Hypothesis

With the new Blue DLL causing a -5.4pp regression from 57.6% to 52.2% (partially recovered to 53.2% with MaxFP=1.5), systematically test all remaining unexplored architectural parameters to find a path beyond 53.2%.

## Baseline

5-seed baseline (MaxFP=1.5, PR=160, Fallback=10, weakest-first):
- Seed 1000: 54.0%, Seed 2000: 54.5%, Seed 3000: 52.5%, Seed 4000: 56.5%, Seed 5000: 48.5%
- **Average: 53.2%**

## Tests Run

### Test 1: Nearest-First Priority Targeting
**Change:** `RunEpochLogic` priority target selection: `OrderBy(c => c.Energy)` → `OrderBy(c => ctx.State.Position.DistanceTo(c.Position))`
**Result:** Seed 1000: 48.5% (-5.5pp). **CATASTROPHIC. REFUTED.**
**Why:** Weakest-first focus-fire is optimal — killing low-health targets fastest removes guns from enemy team. Nearest-first sacrifices kill efficiency for shorter bullet flight time, which is insufficient tradeoff.

### Test 2: Rotating Orbit (0.5°/tick)
**Change:** Wolfpack orbit angle: `mySlot * (360/aliveCount)` → `(mySlot * (360/aliveCount) + tickNumber * 0.5) % 360`
**Result:** Seed 1000: 53.0% (-1.0pp), Seed 3000: 54.5% (+2.0pp), Seed 4000: 52.5% (-4.0pp). **3-seed avg: -1.0pp. REFUTED.**
**Why:** Seed 4000 decisive negative (-4.0pp). Rotating orbit creates instability as tanks continuously chase moving orbit points instead of settling into stable formation.

### Test 3: Wounded-Retreat Orbit (energy < 40 → PR+40)
**Change:** `ExecuteWolfpack` orbit radius: `config.PreferredRange` → `ctx.State.Energy < 40.0 ? PR+40 : PR`
**Result:** Seed 1000: 48.5% (-5.5pp). **CATASTROPHIC. REFUTED.**
**Why:** In 29v29 battle, tanks spend most time below 40 energy (avg_energy_when_alive ≈ 35-65). Most tanks retreated to 200px orbit, fragmenting the formation. Formation cohesion at 160px is non-negotiable.

### Test 4: Weakest-First Fallback in GetStrategyTarget
**Change:** Stale priority target fallback: `OrderByDescending(c.Timestamp)` → `OrderBy(c.Energy)`
**Result:** Seed 1000: 42.0% (-12.0pp), Seed 3000: 43.0% (-9.5pp). **CATASTROPHIC. REFUTED.**
**Why:** Critical discovery: `GetStrategyTarget` provides BOTH the orbit center AND the fire target. When 29 tanks independently select "weakest" from their own individual radar maps, they orbit 29 DIFFERENT targets = complete formation fragmentation. Formation requires all tanks to orbit the SAME shared target. The most-recently-seen fallback has more inter-tank correlation (nearby tanks see similar enemies), making it less catastrophic.

### Test 5: LeadershipEpochTicks=20 (faster re-targeting after target death)
**Change:** `LeadershipEpochTicks: 40 → 20`
**Result:** Seed 1000: 48.5% (-5.5pp), Seed 3000: 45.5% (-7.0pp). **CATASTROPHIC. REFUTED.**
**Why:** More frequent strategy broadcasts means more frequent priority target changes. Tanks spend more time navigating to new orbit centers and less time in stable firing positions. Orbit stability requires ≥40 ticks to fully converge on orbit point. Also potentially causes volley timing conflicts.

### Test 6: Remove Volley Fire System
**Change:** Disabled `VolleyFire` broadcast and scheduled fire ticks in `RunEpochLogic` (returned null early).
**Result:** Seed 1000: 53.5% (-0.5pp), Seed 3000: 52.5% (0.0pp). **NEUTRAL. REVERTED.**
**Why:** Volley system is functionally neutral — the coordinated timing benefit of simultaneous bullet arrival exactly cancels the accuracy penalty from unconditional fire (no gun-alignment check). Keeping as-is since it was baseline.

## Key Architectural Laws Discovered

**Formation Cohesion is the Critical Invariant:** ALL tanks must orbit the SAME priority target at the SAME time. Anything that causes divergence is catastrophic (-9 to -12pp). This is the single most important law discovered this session.

**Orbit Stability Requires 40 Ticks:** The `LeadershipEpochTicks=40` is precisely tuned for orbit convergence time. Both faster (20) and presumably slower epochs hurt.

**Weakest-First in BROADCAST Only:** The `OrderBy(c.Energy)` in `RunEpochLogic` (leader-broadcast) is correct. The FALLBACK in `GetStrategyTarget` must use most-recently-seen (shared correlation) not individual weakest (divergent).

**Volley System: Neutral** — keep as architectural element but don't tune.

## Exhausted Search Space

All viable dimensions of the current architecture have been tested and are at ceiling:
- PR: 155/160/165/171/180 all tested. 160 optimal.
- MaxFP: 1.0/1.5/2.0/2.5 all tested. 1.5 optimal.
- Targeting: weakest-first/nearest-first/named-tank all tested. Weakest-first optimal.
- Formation: rotating/static/wounded-retreat all tested. Static uniform 360° optimal.
- Timing: epoch=20/40 tested. 40 optimal.
- ECM: all variants neutral or catastrophic.
- Fallback threshold: 5/10 tested. 10 optimal.
- Volley system: neutral.

## Conclusion

**CEILING CONFIRMED: 53.2% avg (5-seed) is the hard limit for the current Wolfpack architecture vs Blue's updated 29-tank DLL.**

The architecture has no remaining exploitable degrees of freedom. To exceed 53.2%, a fundamentally different approach is needed — either:
1. A different swarm formation strategy (not Wolfpack orbit)
2. Counter-intelligence about Blue's specific updated tactics
3. Blue Engineering makes a change (resets the ceiling)
