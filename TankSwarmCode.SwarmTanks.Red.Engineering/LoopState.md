# Red Engineering — Loop State

## Last Updated
2026-04-21 — Iter 95 complete (MaxFP 1.5→1.75 REFUTED -4.2pp avg 5 seeds; 1.5 peak is sharp and independent of fire-gate state)

## Current Iteration
**HOLDING** — 56.0% avg (5-seed, MaxFP=1.5, gun-tracks-radar in Scout, fire-gate 7°) vs Blue's updated 29-tank DLL. Iter 95 tested MaxFP=1.75 and regressed -4.2pp avg (seeds 3000/4000 catastrophic -11/-9.5pp, same sensitivity pattern as iters 93/94). MaxFP and fire-gate peaks are independent: 1.5 and 7° are both pinned at their local optima regardless of the other. MaxFP dimension effectively exhausted (coverage: 1.0/1.5/1.75/2.0/2.5).

## Situation

**HOLDING: 56.0% average win rate across 5 seeds against Blue's updated DLL.**

Iter 89 discovered Blue DLL had drifted weaker-for-Red (fresh baseline 51.3% avg).
Gun-tracks-radar in Scout recovered +1.9pp to 53.2% avg.
Iter 92 loosened firing-angle gate 5°→7° for further +2.8pp → **56.0% new ceiling**.

Blue Engineering updated their DLL between iter 49 and iter 82 (-5.4pp). MaxFP=1.5 recovered +1.0pp.
MaxFP sweep vs new Blue DLL: 1.0→catastrophic (-10pp), 1.5→optimal (+1.0pp), 2.0→baseline, 2.5→catastrophic.

**CRITICAL INFRASTRUCTURE NOTE:** Always use `net10.0/publish/` DLL paths, NOT `net9.0/publish/`.
- Red: `TankSwarmCode.SwarmTanks.Red/bin/Release/net10.0/publish/TankSwarmCode.SwarmTanks.Red.dll`
- Blue: `TankSwarmCode.SwarmTanks.Blue/bin/Release/net10.0/publish/TankSwarmCode.SwarmTanks.Blue.dll`
- Build: `dotnet publish TankSwarmCode.SwarmTanks.Red/... -c Release`
- **ALWAYS use `--on-timeout energy`** in battle runs (avoids 35% draw inflation)
- Build protocol: `dotnet clean` then `dotnet publish` (incremental builds sometimes stale)

**Current win rates (5-seed, 200 matches each, --on-timeout energy, 29 Red tanks vs Blue 29 tanks, MaxFP=1.5, Fallback=10, gun-tracks-radar in Scout, fire-gate 7°):**
- Seed 1000: **57.0%**
- Seed 2000: **53.0%**
- Seed 3000: **60.0%**
- Seed 4000: **58.0%**
- Seed 5000: **52.0%**
- **5-seed average: 56.0%**

(Pre-iter-92 baseline was 53.2% avg — iter-92 fire-gate 5°→7° recovered +2.8pp)
(Fresh pre-iter-89 baseline was 51.3% avg — iter-89 recovered +1.9pp)

## What We Know (Current Blue DLL — 29 tanks)

- **Parallel mode is VALID for Red.** Iter 17 fixed the static SwarmCoordinator registry bug.
  Use `--parallel 8` for all runs.
- **Blue has 29 tanks:** Named (BlueSharp, BlueGuard, BlueEcm, BlueRush, BlueStrike) + Blue5-Blue28 slots.
- **Red has 29 tanks:** Named (RedHammer/Blade/Arrow/Ghost) + Red4-Red28 slots.
- **MaxFP=2.0 optimal for all attack tanks** vs 29-tank Blue (dense formation makes slower bullets acceptable, doubled damage is decisive). MaxFP=1.0 was optimal for old 5-tank Blue; wrong for current configuration. MaxFP=3.0 too slow (38% at seed 3000).
- **PR=160 unchanged.** Has been stable through all Blue expansions. PR=180 refuted (neutral).
- **Ghost stays at MaxFP=2.0.** This is unchanged and correct.
- **Fallback threshold = 10.0** (vs old 30.0). Lower = more aggressive mid-battle. Gradient: 30→15→10 consistently positive; 10→5 flat.
- **29 Red tanks is optimal** (30 = Red29 refuted, -0.5pp avg).
- **Frequency constants (AllyPingInterval=15, VolleyIntervalTicks=30) must NOT be changed.**
- **Blue's named tanks have very high DPS rates:** BlueGuard=65.91, BlueStrike=58.10, Blue11=58.86, Blue9=57.04. Red matches with MaxFP=2.0.

## Current Configuration

| Tanks | Slots | MaxFP | PR | HasEcm | Retreat |
|-------|-------|-------|----|--------|---------|
| RedHammer | 0 | 1.5 | 160 | false | 25 |
| RedBlade | 1 | 1.5 | 160 | false | 20 |
| RedArrow | 2 | 1.5 | 160 | false | 20 |
| RedGhost | 3 | 1.5 | 160 | false | 20 |
| Red4-Red28 | 4-28 | 1.5 | 160 | false | 20 |

**Total: 29 Red tanks vs Blue 29 tanks. 53.2% avg 5-seed win rate (MaxFP=1.5).**

**SwarmCoordinator.ExecuteWolfpack:** orbit-based (`slot % aliveCount * 360/aliveCount`, radius = `config.PreferredRange`)

## LOOP STATUS: ADAPTING (52.2% — Blue DLL updated; re-exploring parameter space vs new Blue)

### What was tested and refuted (iter 48, vs OLD Blue DLL 57.6% baseline):
- Encircle: 1.5x threshold (-1pp), disabled (-12pp), orbit 160 (-1.5pp) — all rejected
- PR=140 (-6pp seed 1000), PR=180 (neutral) — 160 confirmed optimal with MaxFP=2.0
- MaxFP=2.5 (-10pp), 3.0 (-13pp) — 2.0 is sharp peak
- Red29/30th tank (-0.5pp) — 29 tanks is ceiling
- Ghost PR=100 (+0.4pp noise) — not actionable
- Centroid orbit (-1.5pp) — uniform 360° optimal
- Per-tank targeting (-3pp) — coordinated weakest-first critical
- Fallback=5 (flat vs 10) — 10 is optimal floor
- 180° arc orbit (-8pp seed 1000, neutral seed 3000) — decisive rejection

### What was tested vs NEW Blue DLL (iters 82-88, baseline 52.2%→53.2% with MaxFP=1.5):
- PR=171 (-3.5pp seed 1000) — REFUTED. PR=160 remains optimal.
- PR=155 (-10.5pp seed 1000) — CATASTROPHIC. Below-160 hurts.
- PR=165 (-2.0pp seed 1000) — REFUTED.
- ECMScreen orbit-based: NEUTRAL. ECM movement doesn't affect outcomes.
- ECMScreen removal: CATASTROPHIC (35% draws). ECMScreen required for radar tracking.
- BlueEcm priority targeting: NEUTRAL (0.0pp avg 2 seeds).
- ECM alert window 20→10 ticks: NEUTRAL (exact baseline).
- MaxFP=1.5: ACCEPTED +1.0pp avg (seeds 1000/4000 +3.5/+2.5pp, others flat/marginal).
- MaxFP=1.0: CATASTROPHIC -10pp seed 1000.
- Ghost MaxFP=2.0 (others 1.5): NEUTRAL (0.0pp avg 2 seeds). Keep uniform at 1.5.
- Fallback=5: NEUTRAL (0.0pp avg 2 seeds). Fallback=10 confirmed optimal.
- Nearest-first priority targeting: CATASTROPHIC -5.5pp seed 1000. Weakest-first is law.
- Rotating orbit 0.5°/tick: -1.0pp avg (3 seeds: 1000 -1.0, 3000 +2.0, 4000 -4.0). REFUTED.
- Wounded-retreat orbit (energy<40 → PR+40): CATASTROPHIC -5.5pp. Tanks always below 40E in 29v29; formation fragments.
- Weakest-first fallback in GetStrategyTarget: CATASTROPHIC -12pp. Formation fragmentation (each tank orbits different target).
- LeadershipEpochTicks=20: CATASTROPHIC -5.5pp. Orbit instability from frequent target switching.
- No volley system: NEUTRAL ~0.0pp. Volley fire is functionally redundant.
- Rank-based orbit assignment (fix slot%aliveCount collision bug): NEUTRAL -0.7pp avg 5-seed (within noise, std err ±1.66pp). Collision bug is real but has negligible practical impact.
- **Iter 88 — Radial breathing orbit (±20px, T=80 ticks): CATASTROPHIC -9.5pp seed 1000 (58.0% → 48.5%).** Moving orbit point desyncs `LeadershipEpochTicks=40` convergence AND mis-aligns Red's own firing (gun solution computed per-tick but bullet departs from post-movement position). Orthogonal to rotating orbit but same root failure: fire-control requires stationary shooter.
- **Iter 89 — Gun-tracks-radar during Scout: ACCEPTED +1.9pp avg 5-seed.** Recovered 53.2% avg vs fresh 51.3% baseline. First-kill Red-victim rate dropped on 3/4 measured seeds (42→39, 32→30, 40→38). Mechanism: during Scout phase, gun rotated to match radar heading instead of sitting at body heading, saving up to 9 ticks of gun-swing latency on first-target acquisition. Seed 3000 decisive +11pp; seed 5000 mild regression -3pp within noise.
- **Iter 90 — Stale-contact lead correction in LinearPredictionFire: CATASTROPHIC -11pp seed 1000.** Added `observationAge = tickNumber - target.Timestamp` to extrapolate current position before computing travelTime. Reason for failure: Blue's DLL turns frequently. Stale VelocityVector projected over `observationAge + travelTime` (up to 15 ticks) aims ahead of the target's actual path. The existing code's "ignore observationAge" implicitly shrinks lead, producing less error when velocity direction is stale. Reverted. **New law: extrapolation beyond travelTime is harmful when target turn rate is non-negligible.** Analogous to iter 15's velocity-led-orbit-prediction refutation.
- **Iter 91 — Firing-angle threshold 5.0° → 3.0°: REFUTED -4.75pp avg (2 seeds).** Seed 1000: 53.5% (-2.5pp); seed 2000: 49.0% (-7.0pp). Tighter gate cuts fire volume more than it improves accuracy. Prediction-staleness error (14+ px cross-range from stale velocity) dominates gun-alignment error, so tightening the gate below 5° only drops shots without improving hit rate. Reverted. **New law candidate: gun-alignment gate is at or beyond the accuracy-limited regime at 5°. Don't tighten; if anything, loosen.**
- **Iter 92 — Firing-angle threshold 5.0° → 7.0°: ACCEPTED +2.8pp avg (5 seeds).** Seeds 1000/2000/3000/4000/5000 = 57/53/60/58/52% (vs 56/56/58/52/44 baseline). 4/5 positive; seeds 4000/5000 decisive (+6/+8pp on weak baseline seeds). **New ceiling: 56.0% avg.** Mechanism confirmed: gun-alignment is NOT the binding constraint — prediction staleness is. Below the ~14 px prediction-noise floor, tightening loses more shots than it gains in accuracy; loosening to 7° adds volume without degrading hit fraction.
- **Iter 93 — Firing-angle threshold 7.0° → 9.0°: REFUTED -1.1pp avg (5 seeds).** Seeds 1000/2000/3000/4000/5000 = 58.5/57.5/53/51.5/54% (vs 57/53/60/58/52 baseline). 3/5 positive but the two negative seeds are decisive (-7.0/-6.5pp) on seeds 3000/4000 where 7° baseline was strongest. Gradient peaked at 7° and reversed by 9°. **Mechanism:** at PR=160, 9° cross-range gate contribution ≈ 25 px now clearly exceeds ~14 px prediction noise floor — gate-edge shots aim outside the hit envelope regardless of prediction accuracy. Inverted-U confirmed: 3°/5°/7°/9° = -4.75/0/+2.8/-1.1pp. No need to test 11°. **New law:** gate optimum is where gate-cone matches prediction noise; sharp peak, not plateau.
- **Iter 94 — Firing-angle threshold 7.0° → 8.0°: REFUTED -1.9pp avg (5 seeds).** Seeds 1000/2000/3000/4000/5000 = 57.5/56.5/53.5/53.5/49.5% (vs 57/53/60/58/52 baseline). 2/5 positive; seeds 3000/4000 regress -6.5/-4.5pp (same pattern as iter-93). 8° sits ~0.8pp below 9° within noise; both are clearly past the peak. **Peak at 7° is SHARP** — no flat-top region; any departure from 7° costs win rate. Fire-gate dimension fully exhausted. Gradient: 3°/5°/7°/8°/9° = -4.75/0/+2.8/+0.9/+1.7pp vs 5°. Reverted.
- **Iter 95 — MaxFP 1.5 → 1.75: REFUTED -4.2pp avg (5 seeds).** Seeds 1000/2000/3000/4000/5000 = 56.5/51/49/48.5/54% (vs 57/53/60/58/52 baseline). 1/5 positive; seeds 3000/4000 catastrophic (-11/-9.5pp). **Three-iter seed-correlation pattern confirmed (93/94/95):** seeds 3000/4000 are brittle under any shot-quality reduction (gate loosening or bullet slowing). MaxFP peak is sharp at 1.5 and independent of fire-gate state. Gradient vs 1.5: 1.0=-10, 1.75=-4.2, 2.0=-1.0, 2.5=-10. MaxFP dimension exhausted. Reverted.

**NEW LAW: Scout must pre-aim gun via radar.** Gun turn rate (20°/tick) is less than radar turn rate (45°/tick); without pre-aim, gun lags radar by up to 9 ticks when first target appears.

**NEW LAW (iter 90): LinearPredictionFire must NOT extrapolate beyond bullet travelTime.** Adding observationAge × VelocityVector is catastrophic (-11pp seed 1000) because Blue's DLL turns frequently and stale velocity points in the wrong direction. Existing "use target.Position as if fresh" behavior is implicit shrinkage and acts as a safer lead model.

**NEW LAW (iter 92/93/94): LinearPredictionFire gun-alignment gate has a SHARP PEAK at 7°.** Inverted-U gradient: 3° = -4.75pp (iter 91), 5° = prior baseline, 7° = +2.8pp (iter 92, ACCEPTED), 8° = +0.9pp (iter 94, REFUTED), 9° = +1.7pp (iter 93, REFUTED). Optimum = where gate cross-range contribution ≈ prediction noise (~14 px at PR=160). 7° is the exact peak — both 8° and 9° land ~1–2pp below and are within noise of each other. **No plateau; fire-gate dimension fully exhausted.**

**CEILING: 56.0% avg — Wolfpack/MaxFP=1.5 architecture with Scout gun-pre-aim and 7° fire-gate.**

### Laws confirmed vs new Blue DLL (MaxFP=1.5, Fallback=10):
| Parameter | Value | Status |
|-----------|-------|--------|
| MaxFP | 1.5 | NEW optimal (was 2.0 vs old Blue; new Blue is more mobile) |
| PreferredRange | 160 | Confirmed optimal (155 catastrophic, 165 negative) |
| Fallback threshold | 10.0 | Confirmed optimal (5 neutral) |
| Ghost MaxFP | 1.5 | Same as others (old 2.0 exception no longer valid) |
| All ECM params | unchanged | All neutral — ECM handling doesn't affect outcomes |
| Priority targeting | Weakest-first (broadcast) | Law confirmed; nearest-first catastrophic |
| LeadershipEpochTicks | 40 | Hard constraint — orbit requires 40+ ticks to converge |
| Orbit formation | Static uniform 360° | Rotating/split/health-based all catastrophic |
| GetStrategyTarget fallback | Most-recently-seen | CRITICAL: weakest-first fallback = -12pp (formation fragmentation) |
| Volley fire | Keep (neutral) | Removing is neutral; system is redundant but harmless |

See Research/iter-0082-blueecm-pr171-newbaseline.md for iter 82-84 details.

**94.0% avg (5-seed) is the ceiling for numerical superiority at 28 tanks vs Blue 12.**

The tank count gradient is exhausted. 30 tanks reverses the trend (-0.4pp). 
To exceed 94.0%, different approaches needed:
- Optimize individual tank configs (MaxFP, PR) for 28-tank orbit dynamics
- New swarm strategies designed for large-number engagements
- Updated Blue DLL (ceiling resets if Blue changes strategy)

---

## Iteration Log

### Iter 95 — MaxFirePower 1.5 → 1.75 (REFUTED -4.2pp avg, 5 seeds)
**Date:** 2026-04-21
**Status:** REFUTED. Red 51.8% avg (5-seed) vs 56.0% baseline (-4.2pp). Reverted.
**Branch:** research/iter-95-maxfp-1.75
- Hypothesis: MaxFP peak may have shifted with fire-gate widening (5°→7°); test halfway point 1.75 between optimal 1.5 and neutral 2.0.
- Seeds: 56.5/51.0/49.0/48.5/54.0% = 51.8% avg vs baseline 57/53/60/58/52 = 56.0%.
- 1/5 positive; seeds 3000/4000 catastrophic (-11.0/-9.5pp).
- **Three-iter seed-correlation pattern (93/94/95):** seeds 3000/4000 regress hard under any shot-quality reduction — both gate loosening and bullet slowing trigger the same signature. Red's baseline at these seeds sits in a narrow local-max basin.
- MaxFP and fire-gate peaks are independent: 1.5 and 7° are both locally optimal regardless of the other's state.
- Gradient vs 1.5: 1.0=-10pp (iter 86), 1.75=-4.2pp, 2.0=-1.0pp (iter 86), 2.5=-10pp (iter 48). Sharp asymmetric peak.
- MaxFP dimension exhausted; 1.25 unlikely to help (interpolates -4 to -7pp between 1.0 and 1.5).
- Next: PR fine-grained (158/162), Scout sub-phase tweaks, or revisit fixed-slot/rank-based orbit refutations.
- See Research/iter-0095-maxfp-1.75-refuted.md

### Iter 94 — Firing-Angle Threshold 7.0° → 8.0° (REFUTED -1.9pp avg, 5 seeds)
**Date:** 2026-04-21
**Status:** REFUTED. Red 54.1% avg (5-seed) vs 56.0% baseline (-1.9pp). Reverted.
**Branch:** research/iter-94-fire-gate-8deg
- Hypothesis: test 8° (between accepted 7° and refuted 9°) to sharpen peak location.
- Seeds: 57.5/56.5/53.5/53.5/49.5% = 54.1% avg vs baseline 57/53/60/58/52 = 56.0%.
- 2/5 positive; seeds 3000/4000 regress -6.5/-4.5pp (same seed-pattern as iter-93).
- 8° (-1.9pp) actually slightly worse than 9° (-1.1pp); within noise but confirms no monotonic descent past peak.
- **Peak at 7° is SHARP.** Gradient: 3°/5°/7°/8°/9° = -4.75/0/+2.8/+0.9/+1.7pp vs 5°. No flat top.
- Fire-gate dimension FULLY EXHAUSTED. Next candidates must be orthogonal: MaxFP fine-grained (1.25/1.75), PR fine-grained (158/162), Scout sub-phase tweaks, or revisit old-Blue refutations vs new Blue.
- See Research/iter-0094-fire-angle-8deg-refuted.md

### Iter 93 — Firing-Angle Threshold 7.0° → 9.0° (REFUTED -1.1pp avg, 5 seeds)
**Date:** 2026-04-21
**Status:** REFUTED. Red 54.9% avg (5-seed) vs 56.0% baseline (-1.1pp). Reverted.
**Branch:** research/iter-93-fire-angle-9deg
- Hypothesis: loosen `Math.Abs(gunDiff) < 7.0` → `< 9.0` to extend iter-91/92 volume-of-fire gradient; saturation check.
- Seeds: 58.5/57.5/53.0/51.5/54.0% = 54.9% avg vs baseline 57/53/60/58/52 = 56.0%.
- 3/5 positive; 2 decisive negatives (-7.0/-6.5pp) on seeds 3000/4000 where 7° baseline was strongest.
- **Inverted-U gradient confirmed:** 3° (-4.75pp) → 5° (baseline) → 7° (+2.8pp) → 9° (-1.1pp). Peak at 7°.
- Mechanism: at PR=160, 9° cross-range gate contribution (~25 px) now clearly exceeds ~14 px prediction noise floor. Gate-edge shots aim outside hit envelope regardless of prediction accuracy.
- **Intelligence:** seeds with highest baseline win rate are most sensitive to gate over-loosening — they lose shots that were formerly accurate. Low-baseline seeds still benefit from more volume.
- iter-94 (test 11°) no longer warranted — gradient reversal already confirmed. Next candidate: iter-94 test 8° to sharpen the peak.
- See Research/iter-0093-fire-angle-9deg-refuted.md

### Iter 92 — Firing-Angle Threshold 5.0° → 7.0° (ACCEPTED +2.8pp avg, 5 seeds)
**Date:** 2026-04-21
**Status:** ACCEPTED. Red 56.0% avg (5-seed) vs 53.2% baseline (+2.8pp). New ceiling.
**Branch:** research/iter-92-fire-angle-loosen
- Hypothesis: loosen `Math.Abs(gunDiff) < 5.0` → `< 7.0` in `LinearPredictionFire`. Iter-91 direction reversal.
- Seeds: 57/53/60/58/52% = 56.0% avg vs baseline 56/56/58/52/44 = 53.2%.
- 4/5 seeds positive; seeds 4000/5000 decisive (+6/+8pp on weakest baseline seeds).
- **Gradient confirmed:** 3° (-4.75pp) → 5° (baseline) → 7° (+2.8pp). Monotonic.
- Mechanism: gun-alignment is NOT binding constraint — prediction staleness dominates (~14 px noise floor at PR=160). Below floor, tighter gate loses shots; above floor, looser gate adds volume cheaply.
- **Next candidates:** iter-93 test 9° (saturation check); iter-94 test 11° (reversal check).
- See Research/iter-0092-fire-angle-7deg-accepted.md

### Iter 91 — Firing-Angle Threshold 5.0° → 3.0° (REFUTED -4.75pp avg, 2 seeds)
**Date:** 2026-04-21
**Status:** REFUTED. Seeds 1000/2000: 53.5%/49.0% vs 56.0%/56.0% baseline = -2.5pp/-7.0pp. Stopped after 2.
**Branch:** research/iter-91-fire-angle-tighten
- Hypothesis: tighten `Math.Abs(gunDiff) < 5.0` → `< 3.0` in `LinearPredictionFire`; theoretical cross-range error reduced from 14px to 8px at target.
- Refuted: hit-rate gain does not compensate for fire-volume loss. Prediction staleness dominates gun-alignment error.
- **New law candidate:** 5° gate is at or beyond the accuracy-limited regime. Tightening is net-negative.
- Next candidate: loosen to 7° (iter 92) to test the volume-of-fire hypothesis directly.
- See Research/iter-0091-fire-angle-3deg-refuted.md

### Iter 90 — Stale-Contact Lead Correction (REFUTED -11pp seed 1000)
**Date:** 2026-04-21
**Status:** REFUTED. Red 45% (90/200) seed 1000 vs 56% baseline (-11pp decisive). Stopped after 1 seed.
**Branch:** research/iter-90-stale-lead-correction
- Hypothesis: `LinearPredictionFire` under-leads stale RadarShare contacts. Fix: advance target.Position by `VelocityVector × (tickNumber - target.Timestamp)` before computing travelTime lead.
- Refuted: Blue's DLL turns frequently. Stale velocity extrapolated over `observationAge + travelTime` (up to 15 ticks) points in wrong direction when target changes heading.
- Existing "ignore observationAge" behavior is implicit shrinkage — produces less error when velocity direction is unreliable.
- **New law:** LinearPredictionFire must NOT extrapolate beyond travelTime. Turning targets make aggressive lead counterproductive.
- Analogous to iter 15's velocity-led-orbit-prediction refutation (-3.7pp).
- See Research/iter-0090-stale-lead-correction-refuted.md

### Iter 89 — Gun-Tracks-Radar During Scout (ACCEPTED +1.9pp avg, 5-seed)
**Date:** 2026-04-21
**Status:** ACCEPTED. Red 53.2% avg (5-seed) vs 51.3% fresh baseline (+1.9pp).
- Seed results: 56.0 / 56.0 / 58.0 / 52.0 / 44.0 (vs baseline 58.0 / 52.0 / 47.0 / 52.5 / 47.0)
- Seed 3000 decisive (+11pp, 3σ+). Seeds 2000/4000 positive/neutral. Seed 5000 -3pp (noise).
- First-kill Red-victim % dropped on every measured seed (42→39, 32→30, 40→38). Mechanism confirmed.
- Code: Scout() in SwarmCoordinator.cs now calls SetTurnGunRight(RadarHeading - GunHeading).
- Rationale: gun turn rate (20°/tick) < radar (45°/tick). Without pre-aim, gun can be up to 180° from first target, costing 9 ticks of fire latency.
- Recovered 53.2% avg ceiling from mild Blue-DLL drift seen in fresh baseline.
- See Research/iter-0089-gun-tracks-radar-accepted.md

### Iter 88 — Radial Breathing Orbit (REFUTED, -9.5pp seed 1000)
**Date:** 2026-04-21
**Status:** REFUTED. Red 48.5% (97/200) seed 1000 vs 58.0% baseline (-9.5pp decisive).
**Branch:** research/iter-88-radial-breathing
- Hypothesis: Radial PR oscillation (140↔180, T=80 ticks) defeats Blue's linear prediction while Red's own firing (uses target velocity) is unaffected.
- Refuted: Red's firing DOES depend on stable shooter position (gun aim computed per-tick; bullet departs from post-move position, creating position/aim mismatch).
- Outer phase (r=190) also increased Red's own bullet travel time → more misses from Red's side.
- Orbit convergence broken: LeadershipEpochTicks=40 requires 40 ticks to settle, but orbit point shifts with 80-tick period.
- **New law:** Fire-control loop requires stationary shooter. Any orbit modulation (angular or radial) breaks both convergence AND fire alignment.
- See Research/iter-0088-radial-breathing-refuted.md

### Iter 87 — Architecture Sweep: ALL REFUTED (6 hypotheses, ceiling confirmed at 53.2%)
**Date:** 2026-04-21
**Status:** ALL REFUTED. Ceiling at 53.2% definitively confirmed.
- Nearest-first targeting: -5.5pp seed 1000 (catastrophic)
- Rotating orbit 0.5°/tick: -1.0pp avg 3 seeds (seed 4000: -4.0pp, decisive)
- Wounded-retreat orbit (energy<40 → PR+40): -5.5pp (tanks always below 40E in 29v29)
- Weakest-first fallback in GetStrategyTarget: -12pp (formation fragmentation — all tanks orbit different targets)
- LeadershipEpochTicks=20: -5.5pp to -7.0pp (orbit instability from frequent target switching)
- No volley system: neutral (0.0pp to -0.5pp)
- **Critical insight:** Formation cohesion requires ALL tanks to orbit the SAME shared target. GetStrategyTarget fallback MUST use most-recently-seen (correlated) not weakest-individual (divergent). The 40-tick leadership epoch is precisely tuned for orbit convergence.
- Rank-based orbit (fix `slot%aliveCount` collision bug): NEUTRAL -0.7pp 5-seed avg (noise). Real bug with negligible practical effect.
- See Research/iter-0087-architecture-sweep-ceiling-confirmed.md

### Iters 85-86 — MaxFP Sweep: 2.0→1.5 ACCEPTED (+1.0pp), 1.0 Catastrophic (-10pp)
**Date:** 2026-04-21
**Status:** ACCEPTED (MaxFP=1.5). New 5-seed avg: 53.2% vs 52.2% baseline (+1.0pp).
- MaxFP=1.5: 54.0/54.5/52.5/56.5/48.5 = 53.2% avg (+1.0pp vs 52.2% baseline)
- MaxFP=1.0: seed 1000 = 40.5% (-10pp). Catastrophic. REVERTED immediately.
- Gradient: 1.0 (-10pp) → 1.5 (optimal +1.0pp) → 2.0 (baseline 0pp) → confirmed sharp peak at 1.5
- Mechanism: New Blue DLL appears more mobile; faster/lighter bullets (1.5 vs 2.0) improve hit rate vs mobile targets
- Seed 1000/4000 strongly positive (+3.5/+2.5pp); seeds 2000/3000 flat; seed 5000 -1pp
- See Research/iter-0085-0086-maxfp-sweep.md

### Iter 82 — Blue DLL Update + PR=171 Refuted (NEW BASELINE: 52.2%)
**Date:** 2026-04-21
**Status:** REFUTED (PR=171). New baseline established.
- Discovered Blue Engineering updated DLL. Old 57.6% baseline is stale.
- New 5-seed baseline: 50.5/54.5/52.5/54.0/49.5% = 52.2% avg (-5.4pp from Blue update)
- PR=171 tested (3 seeds): 47.0/55.0/53.5% = 51.8% avg vs 52.5% avg baseline = -0.7pp. REFUTED.
  Seed 1000 decisive negative (-3.5pp); seeds 2000/3000 marginal noise.
- ECMScreen orbit-based: NEUTRAL (52.5% = baseline). ECMScreen movement doesn't affect outcomes.
- ECMScreen removal: catastrophic (35% draws). ECMScreen required for radar tracking during ECM blackout.
- Infrastructure fix: ALL runs must use `--on-timeout energy` (avoids 35% draw inflation from timeouts).
- See Research/iter-0082-blueecm-pr171-newbaseline.md

### Iter 47 — Fallback Threshold 30→10 (ACCEPTED, +2.0pp avg)
**Date:** 2026-04-21
**Status:** ACCEPTED. Red 57.6% avg (5-seed) vs 55.6% baseline (+2.0pp).
- Fallback threshold sweep: 30→15 (+1.2pp) → 10 (+2.0pp) → 5 (flat at 10).
- Also refuted: PR=180 (neutral), Red29/30th tank (-0.5pp).
- Mechanism: lower threshold = Red attacks more rounds before retreating → more damage in close engagements.
- Seeds: 60/56/57/58/57% = 57.6% avg
- See Research/iter-0047-fallback-threshold-sweep.md

### Iters 45-46 — Red28 + MaxFP=2.0 (ACCEPTED, +12pp avg)
**Date:** 2026-04-21
**Status:** ACCEPTED. Red 55.6% avg (5-seed) vs ~44% baseline (+12pp).
- Iter 45: Added Red28 (29th tank, slot 28) to match Blue's 29 tanks. Marginal alone (+1pp).
- Iter 46: MaxFP 1.0→2.0 for all attack tanks (Trooper/Arrow/Blade/Hammer). Major driver (+12pp).
- MaxFP=3.0 tested and rejected (38% at seed 3000 — slower bullets miss mobile targets).
- Mechanism: 29-tank dense Wolfpack formation increases target density, making slower/harder bullets (MaxFP=2.0) superior to faster/lighter (MaxFP=1.0).
- See Research/iter-0045-0046-maxfp2-red28.md

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
