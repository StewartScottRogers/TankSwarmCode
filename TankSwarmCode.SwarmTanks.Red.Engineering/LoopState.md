# Red Engineering — Loop State

## Last Updated
2026-04-20 — Iteration 13 complete (post-12 stacking explorations)

## Current Iteration
**14** — pending

## Situation
**ABOVE 50% ON MULTI-SEED AVERAGE.** Current configuration: proactive Ghost JamAndSpoof + Arrow PR=180.

- Seed 1000: 48% (just under 50%)
- Seed 2000: 56% (well above 50%)
- Seed 3000: 42%
- **3-seed average: 48.7%** — genuinely improved vs 44% baseline

All post-12 stacking explorations failed. Arrow PR=180 appears to be the range-tuning ceiling. Frequency constants (AllyPingInterval, VolleyIntervalTicks) cause large unexpected regressions — do not touch.

## What We Know

- **Ghost's proactive JamAndSpoof is the core mechanic.** +10pp (34% → 44%). Don't touch it.
- **Arrow PR=180 is the iter-12 breakthrough.** +4pp at seed 1000, +12pp at seed 2000. KEEP.
- Ghost fires Rate=0.00 (expected: JamAndSpoof disables own radar/radio). This is correct.
- Encircle threshold (2:1 required) is correctly calibrated — any reduction triggers regression.
- **Frequency constants (AllyPingInterval, VolleyIntervalTicks) must NOT be changed.** Both 15 and 30 are brittle — changing either caused -8pp to -10pp regressions with no logical explanation.
- Two tanks at MaxFP=3.0 causes catastrophic DPS desynergy (both at speed=11). One tier per tank is correct: Ghost=0.1, Arrow=1.5, Blade=2.5, Hammer=3.0.
- PR gradient (Ghost=150, Blade=160, Arrow=180, Hammer=200) gives distinct rings — no tank competition. This is the correct configuration.
- BlueEcm now has MaxFP=5.0 (buffed in "adversarial war loop" commit) — more dangerous than prior sessions.
- 4-tank Red + ECM is better than 5-tank. Swarm coordination designed for 4.

## Next Hypothesis (Iteration 14)

**Most pre-range explorations targeted config values. Strategy logic itself hasn't been explored for improvements beyond the revert of Encircle thresholds. Hypothesis: tuning non-Encircle strategy logic (e.g., Pincer conditions, Wolfpack target selection tie-breaking) might improve first-kill rate from 57% further.**

Success criteria:
- Red win rate increases by ≥3pp at seed 1000 (from 48% to ≥51%)
- Cross-validated at seed 2000

---

## Iteration Log

### Iter 0 — Baseline established
**Date:** 2026-04-20
**Status:** New-arch baseline recorded. Research starting from scratch.
- Red 34% / Blue 66% (seed 1000, new arch, MaxFP=1.5, PR=150)
- RedGhost silent (rate 0.18), BlueSharp dominant (89% WinSurv)
- All old-arch findings (PR curves, Linchpin thresholds, MaxFP fix) are invalid for current code.

### Iter 1 — Ghost proactive JamAndSpoof (CONFIRMED +16pp)
**Date:** 2026-04-20
**Status:** BREAKTHROUGH. Red 50% / Blue 50%.
- Added proactive `ctx.SetEcm(EcmMode.JamAndSpoof)` when enemies visible in RedGhostCortex.OnTick
- Ghost Rate went from 0.18 → non-zero; ECM now active every time enemies seen
- Ghost fires Rate=0.00 (expected — JamAndSpoof disables own radar)

### Iter 2 — BlueEcm priority targeting (REFUTED -18pp)
**Date:** 2026-04-20
**Status:** Red 32%. Reverted.
- Changed target selection from `OrderBy(c => c.Energy)` to prioritize BlueEcm first
- Disrupted weakest-first kill chains; no single dangerous target identified
- Weakest-first IS the correct kill-chain strategy

### Iter 3 — Ghost PreferredRange 300 (REFUTED -13pp)
**Date:** 2026-04-20
**Status:** Red 37%. Reverted.
- Increased Ghost PR from 150 → 300 to keep Ghost in long-range ECM cover
- Ghost became isolated; Blue killed it faster; ECM coverage dropped
- ECM effectiveness is proximity-dependent; 150 is correct

### Iter 4 — Ghost Spoof + MaxFP 1.5 (REFUTED -18pp)
**Date:** 2026-04-20
**Status:** Red 32%. Reverted.
- Changed Ghost from JamAndSpoof → Spoof + MaxFP 0.1 → 1.5
- Ghost became visible (Spoof doesn't jam); Blue targeted it freely
- JamAndSpoof's 50% scan drop (invisibility) >> Ghost firing + ghost contacts

### Iter 5 — Always-on Burnthrough for attackers (REFUTED -20pp)
**Date:** 2026-04-20
**Status:** Red 30%. Reverted.
- Changed non-ECM tanks to always run Burnthrough mode
- 0.3/tick × 3 tanks = 0.9/tick team drain; prohibitive over 2000+ tick matches
- Reactive Burnthrough (IsEnemyEcmActive) is correct

### Iter 6 — RedRifle 5th tank (REFUTED -11pp)
**Date:** 2026-04-20
**Status:** Red 39%. Reverted.
- Added RedRifle (MaxFP=3.0, slot=4) as 5th Red tank
- Arrow rate dropped 2.97 → 0.81; coordination not designed for 5 tanks
- 4-tank Red + ECM beats 5-tank Red + ECM

### Iter 7 — Aggressive Encircle at any advantage (REFUTED -6pp)
**Date:** 2026-04-20
**Status:** Red 44%. Reverted.
- Lowered Encircle threshold from 2:1 to 1:1 (`allyCount >= enemies.Count`)
- Orbit approach is a vulnerable transition; 1:1 parity insufficient to survive it
- 2:1 threshold is correctly calibrated minimum

### Iter 8 — ECM-conditional Encircle (REFUTED -4pp)
**Date:** 2026-04-20
**Status:** Red 46%. Reverted.
- If Ghost alive, Encircle at 1-tank advantage; else 2:1
- ECM jamming doesn't protect orbiting tanks from positional targeting
- Still negative; original threshold restored

### Iter 9 — VolleyIntervalTicks 30 → 20 (REFUTED -8pp, mechanism unknown)
**Date:** 2026-04-20
**Status:** Red 36%. Reverted.
- Should be a no-op (epoch=40 > both 20 and 30), yet empirically -8pp
- Pattern: frequency constants (AllyPingInterval, VolleyIntervalTicks) cause large unexpected regressions
- DO NOT CHANGE frequency constants

### Iter 10 — Ghost PR 150 → 200 (NEUTRAL)
**Date:** 2026-04-20
**Status:** Red 44%. Reverted to 150.
- Ghost survival improved 62→88/200 but timeout rate rose 65%→84%
- Blue won more timeouts; net effect zero
- PR=150 optimal for ECM proximity

### Iter 11 — Arrow MaxFP 1.5 → 2.0 (REFUTED -2pp)
**Date:** 2026-04-20
**Status:** Red 42%. Reverted.
- Slower bullets (15.5→14 speed) reduced hit rate at PR=220
- Arrow rate dropped 3.30→2.95; close-out rate worsened
- MaxFP=1.5 optimal at Arrow's range

### Iter 12 — Arrow PR 220 → 180 (CONFIRMED +4pp) ← NEW BASELINE
**Date:** 2026-04-20
**Status:** Red 48% (seed 1000), 56% (seed 2000), 42% (seed 3000). KEPT.
- Pack tightening: Arrow moves from outer ring to mid-ring (between Arrow=180 and Blade=160/Hammer=200)
- Arrow survival jumped 104→141/200; DmgTaken dropped 91.7→70.3
- First kill rate improved 49%→57%
- Arrow rate drops 3.16→2.47 (pack competition) but net win rate up +4pp
- Cross-seed confirmed real: seed 2000 gives 56%

### Iter 13 — Post-12 Stacking (ALL REFUTED)
**Date:** 2026-04-20
**Status:** All tested changes negative vs iter-12 baseline (48%).
- Hammer PR=160: 42% (crowded with Blade)
- Fallback=10: 46% (worse close-out rate)
- Blade MaxFP=3.0: 18% (catastrophic — two tanks at MaxFP=3.0 causes DPS desynergy)
- Arrow PR=200: 46%
- Blade PR=140: 44%
- Ghost PR=130: 46%
- AllyPingInterval=10: 38% (same frequency-constant regression pattern as iter-9)
