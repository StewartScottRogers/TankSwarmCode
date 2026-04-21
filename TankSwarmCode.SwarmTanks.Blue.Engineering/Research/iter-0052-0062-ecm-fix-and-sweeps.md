# Research: iter-0052–0062 — BlueEcm Fix + Parameter Exhaustion

## Hypothesis
BlueEcm with MaxFP=5.0 contributes nearly zero combat value (bullet speed 5px/tick). 
Red has no ECM tanks, so ECMScreen never triggers. BlueEcm is just a wasted combat slot.
Fix: MaxFP=5.0→2.0, PR=150→160.

## Baseline (iter-50, committed start of session)
- Slot tanks MaxFP=1.5, PR=160
- BlueEcm MaxFP=5.0, PR=150
- Named tanks (slots 0-4): MaxFP=3.0, PR=200-300
- Mid-range (slots 5-11): MaxFP=2.5, PR=180-200
- 4-seed avg: 47.75%

---

## iter-52: BlueEcm MaxFP 5→2, PR 150→160

**Change:** BlueEcmCortex MaxFP=5.0→2.0, PR=150→160

| Seed | Before | After | Delta |
|------|--------|-------|-------|
| 1000 | 50% | 58% | +8pp |
| 2000 | 46% | 50% | +4pp |
| 3000 | 44% | 54% | +10pp |
| 5000 | 50% | 48% | -2pp |
| **Avg** | **47.75%** | **52.5%** | **+4.75pp** |

**Result: COMMITTED.** BlueEcm was effectively dead weight at MaxFP=5.0.

---

## iter-53: BlueStrike/Guard/Sharp MaxFP 3.0→2.0 (refuted)

**Change:** 3 high-PR named tanks: MaxFP=3.0→2.0, keeping PR

| Seed | Result | Delta |
|------|--------|-------|
| 1000 | 54% | -4pp |
| 2000 | 47% | -3pp |
| 3000 | 50% | -4pp |
| 5000 | 46% | -2pp |
| **Avg** | **49.25%** | **-3.25pp** |

**Refuted.** At PR=200-300, MaxFP=3.0 beats 2.0. Higher damage/hit outweighs marginal accuracy loss.

## iter-53b: BlueTrooper-Phoenix/Rush MaxFP 2.5→3.0 (refuted)

**Change:** 8 mid-range tanks: MaxFP=2.5→3.0

**Avg: 44.75% (-7.75pp)** — Massive regression. MaxFP=2.5 is optimal for PR=180-200.

## iter-54: Mid-range tanks PR 180-200→160 (refuted)

**Change:** BlueTrooper-Phoenix + BlueRush PR → 160

**Avg: 46.5% (-6pp)** — Formation diversity (multiple PR rings) is beneficial, not harmful.

## iter-55: Slot tanks MaxFP 1.5→1.75 (refuted)

**Change:** Blue12-28 MaxFP=1.5→1.75

**Avg: 50.0% (-2.5pp)** — MaxFP=1.5 already optimal for PR=160 slot tanks.

## iter-56: Mid-range tanks MaxFP 2.5→2.0 (refuted)

**Change:** BlueTrooper-Phoenix + BlueRush MaxFP=2.5→2.0

**Avg: 47.5% (-5pp)** — MaxFP=2.5 confirmed optimal for PR=180-200.

## iter-57: BlueEcm MaxFP 2.0→1.5 (refuted)

**Change:** BlueEcm MaxFP=2.0→1.5

**Avg: 48.75% (-3.75pp)** — BlueEcm at MaxFP=2.0 is uniquely optimal (not 1.5 despite PR=160).
The ECM overhead or slot-4 position creates different dynamics than slot tanks.

## iter-58: Fallback threshold 10→5 (refuted)

**Change:** SwarmCoordinator fallback threshold 10.0→5.0

**Avg: 51.25% (-1.25pp)** — Consistent with Red's finding that 10≈5; 10 slightly better.

## iter-59: BlueStrike PR 250→200 (refuted)

**Change:** BlueStrike PR=250→200

5-seed avg: 46.4% (-4.8pp from 51.2%). BlueStrike at PR=250 is critical. Outer orbit position essential.

## iter-60: Target selection: weakest→closest (refuted)

**Change:** Priority target = closest enemy (vs weakest)

Seed 2000 improved +5pp but seed 5000 crashed -6pp. 
**Avg: 49.75% (-2.75pp)** — "Weakest first" (kill economy) is superior overall.

## iter-61: Rank-based orbit angles (refuted)

**Change:** Wolfpack/Encircle: `slot % aliveCount` → `rank among alive allies`

**Avg: 46.25% (-6.25pp)** — Original modular formula is correct. Rank-based approach destabilizes 
late-game angles (rank changes as ally map updates create oscillation).

## iter-62: Named tanks RetreatEnergyThreshold → 0 (neutral)

**Change:** BlueGuard/ECM/Rush/Sharp/Strike: Retreat threshold → 0

**Avg: 52.0% (-0.5pp)** — Neutral. Retreat thresholds rarely trigger in 29v29 (Scatter only fires
when tank is alone AND below threshold). Not meaningful.

---

## Key Findings

### MaxFP Optimal Values by PR Range
| PR Range | Optimal MaxFP | Reasoning |
|----------|--------------|-----------|
| 160 (slot) | 1.5 | Accuracy-bound; faster bullets dominate |
| 160 (BlueEcm) | 2.0 | ECM slot behaves differently |
| 180 | 2.5 | Mid-range sweet spot |
| 200 | 2.5 (most) / 3.0 (BlueGuard) | Transition zone |
| 250 (BlueStrike) | 3.0 | Damage-bound; critical outer orbit |
| 300 (BlueSharp) | 3.0 | Damage-bound |

### PR Configuration
The tiered ring structure is beneficial:
- Inner ring: PR=160 (18 tanks: Blue12-28 + BlueEcm)
- Middle ring: PR=180-200 (8 tanks: BlueTrooper-Phoenix, BlueRush)
- Outer ring: PR=200-300 (3 tanks: BlueGuard, BlueStrike, BlueSharp)

Do NOT compress these rings — formation diversity creates better coverage.

### Architecture: Slot %aliveCount Orbit
The `slot % aliveCount * (360/aliveCount)` orbit formula is correct and robust.
Rank-based alternatives destabilize late-game formation. Leave unchanged.

### Red Has No ECM
BlueEcm's ECM hardware is permanently irrelevant in this matchup.
Red has zero ECM tanks. ECMScreen strategy never activates.

## Current Ceiling
- 4-seed avg: **52.5%** (iter-52 committed)
- 5-seed avg: **~51.2%** (seeds 1000=58%, 2000=50%, 3000=54%, 4000=46%, 5000=48%)
- Red LoopState: 57.6% (5-seed, against older Blue DLL)
- Remaining gap: ~6.4pp

The parameter space for Wolfpack architecture appears exhausted at ~52.5%.
Further gains likely require new algorithmic approaches (e.g., reactive targeting,
dynamic PR adjustment, or cross-tank coordination not currently implemented).
