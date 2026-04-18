# Autonomous Loop State

## Last Updated
2026-04-18 — Iteration 7 complete

## Current Iteration
**8** — pending

## Next Hypothesis
Ghost's structural advantage comes from its passive/non-threatening hider role causing Blue to deprioritize it while real combatants fight. Replace Ghost with an Arrow clone (MaxFirePower=1.5, PreferredRange=220, RetreatEnergyThreshold=20, no ECM) to test: does Red's win rate increase (Ghost was costly), hold (~56%), or decrease (Ghost enables wins by drawing Blue attention)?

**Branch:** `research/iter-8-ghost-replace-arrow-clone`

---

## Iteration Log

### Iter 7 — `research/iter-7-arrow-retreat-nerf`
**Date:** 2026-04-18
**Status:** No Effect

**Hypothesis:** RedArrow RetreatEnergyThreshold 20→35 reduces Arrow's first-kill rate and drops Red's win rate.

**Run:** 200 matches, seed 1000, `--on-timeout energy`, default arena (800×600), Arrow retreat=35
(Note: initial accidental run used stale buffed-BlueEcm dll from iter-6 and returned 62% — discarded. Correct run used rebuilt baseline Blue dll.)

**Results:**
- Red 111 (56%) / Blue 89 (44%) — identical to baseline
- Arrow first kills: **36** (baseline 35-36 — unchanged)
- Arrow solos shifted: 10D+10TO (vs baseline 12D+6TO) — more timeout, same count
- Ghost WinSurv, solo carry: unchanged at 66/111, 35 solos

**No effect:** Arrow's retreat threshold governs late-match behavior; first kills happen at tick ~24 when Arrow still has 80+E — the threshold (35E) never fires at that point.
**Key finding:** 56% structural advantage is deep and parameter-insensitive. Seven parameter sweeps have all returned 56%. Arrow's first-blood advantage is positional/behavioral, not energy-threshold-based.
**Code:** Reverted — Arrow restored to RetreatEnergyThreshold=20.
**Process note:** Always rebuild both dlls after reverting code changes.

---

### Iter 6 — `research/iter-6-blueecm-combat-buff`
**Date:** 2026-04-18
**Status:** Refuted — combat buff backfired, Red climbed to 62%

**Hypothesis:** Buffing BlueEcm (MaxFirePower 1.5→2.5, RetreatEnergyThreshold 35→25) makes BlueEcm harder to kill and raises Blue's win rate.

**Run:** 200 matches, seed 1000, `--on-timeout energy`, default arena (800×600)

**Results:**
- Red 124 (62%) / Blue 76 (38%) — Red INCREASED from 56%
- BlueEcm WinSurv: 33/76 = **43%** (baseline ~60%, collapsed)
- BlueEcm loss-survivals: **17** (baseline 8–9, nearly doubled)
- RedGhost: **All-in** (0 loss-survivals, gained All-in status vs baseline's 5)
- Blue top solo carrier shifted to Guard (14x) from BlueEcm (9x)

**Refuted:** Combat buff caused BlueEcm to over-commit and die frequently. BlueEcm loses ECM energy to combat, can't sustain its survival-carry role.
**Key finding:** ECM tanks (both BlueEcm and Ghost) derive value from surviving, not fighting. Higher firepower = faster energy drain = more deaths in Red wins. Same lesson as Ghost's structural carry: ECM role tanks should NOT be combat-buffed.
**Code:** Reverted — BlueEcm restored to MaxFirePower=1.5, RetreatEnergyThreshold=35.

---

### Iter 5 — `research/iter-5-ghost-no-ecm`
**Date:** 2026-04-18
**Status:** Confirmed — Ghost's carry is structural, ECM is cosmetic

**Hypothesis:** HasEcm=false leaves Ghost's carry rate and WinSurv unchanged (25–35% carry, MVP), Red holds ~56%.

**Run:** 200 matches, seed 1000, `--on-timeout energy`, default arena (800×600), Ghost HasEcm=false

**Results:**
- Red 112 (56%) / Blue 88 (44%) — unchanged
- RedGhost WinSurv: 68/112 = **61%** (baseline 58% — INCREASED)
- RedGhost solo carry: 37/112 = **33%** (baseline 30% — INCREASED)
- RedGhost Rate/100t: **2.08** (near 0 — unchanged)
- RedGhost loss-survivals: **5** (unchanged)
- BlueEcm: NEW **Linchpin** insight (alive: 87%, dead: 25%) — freed from Ghost's jam suppression

**Confirmed:** Ghost is a passive hider (MaxFirePower=0.1, RetreatEnergyThreshold=40). Blue ignores non-threatening Ghost while fighting real Red combatants and eliminating itself. ECM hardware contributed nothing — removing it improved Ghost's metrics slightly.
**Key finding:** Red's 56% is compositional (not ECM). BlueEcm is Blue's Linchpin — and Ghost's ECM was masking this by suppressing BlueEcm's burnthrough.
**Code:** Reverted — observational test only. Ghost restored to HasEcm=true.

---

### Iter 4 — `research/iter-4-jam-drop-nerf`
**Date:** 2026-04-18
**Status:** Refuted — jam parameters are not the binding constraint

**Hypothesis:** EcmJamDropChance 0.50→0.30, EcmJamCorruptChance 0.30→0.15 drops Ghost solo carry to ≤20% and Red win rate toward 45–52%.

**Run:** 200 matches, seed 1000, `--on-timeout energy`, default arena (800×600)

**Results:**
- Red 111 (56%) / Blue 89 (44%) — identical to baseline
- RedGhost WinSurv: 65/111 = **59%** (baseline 58%)
- RedGhost solo carry: 35/111 = **32%** (identical to iter-3)
- RedGhost Rate/100t: **2.13** (near 0 — unchanged)
- RedGhost loss-survivals: **6** (near-baseline 5)

**Refuted:** Zero effect. Three ECM parameter sweeps (cramped arena, spoof radius, jam chances) have all returned 56%/~32%. Balance is insensitive to ECM parameters.
**Key finding:** Ghost's carry is structural, not ECM-driven. Ghost solos decisively (71%) at near-zero combat rate — it outlasts by being passive and non-threatening (MaxFirePower=0.1, RetreatEnergyThreshold=40), not by misdirecting Blue via ECM. ECM appears cosmetic at the carry level.
**Code:** Reverted — jam constants restored to 0.50/0.30.

---

### Iter 3 — `research/iter-3-ghost-ecm-nerf`
**Date:** 2026-04-18
**Status:** Refuted — parameter is not the binding constraint

**Hypothesis:** Reducing EcmSpoofRadius by 25% (130.0 → 97.5) drops Red's win rate from 56% toward 45–52% and Ghost's solo carry from 30% to ≤20%.

**Run:** 200 matches, seed 1000, `--on-timeout energy`, default arena (800×600), EcmSpoofRadius=97.5

**Results:**
- Red 111 (56%) / Blue 89 (44%) — identical to baseline
- RedGhost WinSurv: 66/111 = **59%** (baseline 58%)
- RedGhost solo carry: 36/111 = **32%** (baseline 30% — slight increase)
- RedGhost Rate/100t: **2.24** (near 0 — unchanged)
- RedGhost loss-survivals: **5** (same as baseline)

**Refuted:** Zero effect in either direction. Win rate unchanged; solo carry slightly increased.
**Key finding:** Ghost's carry is via the Jam mechanism (scan drop/corrupt), not spoof radius. Ghost solos decisively (69% decisive), meaning Blue self-eliminates from scan corruption — not from chasing distant ghost echoes. The spoof geometry is irrelevant to Ghost's effectiveness.
**Code:** Reverted — EcmSpoofRadius restored to 130.0.

---

### Iter 2 — `research/iter-2-ghost-cramped-arena`
**Date:** 2026-04-18
**Status:** Inconclusive — direction confirmed, magnitude missed

**Hypothesis:** Cramped arena (350×250) reduces RedGhost solo carry rate by ≥50% (30% → ≤15%) by limiting ECM spoofing effectiveness.

**Run:** 200 matches, seed 1000, `--on-timeout energy`, cramped arena (350×250)

**Results:**
- Red 69 (34%) / Blue 131 (66%)
- RedGhost WinSurv: 33/69 = **48%**
- RedGhost solo carry: 12/69 = **17%** (reduction 43%, threshold ≥50%)
- RedGhost Rate/100t: **2.41** (near 0 — confirmed)
- RedGhost All-in: **0 loss-survivals** (regained All-in status in cramped arena)
- RedGhost Linchpin: **alive → 100% Red wins; dead → 22% Red wins**

**Inconclusive:** Direction confirmed (solo carry fell 30%→17%), threshold not met (≥50% reduction, ≤15%)
**Surprises:**
- Ghost became Linchpin in cramped arena — more critical, not less
- Red collapsed from 56% → 34%; Blue's blitz (76t median decisive) overwhelms Red's grind strategy in small map
- Blade+Ghost combo at 16% of Red wins — Ghost enables Blade in cramped quarters rather than soloing
- Ghost regained All-in status (5 loss-survivals in default → 0 in cramped)

**BlueEcm note:** BlueEcm still MVP at 74/131 = 57% WinSurv, now also a combat unit in cramped (Rate/100t = 23.85).

---

### Iter 1 — `research/iter-1-redghost-solo-carry`
**Date:** 2026-04-18
**Status:** Partially Confirmed — needs human merge decision

**Hypothesis:** RedGhost carries Red wins via ECM outlasting (timeout), not combat — solo carry rate ≥35% of Red wins, combat rate near 0, All-in role, WinSurv 60–80%.

**Run:** 200 matches, seed 1000, `--on-timeout energy`, default arena (800×600)

**Results:**
- Red 112 (56%) / Blue 88 (44%)
- RedGhost WinSurv: 65/112 = **58%** (baseline 60–80%, just below)
- RedGhost solo carry: 34/112 = **30%** (threshold ≥35% — not met)
- RedGhost Rate/100t: **2.16** (near 0 — confirmed)
- RedGhost solo breakdown: **25 decisive + 9 timeout** (74% decisive)
- RedGhost loss-survivals: **5** (not All-in)

**Confirmed:** MVP role, ECM role, near-zero combat rate
**Refuted:** "Via timeout outlasting" claim; "All-in" claim from prior baseline
**New finding:** Ghost wins decisive solos at near-zero damage → Blue self-eliminates via ECM spoofing misdirection

**Baseline corrections:**
- Prior baseline claimed solo carry 35–50% via timeout → actual is 30% with majority decisive
- Prior baseline claimed All-in → actual has 5 loss-survivals

**BlueEcm note:** BlueEcm is Blue's MVP (53/88 WinSurv = 60%), using burnthrough + jam. Blue is 8% below parity (44%).
