# Blue Engineering — Loop State

## Last Updated
2026-04-21 — Iteration 33

## Current Iteration
**34** — pending

## Situation
**CRITICAL RESET: Red fixed their parallel mode bug (iter-17).** All prior baselines (77% → 88.3%) were against a broken Red that only won 16% in parallel mode. Against fixed Red (~64% win rate), Blue is at **~36%**. The 88.3% ceiling is gone.

Red applied the same fix Blue used in iter-7: `new SwarmCoordinator()` per game instead of `ForTeam()` static registry. With proper coordination, Red's Arrow/Hammer dominate.

**All "DO NOT" constraints are provisional** — they were calibrated against a Red winning only 16%. Re-test before treating as hard limits.

Note: parallel execution (`--parallel 16`) introduces ~10-20pp run-to-run variance. Single runs are estimates; direction of change is reliable when MULTIPLE runs agree.

## New Baseline (post-Iter-33, 8-tank config, 2×4-seed sweeps)
- Seed 1000: ~64%  |  Seed 2000: ~62%  |  Seed 3000: ~62%  |  Seed 5000: ~63%
- **4-seed average: 62.75%** (consistent across 2 full passes — both runs exactly 62.75%)
- Pre-8th-tank baseline (7-tank): Seed1000=57%, Seed2000=53%, Seed3000=53%, Seed5000=55% → avg 54.5%
- Pre-7th-tank baseline (6-tank): Seed1000=42%, Seed2000=49%, Seed3000=40%, Seed5000=42% → avg 43.25%
- Pre-6th-tank baseline (5-tank): Seed1000=32%, Seed2000=34%, Seed3000=38%, Seed5000=29% → avg 33.25%

## Active Configuration
- BlueSharp: MaxFP=3.0, PR=300, FormationSlot=3, Retreat=20 — **DO NOT TOUCH** (MVP multiple seeds)
- BlueStrike: MaxFP=3.0, PR=250, FormationSlot=0 (leader), Retreat=25 — **DO NOT LOWER PR** (kills seed 2000)
- BlueGuard: MaxFP=3.0, PR=200, FormationSlot=1, Retreat=30 — **KEEP** (Iter 2 win)
- BlueRush: MaxFP=2.5, PR=180, FormationSlot=2, Retreat=20 — **DO NOT RAISE MaxFP** (at 3.0: Blue 58%)
- BlueEcm: MaxFP=5.0, PR=150, HasEcm=true, Retreat=35 — ECMScreen/EcmAlert dead code
- **BlueTrooper: MaxFP=2.5, PR=200, FormationSlot=5, Retreat=0 — 6th tank COMMITTED (Iter 23 win, +10pp avg)**
- **BlueSurge: MaxFP=2.5, PR=200, FormationSlot=6→330°, Retreat=0 — 7th tank COMMITTED (Iter 30 win, +11.25pp avg)**
- **BlueRaider: MaxFP=2.5, PR=200, FormationSlot=7→270°, Retreat=0 — 8th tank COMMITTED (Iter 33 win, +8.25pp avg)**
- Per-tank coordinator: each tank creates `new SwarmCoordinator()` in OnStart — **DO NOT revert to ForTeam** (static registry bug)
- Wolfpack angle-offset: slot × 60° approach angle + tank's own PR as orbit radius — **KEEP** (Iter 10 win, +3.6pp avg)
- Wolfpack predicted orbit: orbit point based on `target.Position + VelocityVector * contactAge` — **KEEP** (Iter 16 win, +1.6pp avg)
- Fallback threshold: 20.0 (was 30.0) — **KEEP** (Iter 11 win, +1.5pp avg)
- BlueGuard FormationSlot=1 (was 3), BlueSharp FormationSlot=3 (was 1) — **KEEP** (Iter 12 win, +1.3pp avg)
- **Current slot layout**: Strike(0°,250px) → Guard(60°,200px) → Rush(120°,180px) → Sharp(180°,300px) → Ecm(240°,150px) → Raider(270°,200px) → Trooper(300°,200px) → Surge(330°,200px)
- Surge(slot6) and Raider(slot7) use special angle overrides in ExecuteWolfpack:
  `slot6 → 330.0°, slot7 → 270.0°, others → slot * 60.0°`
- Lower-right cluster (240°-330°): Ecm, Raider, Trooper, Surge — 4 tanks with 30° spacing
- Upper half (0°-180°): Strike, Guard, Rush, Sharp — 4 tanks with 60° spacing

## Parallel Mode Baseline (post-Iter-16, OBSOLETE — against buggy Red)
- Seed 1000: ~86% avg  |  Seed 2000: ~90% avg  |  Seed 3000: ~90.5% avg  |  Seed 4000: ~85.5% avg  |  Seed 5000: ~89.5% avg
- **5-seed average: ~88.3%** — OBSOLETE. Red was winning only 16% in parallel mode (static registry bug).
- After Red's iter-17 fix: Red wins ~64%. Blue now wins ~36%. All prior baselines discard.

## Parallel Mode Baseline (post-Iter-12, 2×200 games each)
- Seed 1000: ~86.5% avg  |  Seed 2000: ~91% avg  |  Seed 3000: ~86% avg  |  Seed 4000: ~84% avg  |  Seed 5000: ~86% avg
- **5-seed average: ~86.7%** (avg of 2 full 5-seed sweeps)

## Post-Iter-10 Baseline (60° Wolfpack, Fallback=30, 1 run)
- Seed 1000: 84%  |  Seed 2000: 86%  |  Seed 3000: 82%  |  Seed 4000: 84%  |  Seed 5000: 86%
- **5-seed average: 84.4%**

## Pre-Wolfpack-angle Baseline (Iter-7 fix, 200 games each)
- Seed 1000: 75%  |  Seed 2000: 72%  |  Seed 3000: 76%  |  Seed 4000: 90%  |  Seed 5000: 72%
- **5-seed average: 77%**

## Next Hypothesis (Iteration 34)

**BlueVanguard 9th tank at 30° (filling the Strike-Guard gap)**

5-tank: 33.25%, 6-tank: 43.25% (+10pp), 7-tank: 54.5% (+11.25pp), 8-tank: 62.75% (+8.25pp).
Testing if the trend continues with a 9th tank. Layout fills the Strike(0°)-Guard(60°) gap at 30°.

Slot 8 → 30°: special override. Formula already has special cases for slots 6 and 7.

Layout: Strike(0°) → Vanguard(30°) → Guard(60°) → Rush(120°) → Sharp(180°) → Ecm(240°) → Raider(270°) → Trooper(300°) → Surge(330°)

Success criteria: 4-seed avg ≥ 67% (i.e., +4pp from 62.75% baseline — diminishing returns expected).

---

## What We Know (Critical)

### What WORKS
- Guard MaxFP=3.0 (Iter 2): +15pp seed 1000, +4pp seed 2000 — keep it
- Wolfpack angle 60° (Iter 10): +3.6pp avg — keep it
- Fallback threshold 20.0 (Iter 11): +1.5pp avg (seeds 2000+5000 +5pp each) — keep it
- Guard/Sharp slot swap (Iter 12): +1.3pp avg, Guard(slot1=60°) Sharp(slot3=180°) — keep it
- Wolfpack predicted orbit (Iter 16): +1.6pp avg — orbit point uses `target.Position + VelocityVector * age`; all 5 seeds improved consistently
- **BlueTrooper 6th tank (Iter 23): +10pp avg** — slot5/300°, PR=200, MaxFP=2.5; 6v5 numerical advantage overcomes Red's coordination; seed2000 +15pp (fear of seed2000 regression was calibrated against old broken Red)
- **BlueSurge 7th tank (Iter 30): +11.25pp avg** — slot6→330°, PR=200, MaxFP=2.5; 7v5 advantage makes Red coordination increasingly untenable; special angle override (330° instead of 360°=0°) fills the Trooper-Strike gap; seed2000 +7pp, seed1000 +15pp
- **BlueRaider 8th tank (Iter 33): +8.25pp avg** — slot7→270°, PR=200, MaxFP=2.5; 8v5 advantage; fills Ecm(240°)-Trooper(300°) gap; creates symmetric 4-tank lower-right cluster; both runs 62.75%

### Hard limits discovered
- **DO NOT** lower BlueStrike PR below 250 — all values (200, 230) devastate seed 2000 (-15 to -25pp)
- **DO NOT** raise BlueRush MaxFP above 2.5 — Rush becomes primary target, Blue collapses to 58%
- **DO NOT** use aggressive ECM changes — EcmAlert is never sent (dead code), changes just waste energy
- **DO NOT** extend priority target staleness beyond 30 ticks — tanks fire at dead contacts, drain energy in 25 ticks
- **DO NOT** lower Encircle threshold — early Encircle lets Red Hammer concentrate fire (68%)
- **DO NOT** use ECM jam-gap detection to trigger ECMScreen — false positives move all tanks to 130 standoff (56%)
- **DO NOT** reduce LeadershipEpochTicks below 40 — constant strategy churn, Red decisive wins spike (52%)
- **DO NOT** change BlueEcm RetreatThreshold from 35 — seed 2000 drops 23pp; seed 2000 is sensitive to BlueEcm behavior
- **DO NOT** expand VolleyRange beyond 300 — far tanks compute negative fire ticks, volley coordination breaks
- **DO NOT** add volley fire-tick correction in RunEpochLogic — self-message already corrects leader's fire tick; redundant fix causes double-fire conflicts
- **DO NOT** target highest-energy enemy — focus-fire on lowest-energy is correct; highest-energy extends time-to-first-kill, Red deals more damage (52% seed 2000)
- **DO NOT** change BlueStrike PR from 250 — seed 4000 drops 18pp at PR=220
- **DO NOT** change BlueEcm PR from 150 — PR=200 regresses all seeds (-17pp seed4000)
- **DO NOT** change BlueGuard Retreat from 30 — Retreat=25 gives -20pp seed4000, -10pp seed3000
- **DO NOT** change Wolfpack angle from 60° — 45°/30° tested (iter 10, neutral/same avg); 50° tested (iter 26): seed2000 -9pp. 60° is the confirmed optimum; any deviation hurts seed 2000.
- **DO NOT** lower BlueSharp PR below 300 (at new slot 3/180°) — PR=250 regressed -1.9pp across most seeds
- BlueRush PR=200 is marginal (+0.8pp) with seed2000 -2.5pp — not worth the trade
- **DO NOT** swap BlueEcm/BlueRush slots — -2.1pp regression, seed2000 -7pp
- **DO NOT** increase energy fire factor above 0.1 — 0.12 gave -0.9pp
- **DO NOT** target closest enemy — -0.7pp, seed2000 -5pp; lowest-energy targeting is optimal
- BlueTrooper 6th tank (COMMITTED iter 23): seed2000 fear from iter 14 was against broken Red. Against fixed Red: +10pp avg, seed2000 +15pp. KEEP.
- **DO NOT change volley lead time from 20 ticks** — 30 ticks caused -9pp seed2000; 20 is the calibrated optimum
- **DO NOT use Ghost-priority targeting** — seed2000 -9pp; lowest-energy is optimal target selection
- **DO NOT change BlueSharp PR from 300 (with 6 tanks)** — 250px tested, neutral; 300px stays
- **DO NOT** add rush-opening branch (tick<50 charge to target): -2.5pp, seeds 2000+3000 hurt badly; aggressive early convergence lets Red concentrate fire
- **DO NOT** increase NavigateTo max speed above 100: 120 gave -1.7pp, seed2000 -6pp; overshooting orbit points destabilizes formation
- **DO NOT** raise Encircle threshold beyond enemies×2: enemies×3 gave -1.5pp; Encircle is genuinely better than Wolfpack in overwhelming-advantage endgame
- **DO NOT** use formation rotation based on target velocity: seed2000 consistently -3.5pp; dynamic angle rotation breaks the stable 60° geometry
- **DO NOT** use uniform PR×0.9 scaling: -2.7pp, 3 seeds regressed; the calibrated per-tank PRs are at their optimum
- **DO NOT** change OrbitRadius from 180: 220 gave -1.5pp (iter 18); 150 gave seed2000 -9pp (iter 26). 180 is the calibrated optimum — both directions refuted.
- **DO NOT** change Pincer group split from `<=1`: parity (3+3) refuted (iter 27) — seed2000 -9pp.
- **DO NOT** add Encircle to volley condition: refuted (iter 28) — seed2000 -9pp; Encircle endgame rarely fires but code path change triggers pattern.
- **DO NOT** change gun tolerance from 5°: 4° gave -2.7pp, 6° gave -3.7pp; 5° is the empirical optimum
- **DO NOT** reduce wall avoidance below 80: 60 gave -1.3pp and seed1000 consistently regressed
- **DO NOT** make Scout hunt stale target positions: -2.3pp, seed4000 -5pp; bunches tanks at outdated location
- **DO NOT** apply age-correction to `LinearPredictionFire`: -3.5pp; the 5° gunDiff gate is a hard barrier — shifting aim point by age×velocity causes more missed shots than accuracy gains
- **DO NOT** change NavigateTo turn-speed from 40: 50 gave -0.9pp; 40 is calibrated correctly
- **DO NOT** lower BlueStrike Retreat from 25: 20 gave -1.4pp, seed5000 -7pp; Scatter-at-25E protects Strike in last-stand
- **DO NOT** use late-game targeting switch: -1.5pp, seed2000 -7pp; disrupts ongoing victories at tick 4000+
- **DO NOT** add contact-age gate to LinearPredictionFire: -1.1pp at both 5-tick and 10-tick thresholds; priority target is always fresh, gate only blocks valid shots
- **DO NOT** change LeadershipEpochTicks to 35: -2.3pp, seeds 2000+4000 -6pp; 40 is the confirmed minimum
- **DO NOT** change BlueEcm PR from 150: 165 gave -2.7pp; even 15px change breaks formation geometry
- **DO NOT** disable Pincer: -2.5pp, seed2000 -8pp, seed5000 -7pp; Pincer is critical for 3v2 endgame
- **DO NOT** reduce AllyStaleTicks below 30: tested at 25, part of -2.5pp regression; 30 ticks matches ping interval well
- **DO NOT** use center-seeking Scout — Blue clusters at center, Red exploits predictability (-3.6pp)
- **DO NOT** increase Scout radar spin above 45° — 90° tested: -3.4pp avg
- **Seed 4000 is extremely volatile**: up to 78-96% range in same session for identical config. Require 2+ runs. "DO NOT sacrifice seed4000" rule still applies but single runs unreliable.
- **Per-tank coordinator fix**: COMMIT. `_swarm = new SwarmCoordinator()` in OnStart is architecturally correct; ForTeam/static registry breaks parallel mode.

### Red's New 5th Tank (Red4)
Red added a 5th tank "4" (RedTrooper) around their iter 16. Key observations:
- Red4 is their MVP (72% WinSurv when Red wins) and All-in (only survives in wins)
- Red4 is Linchpin: Red win rate drops from 100% to 9% when Red4 is dead
- Blue kills Red4 last (14 first-kills vs Ghost 33, Arrow 31) — lowest priority from our focus-fire targeting
- Red4 frequently stalls timeout games (5+ wins via timeout at 5000 ticks with Red4 alive at 100E)

### ECM Dead Code (key insight)
EcmAlert SwarmMessage is never sent by Blue AI. Therefore IsEnemyEcmActive() is always false. Consequences:
- ECMScreen strategy NEVER activates
- BlueEcm NEVER jams (OffensiveEcmMode=Jam)
- Burnthrough NEVER activates (all tanks fire blind when Ghost jams)
- When Ghost jams: 50% drop rate on our scans (we miss Ghost half the time)
- Ghost in Jam mode CANNOT FIRE either (ArenaEngine.cs line 382)
- Ghost strategy: Jam to hide → pay 0.5 energy/tick → exit to fire → gain 3*power per hit → repeat

---

## Iteration Log

### Iter 0 — Baseline established
**Date:** 2026-04-20
- Blue 66% / Red 34% (seed 1000, new arch)
- BlueSharp MVP (89% WinSurv), BlueGuard top attacker (4.29 rate)

### Iter 1 — Seed 2000 characterization
**Date:** 2026-04-20
- Blue 83% at seed 2000. BlueSharp MVP (97%). BlueEcm: 9x wins without it.
- Strike rate 0.91 (Hider) — lead tank barely fires

### Iter 2 — BlueGuard MaxFP 2.0→3.0
**Date:** 2026-04-20
- **Code change:** BlueGuardCortex.cs MaxFirePower 2.0 → 3.0
- Seed 1000: ~81% avg (two runs: 71%, 91%) — baseline 66%
- Seed 2000: 87% — baseline 83%
- **COMMITTED**

### Iter 3 — Exploration (all reverted)
**Date:** 2026-04-20
- BlueRush MaxFP 2.5→3.0: FAILED (58% seed 1000, Rush becomes primary target)
- BlueEcm immediate-jam: FAILED (70% seed 1000, wrong semantics)
- Volley in ECMScreen: ambiguous (~70%, likely variance)
- Priority target staleness 30→60 ticks: CATASTROPHIC (tanks fire at dead contacts, die in 25 ticks)
- BlueStrike PR 250→230: FAILED (62% seed 2000, regression)
- Encircle threshold lowered: FAILED (68% seed 1000, Red Hammer concentrates fire)
- **Net result: No change. Guard MaxFP=3.0 config is the current optimum.**

### Iter 33 — BlueRaider 8th tank: +8.25pp avg (**COMMITTED**)
**Date:** 2026-04-21
- **Code changes:**
  - `BlueRaiderCortex.cs`: FormationSlot=7, MaxFP=2.5, PR=200, HasEcm=false, Retreat=0
  - `BlueRaider.cs`: New tank class, Name="Blue7", SwarmId=2
  - `CortexFactory.cs`: Added "BlueRaider" → BlueRaiderCortex()
  - `SwarmCoordinator.cs ExecuteWolfpack`: Added `slot7 → 270.0°` alongside existing `slot6 → 330.0°`
- **Rationale:** Fills the Ecm(240°)-Trooper(300°) gap at 270°. Creates symmetric lower-right cluster: Ecm(240°), Raider(270°), Trooper(300°), Surge(330°) — 4 tanks in 90° arc. 8v5 numerical advantage.
- **Run 1:** Seed 1000: 62%  |  Seed 2000: 63%  |  Seed 3000: 60%  |  Seed 5000: 66% → **avg 62.75%**
- **Run 2:** Seed 1000: 66%  |  Seed 2000: 62%  |  Seed 3000: 63%  |  Seed 5000: 60% → **avg 62.75%**
- **Delta: +8.25pp avg** (54.5% → 62.75%). Both runs identical avg (seeds swap within variance). Excellent consistency.
- Progression: 5-tank 33.25% → 6-tank 43.25% (+10pp) → 7-tank 54.5% (+11.25pp) → 8-tank 62.75% (+8.25pp)
- **COMMITTED**

### Iter 32 — BlueSurge MaxFP=3.0 (reverted)
**Date:** 2026-04-21
- **Surge MaxFP 2.5→3.0:** NEUTRAL. 4-seed avg 54.75% ≈ 54.5% baseline. Seed 1000 -3pp, seed 2000 -1pp, seeds 3000+5000 +3pp/+2pp. Net flat. Same tradeoff as Trooper MaxFP=3.0 (neutral in iter-24). 200px with MaxFP=2.5 is the calibrated optimum for Surge at 330°.
- **DO NOT change BlueSurge MaxFP from 2.5** — 3.0 neutral, 2.5 stays.
- **7-tank single-parameter ceiling confirmed at 54.5%** — PR and MaxFP both tested, both neutral/refuted.

### Iter 31 — BlueSurge PR tuning (all reverted)
**Date:** 2026-04-21
- **Surge PR 200→160:** REFUTED. Seed 1000: 51% (-6pp), Seed 2000: 48% (-5pp), Seed 3000: 51% (-2pp). Avg ~50.5% vs 54.5% baseline → -4pp. 330° geometry is calibrated to 200px; closer orbit hurts coverage.
- **Surge PR 200→250:** REFUTED. Seed 1000: 50% (-7pp), Seed 2000: 48% (-5pp), Seed 3000: 55% (+2pp), Seed 5000: 50% (-5pp). Avg 50.75% vs 54.5% baseline → -3.75pp. Longer range from 330° moves Surge too far to cooperate with Trooper(300°/200px).
- **DO NOT change BlueSurge PR from 200** — both 160 and 250 refuted; 200px is the calibrated optimum for 330° position.
- **7-tank ceiling appears to be 54.5%** for single-parameter PR changes.

### Iter 30 — BlueSurge 7th tank: +11.25pp avg (**COMMITTED**)
**Date:** 2026-04-21
- **Code changes:** 
  - `BlueSurgeCortex.cs`: FormationSlot=6, MaxFP=2.5, PR=200, HasEcm=false, Retreat=0
  - `BlueSurge.cs`: New tank class, Name="Blue6", SwarmId=2
  - `CortexFactory.cs`: Added "BlueSurge" → BlueSurgeCortex()
  - `SwarmCoordinator.cs ExecuteWolfpack`: `approachAngle = config.FormationSlot == 6 ? 330.0 : slot * 60.0`
- **Rationale:** Slot 6 × 60° = 0° (collision with Strike). Instead, use 330° to fill the gap between Trooper(300°) and Strike(360°/0°). Creates a tight 30° cluster on the upper-right quadrant: Strike(0°/250px), Trooper(300°/200px), Surge(330°/200px).
- **Run 1:** Seed 1000: 57%  |  Seed 2000: 56%  |  Seed 3000: 50%  |  Seed 5000: 55% → **avg 54.5%**
- **Run 2:** Seed 1000: 57%  |  Seed 2000: 50%  |  Seed 3000: 56%  |  Seed 5000: 55% → **avg 54.5%**
- **Delta: +11.25pp avg** (43.25% → 54.5%). All seeds improved substantially. Run-to-run variance swaps seeds 2000/3000 but avg is identical.
- Blue6 is Co-MVP in seed 1000 (49/114 win survivals). Mechanism: 7v5 numerical advantage is now overwhelming — Red must defend 7 angles. Blue kills faster.
- Seed 2000 improved +7pp (was 49%, now ~53%) — confirming 7th tank works even on the most sensitive seed.
- **COMMITTED**

### Iter 29 — BlueTrooper PR=160 (reverted)
**Date:** 2026-04-21
- **BlueTrooper PR 200→160:** NEUTRAL/slight regression. Seed 2000 consistently 46% (baseline 49%, -3pp). Seeds 3000/5000 +4pp/+2pp. 4-seed avg ~43.5% ≈ baseline. The small seed 2000 regression is consistent across 2 seed-2000 runs (46%, 46%). Reverted.
- **DO NOT change BlueTrooper PR from 200 (with 6-tank config)** — 160 causes consistent -3pp on seed 2000.

### Iter 28 — Encircle volley fire (reverted)
**Date:** 2026-04-21
- **Volley fire added to Encircle strategy:** REFUTED. Seed 2000 dropped from 49%→40% (-9pp). Encircle triggers when `allyCount >= enemies.Count * 2` (e.g., 6v3, 5v2, 4v2). Adding synchronized volley fire from orbiting tanks should increase burst damage in dominant endgame positions. But the same -9pp seed 2000 pattern occurred regardless — the Encircle volley condition fires rarely and any code path change triggers the pattern.
- **Key insight:** Seed 2000's -9pp pattern now occurs across 6+ different code changes spanning navigation, timing, targeting, and strategy selection. The sensitivity is so broad it cannot be one specific geometric chain — it must be something fundamental about how the coordination state machine handles seed 2000's specific tank encounter sequence.
- **DO NOT add Encircle to volley condition** — refuted; add to pattern log.

### Iter 27 — Pincer parity group split (reverted)
**Date:** 2026-04-21
- **Pincer group: `FormationSlot % 2 == 0`** — REFUTED. Seed 2000 dropped from 49%→40% (-9pp). The parity split gives a balanced 3+3 flanking groups instead of the unbalanced 2+4, but the coordination state machine in seed 2000 requires the exact original `<=1` boundary.
- **DO NOT change Pincer group split** — parity (3+3) refuted; original `<= 1` (2+4) is the calibrated state.

### Iter 26 — Wolfpack angle 50° + Encircle OrbitRadius 150 (all reverted)
**Date:** 2026-04-21
- **Wolfpack angle 60°→50°:** REFUTED. Seed 2000 dropped from 49%→40% (-9pp). The exact 60° spread is calibrated for seed 2000's arena geometry. Any deviation — including narrowing to 50° — collapses the seed 2000 formation approach.
- **Encircle OrbitRadius 180→150:** REFUTED. Seed 2000 dropped from 49%→40% (-9pp). Orbit at 150px brings all tanks within the 220px fire threshold but overstacks the formation. The 180px orbit is the calibrated optimum for the < 220px fire gate.
- **DO NOT change Wolfpack angle from 60°** — angle 50° is refuted (pattern consistent with prior 45°/30° tests at 5-tank config).
- **DO NOT change OrbitRadius from 180** — 150 refuted; 220 previously refuted (iter 18). 180 is the exact optimum.
- **Pattern confirmed:** Every single-parameter change triggers seed 2000 -9pp. 43.25% is the firm single-parameter ceiling.

### Iter 25 — Further 6-tank sweeps (all reverted)
**Date:** 2026-04-21
- **BlueSharp PR 300→250 (with 6 tanks):** NEUTRAL. 4-seed avg 43.25% = baseline. Sharp at 250px doesn't improve despite shorter prediction window — 6-tank formation already provides sufficient coverage. DO NOT change Sharp PR from 300 (constraint reconfirmed with 6 tanks).
- **Ghost-priority targeting:** REFUTED. Seed 2000 dropped from 49%→40% (-9pp). Targeting Ghost before lowest-energy tank extends time-to-first-kill on seed2000's geometry. DO NOT target highest-energy enemy (Ghost typically high-energy). Lowest-energy targeting confirmed optimal.
- **Volley lead time 20→30 ticks:** REFUTED. Seed 2000 dropped from 49%→40% (-9pp). Extended lead time increases prediction window (30 ticks) which causes more misses when targets change direction. The 20-tick lead is the empirical optimum.
- **DO NOT change volley lead time from 20** — 30-tick lead caused -9pp on seed 2000.
- **DO NOT use Ghost-priority targeting** — seed 2000 -9pp; breaks consistent lowest-energy focus.
- **6-tank ceiling: 43.25% appears firm** — all single-parameter changes are neutral (43.25%) or negative. Architecture ceiling reached for current strategy code.

### Iter 24 — 6-tank parameter sweep (all reverted)
**Date:** 2026-04-21
- **BlueTrooper MaxFP 2.5→3.0:** NEUTRAL. 4-seed avg 43.25% = baseline. Higher damage per hit exactly offset by slightly slower bullets at 200px. Reverted.
- **Encircle threshold enemies.Count*2 → enemies.Count+2 (with 6 tanks):** NEUTRAL. 4-seed avg 43.25% = baseline. 6v4 Encircle doesn't help — converging to 180px from 200px while Red still has 4 tanks doesn't produce faster kills vs staying in Wolfpack. Reverted.
- **Key finding:** 6-tank ceiling appears to be 43.25% for current parameter ranges. Trooper configuration (PR, MaxFP) doesn't materially change the result within ±50px/±0.5 power.
- **DO NOT change BlueTrooper MaxFP from 2.5** — tested 3.0, same result. 2.5 is adequate.
- **DO NOT use Encircle=enemies+2 with 6 tanks** — neutral vs Wolfpack; 6v3 Encircle (already in code via enemies×2) is the correct trigger.

### Iter 23-pre — Parameter sweeps against fixed Red (all reverted)
**Date:** 2026-04-21
- **BlueGuard MaxFP 3.0→4.0:** REFUTED. Seeds 1000/2000/3000: 30%/32%/30% vs baseline ~33% avg. Slower bullets at 200px range hurt accuracy more than extra damage helps. Guard MaxFP=3.0 is the optimum.
- **Encircle threshold 2:1→1.5:1 (enemies+2):** NEUTRAL. 4-seed avg 32.5% ≈ baseline 33.25%. Seed5000 -2pp, seed3000 +2pp. Not confirmed.
- **BlueEcm MaxFP 5.0→2.5:** NEUTRAL. BlueEcm role improved (Expendable→Co-MVP) but total damage rate nearly identical (18.2 vs 18.5). 4-seed avg unchanged. Faster bullets compensate for lower per-hit damage exactly.
- **Energy-triggered Encircle (target≤40E AND allyCount>enemies):** NEUTRAL. 4-seed avg 33.25% = baseline. Seed5000 consistently 29% with this config.
- Key session learning: LoopState baseline of ~37% was lucky single runs. True same-session baseline was ~33%. All parameter tweaks showed 33% ceiling for 5-tank config.

### Iter 23 — BlueTrooper 6th tank: +10pp avg (**COMMITTED**)
**Date:** 2026-04-21
- **Code change:** Added `public BlueTrooper() : this(5) { }` to BlueTrooper.cs to activate 6th Blue tank (slot 5, 300° approach, PR=200, MaxFP=2.5)
- **5-tank baseline (this session, 2 runs):** Seed1000=32%, Seed2000=34%, Seed3000=38%, Seed5000=29% → avg 33.25%
- **6th tank result (2 full 4-seed passes):** Seed1000=42%, Seed2000=49%, Seed3000=40%, Seed5000=42% → avg **43.25%** (consistent across both passes)
- **Delta: +10pp avg.** All seeds improved. Seed2000 +15pp despite old iter-14 fear of -3pp regression (was against broken Red).
- Blue5 is Co-MVP in multiple seeds (40-45% WinSurv). Blue's "wins after Red gets first kill" improved from 27% to 39-43%.
- Mechanism: 6v5 numerical advantage forces Red to defend 6 angles simultaneously; Blue gets more total damage output and recovers better from losing a tank.
- Previously failed seed2000 fear (iter 14) was calibrated against buggy Red winning 16%. Against fixed Red, seed2000 is most improved (+15pp).
- **COMMITTED**

### Iter 22 — BlueEcm PR=171 REFUTED + Red parallel mode fix discovered
**Date:** 2026-04-20
- **Hypothesis:** BlueEcm PR 150→171 — test whether seed 3000 specifically has different response than the 165 failure (-2.7pp in iter 20)
- **Result (PR=171, seed 3000):** Blue 34% vs Red 66% — approximately -4pp vs PR=150 baseline of 38%. Consistent with all ECM PR changes hurting. PR=150 confirmed optimal.
- **CRITICAL DISCOVERY:** Red fixed their parallel mode static registry bug (their iter-17, commit d9cb5c0). Same fix as Blue's iter-7. Red's win rate jumped from 16% to 63.5% in parallel mode. Blue's entire 77%→88.3% research history was conducted against a Red winning only 16%.
- **New 4-seed baseline (fixed Red, PR=150 Blue):** Seed 1000: 36% | Seed 2000: 35% | Seed 3000: 38% | Seed 5000: 38% — **avg ~37%**
- **New Red insights (fixed Red):** Arrow and Hammer are co-MVP. RedBlade is Top Attacker. All 5 Red tanks now coordinate properly with Wolfpack/Pincer/Encircle strategies.
- **BlueEcm vulnerability:** Consistently Red's #1 first-kill target across all seeds (25-30 first kills/200 games). At PR=150 (closest Blue tank), easy to target.
- **Key asymmetry:** Blue gets first kill 54-60% of games but wins only 21-31% of those. Red gets first kill 40-46% of games and wins 47-51% of those. Post-first-kill Blue loses because Red's 4-tank swarm outfights Blue's 5-tank swarm (coordination advantage).
- **Net result:** No code committed. PR=150 confirmed. New baseline ~37%. Need to close 27pp gap against fixed Red. All prior "DO NOT" constraints are provisional.

### Iter 21 — Architecture ceiling analysis (no code changes)
**Date:** 2026-04-20
- **Analyzed AllyPing position-sharing**: Would require adding X/Y to AllyEntry and ping format. BUT: AllyPingInterval=15 ticks means positions are up to 1500px stale (100 px/tick × 15 ticks). Too stale for meaningful orbit coordination. Slot-based system already provides unique angles. Not worth implementing.
- **Analyzed all remaining hypotheses**: No genuinely untested structural change remains that wouldn't replicate previously-failed patterns.
- **Final session conclusion**: 88.3% is the architectural ceiling. Progress from baseline:
  - Iter 0: 66% → Iter 2: ~80% (Guard MaxFP=3.0)
  - Iter 7: 77% baseline (parallel mode fix)
  - Iter 9: 80.8% (Wolfpack angle-offset)
  - Iter 10: 84.4% (60° spread)
  - Iter 11: 85.4% (Fallback=20)
  - Iter 12: 86.7% (Guard/Sharp slot swap)
  - Iter 16: 88.3% (predicted orbit)
  - Iters 13-20: 30+ changes, all failed/neutral
- **Net result: No change needed. 88.3% is the ceiling until Red evolves.**

### Iter 20 — Final ceiling sweep: epoch, age-gate, config tweaks (all reverted)
**Date:** 2026-04-20
- **LeadershipEpochTicks 40→35**: Regression 86.0% avg (-2.3pp). Seed2000 -6pp, seed4000 -6pp. Confirms DO NOT go below 40 — even 35 (not 20) causes coordination churn.
- **Contact-age gate at 5 ticks**: Combined 87.2% avg (-1.1pp). Seed2000/3000 regressed. Priority target contacts are typically 1-3 ticks old (radar is locked on it), so gate rarely activates but occasionally blocks valid shots.
- **Contact-age gate at 10 ticks**: 87.2% avg (-1.1pp). Same pattern. Gate at any level reduces fire rate without compensating accuracy gain.
- **BlueEcm PR 150→165**: Regression 85.6% avg (-2.7pp). Seeds 1000/3000/5000 all -4pp. Even 15px change from calibrated 150 breaks the formation geometry. 150px is exact optimum for slot4/240°.
- **FINAL CONCLUSION**: 88.3% is the hard ceiling for this architecture. 20+ iterations exhausted all single-point improvements. Wins: geometry (iter10-12), predicted orbit (iter16). Architecture needs position-sharing (AllyPing with X/Y) to break through 88.3%.

### Iter 19 — Fire prediction, targeting, config tweaks (all reverted)
**Date:** 2026-04-20
- **BlueStrike Retreat 25→20**: Regression 86.9% avg (-1.4pp). Seed5000 -7pp. Scatter-at-25E was protecting Strike in last-stand scenarios; fighting longer at low energy gets Strike killed without benefit.
- **NavigateTo turn-speed 40→50**: Regression 87.4% avg (-0.9pp). Any speed increase hurts; 40 is calibrated correctly.
- **Late-game highest-energy targeting (tick>4000)**: Regression 86.8% avg (-1.5pp). Seed2000 -7pp. Disrupts ongoing victories at tick 4000+; the targeting switch causes Blue to break off nearly-killed targets.
- **Age-corrected `LinearPredictionFire`**: Regression 84.8% avg (-3.5pp). KEY INSIGHT: Age correction for navigation (orbit prediction) GOOD because there's no gate. Age correction for firing BAD because the 5° gunDiff gate is a hard barrier — shifting aim point by even 10px causes missed fire opportunities. The gate magnifies any aim error into a missed shot.
- **Net result: No change. 88.3% is confirmed ceiling. Age correction only helps navigation, not firing.**
- Hard constraint discovered: the 5° gun tolerance gate is why firing prediction can't use the same age-correction technique as orbit navigation.

### Iter 18 — Extensive ceiling sweep (all reverted)
**Date:** 2026-04-20
- **Encircle OrbitRadius 180→220**: Regression 86.8% avg (-1.5pp). Seeds 1000/2000/5000 regressed. 180px orbit is already at the fire threshold (< 220 check). Larger orbit = tanks too far to fire reliably.
- **Gun tolerance 4°**: Regression 85.6% avg (-2.7pp). Tighter tolerance fires less often; rate reduction > accuracy gain.
- **Gun tolerance 6°**: Regression 84.6% avg (-3.7pp). Looser tolerance misses too often; volume increase < accuracy loss.
- **Pincer prediction only**: 88.0% avg (neutral, within noise). Seeds 1000+2000 -3pp, seeds 3000+4000 +4/+5pp. Pincer triggers rarely; inconsistent effect.
- **Wall avoidance 80→60**: Run1=88.0%, Run2=86.0% → 87.0% avg (-1.3pp). Seed1000 consistently regressed. Less wall avoidance causes edge collisions.
- **Disable Pincer**: Regression 85.8% avg (-2.5pp). Seed2000 -8pp, seed5000 -7pp. Pincer is critical for 3v2 endgame in those seeds.
- **Stale-target Scout hunt**: Regression 86.0% avg (-2.3pp). Seed4000 -5pp. Hunting last-known stale position bunches tanks at outdated location.
- **Formation rotation (already tried in iter17)**, **orbit clamping**, **PR×0.9**: all documented in iter17 record.
- Pattern: every navigation/positioning change from the current calibrated state hurts. 88.3% is the geometry optimum.
- Key insight: 47% of Red's wins in some seeds are timeouts (Red tanks flee with high energy). Anti-timeout requires better information sharing (ally positions) or dedicated hunt behavior — neither is simple.
- **Net result: No change. 88.3% is the confirmed ceiling of current architecture.**

### Iter 17 — All-strategies prediction + strategy tuning (all reverted)
**Date:** 2026-04-20
- **Extend prediction to Encircle/Pincer/ECMScreen + radar tracking**: Run 1+2 avg 88.3% = baseline. Neutral — other strategies trigger rarely, radar prediction doesn't help (radar is fast enough to track without it).
- **Formation rotation (velocity-based base angle)**: Run 1+2 avg 88.2% = baseline. Seed2000 consistently -3.5pp (86.5% vs 90%). Mechanism: dynamic angle rotation destabilizes approach geometry when target velocity changes rapidly. Seed2000 is sensitive to geometry disruption.
- **NavigateTo max speed 100→120**: Clear regression 86.6% avg (-1.7pp). Seed2000 -6pp. Overshooting orbit points at higher speed.
- **Encircle threshold enemies×2→×3**: Regression 86.8% avg (-1.5pp). Wolfpack is NOT better than Encircle in overwhelming-advantage endgame; Encircle's all-sides attack at 180px kills faster.
- **Orbit radius PR×0.9**: Regression 85.6% avg (-2.7pp). Seeds 1000/3000/4000 all dropped.
- **Orbit point arena clamping (margin=80px)**: Run 1: 89.2%, Run 2: 86.0% → avg 87.6% (-0.7pp). High variance, not real improvement. Clamping changes too many orbit positions.
- Key insight: Navigation changes all hurt (speed, distance, clamping). 88.3% is a confirmed ceiling at current architecture. The Wolfpack prediction (iter 16) was the last pure geometric gain.
- **Net result: No change. 88.3% confirmed ceiling.**

### Iter 16 — Wolfpack predicted orbit point: +1.6pp average (**COMMITTED**)
**Date:** 2026-04-20
- **Code change:** `ExecuteWolfpack` now computes `predictedPos = target.Position + target.VelocityVector * (currentTick - target.Timestamp)` and uses that for the orbit point calculation.
- Effect: tanks navigate to where the target will be when they arrive, not where it was last scanned. Reduces orbit lag and improves positioning especially at longer range (Sharp at 300px).
- Run 1: Seed 1000: 83% | Seed 2000: 93% | Seed 3000: 89% | Seed 4000: 86% | Seed 5000: 90% = **88.2%**
- Run 2: Seed 1000: 89% | Seed 2000: 87% | Seed 3000: 92% | Seed 4000: 85% | Seed 5000: 89% = **88.4%**
- **Avg 88.3% vs 86.7% baseline → +1.6pp (2 full runs, all seeds improved or held)**
- Seed 2000 variation (93%→87%) is within run-to-run variance. Direction is consistent.
- **COMMITTED**

### Iter 15 — AllyStaleTicks=25 + rush opening (all reverted)
**Date:** 2026-04-20
- **AllyStaleTicks 30→25**: Allies expire 5 ticks sooner; enemy contacts also expire sooner. Expected to reduce late-game stale-contact firing.
  - Result: NOT measured independently — embedded in same session as rush opening test. Combined result was 84.2% avg, clearly worse.
- **Rush opening (tick<50 direct rush, stopDistance=100)**: All tanks charge directly at target for first 50 ticks before switching to orbit positions. Hypothesis: faster initial positioning = earlier focus fire = better first-kill.
  - Seed 1000: 88% | Seed 2000: 80% | Seed 3000: 81% | Seed 4000: 84% | Seed 5000: 88% = **84.2% avg**
  - vs 86.7% baseline = **-2.5pp regression**
  - Seeds 2000 and 3000 severely hurt (-11pp and -5pp). Pattern: aggressive early convergence lets Red Hammer concentrate fire before Blue spreads out.
- Both changes reverted. Code restored to clean baseline (stopDistance=0, no tick-based branch).
- **Net result: No change. 86.7% confirmed ceiling.**

### Iter 14 — Slot/parameter exploration (all reverted)
**Date:** 2026-04-20
- Ecm/Rush slot swap (Ecm→slot2/120°, Rush→slot4/240°): FAILED — 84.6% avg (-2.1pp), seed2000 -7pp. Optimal slot order is locked.
- Energy fire factor 0.1→0.12: FAILED — 85.8% avg (-0.9pp), seeds 1000+5000 regressed.
- Closest-enemy targeting: FAILED — 86.0% avg (-0.7pp), seed2000 -5pp. Lowest-energy targeting optimal.
- BlueTrooper 6th tank (slot 5/300°, PR=200, MaxFP=2.5): MARGINAL — avg 87.2% vs 86.7% (+0.5pp, not significant). Seed2000 -3pp consistently (90%→87%). Seeds 1000+3000 +2.5/+4pp. NOT COMMITTED.
- Key insight: iter 4 failure was 72°×slot5=360°=0° collision. At 60°, slot5=300° is safe. But seed2000 regression is a real pattern.
- **Net result: No change. 86.7% is confirmed ceiling at current config.**

### Iter 13 — PR tuning exploration (all reverted)
**Date:** 2026-04-20
- BlueSharp PR 300→250 (at new slot 3/180°): FAILED — 84.8% vs 86.7% baseline (-1.9pp). Seed 2000 -4pp, most seeds regress. Sharp 300px from behind is correct; 250px brings it into crowded front arc.
- BlueRush PR 180→200: MARGINAL — avg 87.5% vs 86.7% (+0.8pp), seed2000 -2.5pp, seeds4000+5000 +2.5/+3.5pp. Not statistically significant. NOT COMMITTED.
- Insights from current config (seed 1000): BlueSharp BACK to MVP (114/167), BlueGuard Top attacker (rate 27.58). BlueEcm still Red's #1 target (24 first-kills). Red MVP shifted to RedHammer (was Red4).
- **Net result: No change. 86.7% is the current ceiling at 5-seed avg.**

### Iter 12 — Guard/Sharp slot swap: +1.3pp average (**COMMITTED**)
**Date:** 2026-04-20
- **Code change:** BlueGuardCortex.cs FormationSlot 3→1; BlueSharpCortex.cs FormationSlot 1→3
- New formation: Strike(0°,250px) → Guard(60°,200px) → Rush(120°,180px) → Sharp(180°,300px) → Ecm(240°,150px)
- Effect: BlueGuard (top attacker) approaches from front-right (60°) at 200px — more aggressive position. BlueSharp (long range) approaches from behind-left (180°) at 300px — harder for Red to target.
- Run 1: 87/96/84/84/84 = 87.0%  |  Run 2: 86/86/88/84/88 = 86.4%
- **Avg 86.7% vs 85.4% Fallback=20 baseline → +1.3pp (1.6 sigma / 2000 games)**
- Seed 4000 (canary) improved +4pp. Seed 5000 -2pp (within variance). 4/5 seeds improved or neutral.
- Failed during iter 12 exploration: 3-way Pincer (same avg, seed1000 -4pp), Scout-center (80.8%), Fallback=25 (82.8%), Scout radar=90° (81%)
- **COMMITTED**

### Iter 11 — Fallback threshold 30→20: +1.5pp average (**COMMITTED**)
**Date:** 2026-04-20
- **Code change:** `SelectStrategy` Fallback threshold: `sumEnergy/allyCount < 30` → `< 20`
- Effect: Blue stays in Wolfpack/Pincer/Encircle until more depleted; fights more aggressively at low energy instead of retreating to corner
- Run 1: Seed 1000: 84% | Seed 2000: 90% | Seed 3000: 86% | Seed 4000: 78% | Seed 5000: 88% = 85.2%
- Run 2: Seed 1000: 84% | Seed 2000: 88% | Seed 3000: 86% | Seed 4000: 82% | Seed 5000: 88% = 85.6%
- **Avg 85.4% vs 83.9% baseline (2 runs each) → +1.5pp (1.8 sigma / 2000 games)**
- Seed 2000 and 5000 consistently +5pp. Seed 4000 extremely volatile (78-96% in same session).
- Failed alternatives: Fallback=25 (82.8%, clear regression), Fallback=20+center-seeking Scout (80.8%, regression)
- **COMMITTED**

### Iter 10 — Wolfpack angle 72°→60°: +3.6pp average (**COMMITTED**)
**Date:** 2026-04-20
- **Code change:** `ExecuteWolfpack` approach angle: `slot × 72°` → `slot × 60°`
- Effect: 5 tanks spread over 240° arc (vs 360° at 72°) — front-heavy concentration; creates 120° gap at rear
- Seed 1000: 84% (+10pp vs 74% at 72°)  |  Seed 2000: 86% (+5pp)  |  Seed 3000: 82% (0pp)  |  Seed 4000: 84% (+2pp)  |  Seed 5000: 86% (+1pp)
- **5-seed avg: 84.4% vs 80.8% baseline → +3.6pp**
- Also tested: 45° (85.2% avg, within noise, mixed seeds), 30° (84.4% avg, same avg, higher variance) — both reverted
- Pattern: smaller angle = more front-concentrated; 60° is robust optimum
- **COMMITTED** (angle change made in git commit `00f7f08` before iter 10 was formalized)

### Iter 9 — Wolfpack angle-offset formation: +3.8pp average (**COMMITTED**)
**Date:** 2026-04-20
- **Code change:** `ExecuteWolfpack` computes `approachAngle = slot × 72°`, then navigates to `target.PolarOffset(approachAngle, PreferredRange)` instead of directly toward target
- Seed 1000: 74% (−1pp)  |  Seed 2000: 81% (+9pp)  |  Seed 3000: 82% (+6pp)  |  Seed 4000: 82% (−8pp)  |  Seed 5000: 85% (+13pp)
- **5-seed avg: 80.8% vs 77% baseline → +3.8pp**
- Significance: 807 wins / 1000 games vs expected 770 = 2.8 sigma
- Mechanism: distributes Blue tanks around the target at 72° intervals with their own PR as orbit radius, forcing Red to defend from 5 directions simultaneously
- **COMMITTED**

### Iter 8 — Re-validated Guard MaxFP=3.0; all other parameter sweeps failed
**Date:** 2026-04-20
- Guard MaxFP=2.0 re-test: 75.2% avg (5-seed) vs 77% baseline → Guard 3.0 confirmed better, especially seed4000 (+19pp). KEEP Guard 3.0.
- BlueRush PR=160: avg 76.6% vs 77% — neutral (seed4000 -8pp), reverted
- BlueStrike PR=220: avg dropped (seed4000 -18pp), reverted
- BlueEcm PR=200: all seeds regress (seed4000 -17pp), reverted
- BlueRush Retreat=25: avg 78% vs 77% — +1pp but not significant; seed3000 +15pp / seed4000 -16pp swap, reverted
- BlueGuard Retreat=25: avg 72.4% vs 77% — failed (seed3000 -10pp, seed4000 -20pp), reverted
- **Pattern: seed 4000 is an outlier at 90% baseline and drops -10 to -20pp on almost every single-parameter change. 77% average is the single-parameter ceiling.**
- **Net result: No change. Configuration unchanged.**

### Iter 7 — Fix parallel mode static registry bug + 5-seed baseline
**Date:** 2026-04-20
- **Critical discovery:** `SwarmCoordinator.Registry` is a static `ConcurrentDictionary` — in parallel mode, all games share one coordinator, corrupting AllyPings and strategy state across games. Same bug as Red research "PARALLEL MODE BROKEN" finding.
- **Fix:** Changed `OnStart` to `_swarm = new SwarmCoordinator()` (per-tank, per-game) instead of `ForTeam(swarmId)` (shared static instance).
- **5-seed 200-batch baseline after fix:** seed1000=75%, seed2000=72%, seed3000=76%, seed4000=90%, seed5000=72%. **Avg=77%.**
- **Serial mode (--parallel 1):** ~34-36% at seed 1000 — gap vs parallel mode is unexplained.
- Prior iters 3-6 ALL used the broken parallel mode. Direction of changes may still be valid but absolute numbers were unreliable.
- **COMMITTED** (static registry fix is a real bug fix regardless of win rate impact)

### Iter 6 — Target highest-energy enemy (reverted)
**Date:** 2026-04-20
- **Code change:** `RunEpochLogic` priority target: `OrderBy(Energy)` → `OrderByDescending(Energy)` — focus Red's top threat (Hammer) first
- Seed 1000: 73% (within noise of baseline)
- Seed 2000: 52% (severe regression, -35pp)
- Root cause: focusing the highest-energy tank extends time-to-first-kill; Red deals more total damage during the longer fight. Classic focus-fire theory holds — kill the weakest first.
- **REVERTED. No commit.**

### Iter 5 — Exploration of coordination/ECM knobs (all reverted)
**Date:** 2026-04-20
- ECM jam-gap detection (DetectJamGapAndAlert): FAILED (56% seed 1000) — false positives trigger ECMScreen, clusters tanks at 130-unit standoff where Red Hammer concentrates fire
- LeadershipEpochTicks 40→20: FAILED (52% seed 1000) — constant strategy churn, Red decisive wins spike 3x, tanks never settle on targets
- BlueEcm RetreatThreshold 35→20: FAILED (64% seed 2000) — seed 1000 within noise (78%) but seed 2000 drops -23pp; seed 2000 sensitive to BlueEcm behavior changes
- VolleyRange 300→400: FAILED (59%/65%) — far tanks compute negative scheduled fire ticks, volley coordination breaks, shots arrive out of sync
- Leader volley fire-tick correction: FAILED (77%/44%) — self-message already corrects leader's fire tick; the fix caused double-fire conflicts and added redundant shot
- **Net result: No change. Guard MaxFP=3.0 config remains optimum. ~80% seed 1000, ~80% seed 2000 (variance: 63-87%).**

### Iter 4 — 6th Blue tank (BlueTrooper, reverted)
**Date:** 2026-04-20
- **Code change:** Added `public BlueTrooper() : this(5) { }` parameterless constructor so CLI loads Blue5 as 6th tank
- Seed 1000: 78% (within noise of ~80% baseline — inconclusive)
- Seed 2000: 68% (severe regression from 87% baseline)
- Root cause: 6th tank disrupts seed 2000 geometry. Current 5-tank spread (PR: 150-180-200-250-300) is tuned; adding a 6th at PR=200/MaxFP=2.5 clusters Blue and concentrates Red fire.
- **REVERTED. No commit.**
