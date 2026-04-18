# Karpathy Loop — Automated Balance Research

A hypothesis-driven experiment cycle for tank balance investigation using the CLI.

## The Loop

```
Hypothesize → CLI run → Analyze → Update hypothesis → repeat
```

Each iteration is one Claude session. The loop terminates when the hypothesis is confirmed, refuted, or redirected.

---

## 1. Hypothesize

State a concrete, falsifiable claim about tank behavior or balance before running anything.

Bad: "Check if RedGhost is good."  
Good: "RedGhost carries Red wins via ECM outlasting, not combat (timeout rate, not dmg output)."

Success criteria must be measurable:
- win-survival rate (WinSurv column)
- combat rate per 100t (Rate/100t)
- solo carry count (Solo carry insight)
- role assignments (ECM, MVP, Hider, etc.)

---

## 2. Design the Experiment

Pick parameters that give clean signal:

| Parameter | Research default | Notes |
|---|---|---|
| `--batch` | 100–200 | ≥100 for stable percentages |
| `--parallel` | 8 | Matches available CPU cores |
| `--seed` | 1000 | Fixed for reproducibility |
| `--on-timeout` | `energy` | Eliminates draws; reveals true balance |
| `--format` | `table` | Summary stats auto-included |

Arena size affects ECM effectiveness:
- Default (800×600): ECM tanks can hide, high draw rates without `energy` policy
- Cramped (350×250): buffs close-quarters, nerfs hiding strategies

---

## 3. Run

```bash
# Standard 200-match benchmark
TankSwarmCode.Cli \
  --bot1 ..\TankSwarmCode.SwarmTanks.Red\bin\Release\net9.0\publish\Red.dll \
  --bot2 ..\TankSwarmCode.SwarmTanks.Blue\bin\Release\net9.0\publish\Blue.dll \
  --batch 200 --parallel 8 --seed 1000 \
  --on-timeout energy \
  --format table

# Cramped arena variant (ECM stress test)
TankSwarmCode.Cli ... --width 350 --height 250

# CSV export for external analysis
TankSwarmCode.Cli ... --format csv > results.csv
```

---

## 4. Analyze

Read the summary table bottom-up:

1. **Win tally** — overall balance (aim for 45–55% each side)
2. **Win quality** — Blue decisive vs Red timeout pattern? `(N% via TO)` fires when >20%
3. **Decisive med** — Blue 150–220t blitz vs Red 350–550t grind is the baseline
4. **Per-tank WinSurv** — `N/W (XE)` shows which tanks survive wins and at what energy
5. **Insights** — ECM, Solo carry, All-in, Fragile, Linchpin fire when thresholds are met
6. **Win combos** — who solos? `⚑TO-only` means all solo wins are timeout grinds
7. **First kills** — Primary target fires at 1.5× swarm avg; reveals targeting patterns

### Key thresholds

| Insight | Condition |
|---|---|
| ECM | `role == EcmSpecialist` |
| MVP | highest win-survival in swarm |
| Solo carry | soloWins ≥ 3 AND ≥ 20% of swarm wins |
| All-in | every survival is a win (zero loss-survivals) |
| Hider | `rate < 3.0` AND non-ECM |
| Expendable | `winRate < 33%` |
| Fragile | survived < 10% of matches (≥20 matches) |
| Linchpin | swarm win rate drops below 20% without this tank |
| Balanced | ≥3 co-carriers, leader <50% of swarm wins |

---

## 5. Known Baselines

From prior runs (200 matches, default arena, `--on-timeout energy`, seed 1000):

**RedGhost (ECM):** 60–80% win-survival, solo-carries 35–50% of Red's wins via timeout outlasting. All-in — every survival is a win. Jams + spoofs, combat rate near 0.

**BlueEcm (ECM):** 40–65% win-survival, solo wins 20–30%. Burnthrough mode cuts enemy jamming; switches to Jam for suppression.

**Blue tempo:** Decisive wins average 150–220t. BlueRush drives fast kills.  
**Red tempo:** Decisive wins average 350–550t. RedGhost grinds via ECM.

**ECM draw rate:** Without `--on-timeout energy`, ECM causes 25–30% draws. Always use `energy` policy for balance research.

**Small arena:** 350×250 boosts BlueRush combat rate, cuts BlueEcm survival — nerfs ECM hiding, buffs close-quarters.

---

## 6. Iterate

After analysis:
- **Hypothesis confirmed** → document in memory, adjust tank params if overcorrection needed
- **Hypothesis refuted** → refine the hypothesis, re-run with new parameters
- **New question raised** → start the loop again with the new hypothesis

For tank config changes (e.g., adjusting `MaxFirePower`, `PreferredRange`, `RetreatEnergyThreshold`), always record a before/after batch at the same seed so the delta is clean.
