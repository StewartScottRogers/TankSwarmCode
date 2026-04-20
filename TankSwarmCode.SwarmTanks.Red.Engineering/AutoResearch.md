# Red Engineering — Research Reference

A hypothesis-driven experiment cycle for Red swarm improvement.

## The Loop

```
Hypothesize → CLI run → Analyze → Update hypothesis → repeat
```

Goal: raise Red win rate. Every iteration must have a measurable success criterion.

---

## 1. Hypothesize

State a concrete, falsifiable claim before running anything.

Bad: "Try making RedArrow faster."
Good: "Increasing RedArrow's MaxFirePower from 2.0 to 2.5 will raise its Rate/100t by ≥1.5 and increase Red win rate by ≥3pp without reducing Ghost's solo carry below 10%."

Success criteria must be measurable:
- Red win rate (primary)
- Per-tank WinSurv and Rate/100t
- Solo carry count and %
- Role assignments (MVP, Linchpin, ECM, Hider, etc.)

---

## 2. Design the Experiment

| Parameter | Research default | Notes |
|---|---|---|
| `--batch` | 200 | Stable percentages require ≥100 |
| `--parallel` | 8 | Matches available CPU cores |
| `--seed` | 1000 | Fixed for reproducibility |
| `--on-timeout` | `energy` | Eliminates draws; reveals true balance |
| `--format` | `table` | Summary stats auto-included |

Arena size affects ECM effectiveness:
- Default (800×600): room to hide and maneuver
- Cramped (350×250): buffs close-quarters, nerfs hiding strategies

Cross-seed validation: if a change shows +5pp at seed 1000, verify at seed 2000 before treating it as confirmed.

---

## 3. Run

```bash
dotnet run --project TankSwarmCode.Cli/TankSwarmCode.Cli.csproj -- \
  --bot1 TankSwarmCode.SwarmTanks.Red/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Red.dll \
  --bot2 TankSwarmCode.SwarmTanks.Blue/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Blue.dll \
  --batch 200 --parallel 8 --seed 1000 \
  --on-timeout energy \
  --format table
```

If DLLs are stale: `dotnet publish` both projects before running.

---

## 4. Analyze

Read the output bottom-up:

1. **Win tally** — Red % vs Blue %
2. **Win quality** — `(N% via TO)` fires when >20% of wins are timeouts
3. **Decisive med** — how fast are decisive wins?
4. **Per-tank WinSurv** — `N/W` shows which tanks survive Red wins
5. **Insights** — MVP, Solo carry, Linchpin, ECM, Hider, All-in, Expendable, Fragile
6. **Win combos** — which Red tanks solo? which combos close out matches?
7. **First kills** — which Blue tank does Red kill first most often?

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

## 5. Red Swarm — Current Baselines (New Architecture)

**Established iter-84. All old-arch data is invalid.**

**New-arch baseline** (seed 1000, 200 matches, MaxFP=1.5, PR=150):
- **Red 34% / Blue 66%** — we are losing badly

### Red Tank Battle Intel

| Tank | Role | Rate/100t | WinSurv | Notes |
|------|------|-----------|---------|-------|
| RedGhost | ECM | 0.18 | unknown | Nearly silent — cortex not using ECM; was solo carry in old arch |
| RedArrow | Scout | unknown | unknown | New-arch behavior not yet characterized |
| RedBlade | Attacker | unknown | unknown | New-arch behavior not yet characterized |
| RedHammer | Attacker | unknown | unknown | New-arch behavior not yet characterized |

RedGhost was the entire foundation of old Red wins (solo carry 32–48%, Linchpin). In new arch its rate dropped to 0.18 — the composition cortex is not activating its ECM. This is the most urgent problem to fix.

### Enemy Battle Intel (from CLI output only)

Observed from battle reports — **not derived from Blue source code**:

| Blue Tank | Observed behavior |
|-----------|------------------|
| BlueSharp | MVP in 89% of Blue wins (seed 1000). Primary threat — must be killed. |
| BlueGuard | Top attacker, rate 4.29. Driving Red kills. |
| BlueEcm | Rate 1.83, passive role in current battles |
| BlueRush / BlueStrike | Role unknown in new arch — not highlighted in current reports |

---

## 6. Iterate

After each run:
- **Confirmed** → commit, document, push to next hypothesis
- **Refuted** → revert, form tighter hypothesis, try again
- **Inconclusive** → run at second seed (2000) before deciding

Always compare at the same seed. Always rebuild both DLLs before a comparison run.
