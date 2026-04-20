# Blue Engineering — Research Reference

A hypothesis-driven experiment cycle for Blue swarm improvement.

## The Loop

```
Hypothesize → CLI run → Analyze → Update hypothesis → repeat
```

Goal: maintain and extend Blue win rate. Every iteration must have a measurable success criterion.

---

## 1. Hypothesize

State a concrete, falsifiable claim before running anything.

Bad: "Try making BlueSharp better."
Good: "BlueSharp's 89% WinSurv comes from its 300px preferred range keeping it out of close-quarters danger. Increasing that range to 350px will push WinSurv above 92% and increase Blue win rate by ≥3pp."

Success criteria must be measurable:
- Blue win rate (primary — must not drop below 55%)
- BlueSharp WinSurv (protect this — it drives wins)
- BlueGuard Rate/100t
- Solo carry / win combo breakdown

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
- Default (800×600): room to hide and maneuver; current results from here
- Cramped (350×250): buffs close-quarters, nerfs hiding strategies

Cross-seed validation: if a change shows improvement at seed 1000, verify at seed 2000 before treating it as confirmed.

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

1. **Win tally** — Blue % vs Red %
2. **Win quality** — `(N% via TO)` fires when >20% of wins are timeouts
3. **Decisive med** — how fast are decisive wins?
4. **Per-tank WinSurv** — `N/W` shows which tanks survive Blue wins
5. **Insights** — MVP, Solo carry, Linchpin, ECM, Hider, All-in, Expendable, Fragile
6. **Win combos** — which Blue tanks solo? which combos close out matches?
7. **First kills** — which Red tank does Blue kill first most often?

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

## 5. Blue Swarm — Current Baselines (New Architecture)

**Established iter-84. All old-arch data is invalid.**

**New-arch baseline** (seed 1000, 200 matches, MaxFP=1.5, PR=150):
- **Blue 66% / Red 34%** — dominant position

### Blue Tank Battle Intel

| Tank | Role | Rate/100t | WinSurv | Notes |
|------|------|-----------|---------|-------|
| BlueSharp | Support/MVP | ~high | 89% | Primary carry — survives almost every Blue win |
| BlueGuard | Defender/Attacker | 4.29 | unknown | Highest attack rate — drives kills |
| BlueEcm | ECM Specialist | 1.83 | unknown | Far below old-arch rate (8–12); contribution unclear |
| BlueRush | Attacker | unknown | unknown | New-arch role not yet characterized |
| BlueStrike | Attacker | unknown | unknown | New-arch role not yet characterized |

BlueSharp + BlueGuard appear to be the core engine of Blue wins. BlueEcm is present but not clearly contributing. Whether the ECM suppression is helping BlueSharp survive, or BlueSharp is winning on its own, is not yet known.

### Enemy Battle Intel (from CLI output only)

Observed from battle reports — **not derived from Red source code**:

| Red Tank | Observed behavior |
|----------|-----------------|
| RedGhost | Rate 0.18 — nearly silent, NOT MVP, NOT Linchpin. Their former carry is broken. |
| RedArrow | Role in new arch uncharacterized |
| RedBlade | Role in new arch uncharacterized |
| RedHammer | Role in new arch uncharacterized |

Red's ECM specialist is nearly silent. Their old strategy (Ghost ECM carry, timeout grinding) has collapsed. Watch for Red to fix this — when Ghost starts firing again, our win rate will come under pressure.

---

## 6. Iterate

After each run:
- **Confirmed** → commit, document, push to next hypothesis
- **Refuted** → revert, form tighter hypothesis, try again
- **Inconclusive** → run at second seed (2000) before deciding

If Blue win rate drops below 55% at any point: immediate Refuted. Do not accept regression.

Always compare at the same seed. Always rebuild both DLLs before a comparison run.
