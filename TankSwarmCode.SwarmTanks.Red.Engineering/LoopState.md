# Red Engineering — Loop State

## Last Updated
2026-04-20 — Iteration 14 complete (serial rebase + Wolfpack orbit breakthrough)

## Current Iteration
**15** — pending

## Situation

**DOMINANT: 64.2% average win rate across 5 seeds.** All iters 0–13 were measured in
**parallel mode** which is permanently broken (static `SwarmCoordinator.Registry` shared across
matches, `Reset()` corrupts mid-game state). Those results are **invalid and discarded**.

**True serial baseline (iter-0 config):** 38.2% avg (seeds 1000/2000/3000: 39/34/41.5%)

**Current win rates (5-seed serial, 200 matches each):**
- Seed 1000: **66.0%**
- Seed 2000: **67.0%**
- Seed 3000: **57.5%**
- Seed 4000: **65.5%**
- Seed 5000: **65.0%**
- **5-seed average: 64.2%**

## What We Know (Serial)

- **Parallel mode is INVALID.** Never use `--parallel` > 1 for measurements. Static registry is shared.
- **Ghost ECM was self-destructive.** JamAndSpoof + RadarShare broadcasts = infinite jam loop = death ~tick 80. Removing ECM and enabling MaxFP=2.0 was +7pp.
- **Wolfpack orbit positioning is the dominant improvement.** +16.6pp. `slot % aliveCount` assigns each Red tank a 90° orbit angle around the priority target, creating crossfire Blue cannot escape.
- **Arrow MaxFP=1.5 is optimal.** Faster bullets (speed 15.5 vs 14 at MaxFP=2.0) at PR=160 improve hit rate. MaxFP=2.0 for Arrow is always a regression.
- **PR=160 orbit radius is optimal.** PR=130 too close (more damage taken), PR=180 too far (less DPS). Sweet spot is 160.
- **Do NOT change Pincer/Encircle to orbit-based positioning.** Strategy transition repositioning disruption causes -4.9pp regression. Only Wolfpack uses orbit.
- **Frequency constants (AllyPingInterval=15, VolleyIntervalTicks=30) must NOT be changed.** Regression risk from prior session data.

## Current Configuration

| Tank | Slot | MaxFP | PR | HasEcm | Retreat |
|------|------|-------|----|--------|---------|
| Hammer | 0 | 2.0 | 160 | false | 25 |
| Blade | 1 | 2.0 | 160 | false | 20 |
| Arrow | 2 | 1.5 | 160 | false | 20 |
| Ghost | 3 | 2.0 | 160 | false | 20 |

**SwarmCoordinator.ExecuteWolfpack:** orbit-based (`slot % aliveCount * 360/aliveCount`, radius = `config.PreferredRange`)

## Next Hypothesis (Iteration 15)

**Potential improvements to explore:**
1. Target selection: currently weakest-first (`OrderBy(c => c.Energy)`). Try targeting
   the enemy closest to the current leader tank to minimize travel time to firing range.
2. Volley fire timing: `fireAt = tick + 20`. With tanks at PR=160, bullet at MaxFP=2.0
   (speed=14) takes 160/14=11.4t. So fireAt=tick+11 would be more synchronized?
3. Ghost PR=150 (inner orbit point) vs current 160 — the distinct inner ring might help.

Success criteria: Red win rate increases by ≥2pp at seed 1000 (from 66% to ≥68%)

---

## Iteration Log

### Iter 0 — Serial Baseline Established
**Date:** 2026-04-20
**Status:** True serial baseline: 38.2% avg (39/34/41.5% seeds 1000/2000/3000)
- NOTE: All prior iters 0–13 used invalid parallel mode. Their findings are discarded.
- Ghost MaxFP=0.1, JamAndSpoof, proactive ECM — ECM was causing continuous drain death

### Iter 14 — Serial Rebase + Wolfpack Orbit (CONFIRMED +26pp total)
**Date:** 2026-04-20
**Status:** DOMINANT. Red 64.2% avg (5-seed serial).
- Phase 1: Ghost ECM removal + MaxFP=2.0: +7pp → 45.2% avg
- Phase 2: Formation tightening to PR=160 + Blade/Hammer MaxFP=2.0: +2.4pp → 47.6% avg
- Phase 3: Wolfpack orbit positioning: +16.6pp → 64.2% avg
- 400-match validation seed 1000: 62.2% (249/400)
- See Research/iter-0014-serial-rebase-and-wolfpack-orbit.md for full details
