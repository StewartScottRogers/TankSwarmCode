# iter-0016 — 5th Tank (5v5 Parity) vs Improved Blue

## Context

LoopState iteration counter was 16. The LoopState stated a 66% baseline at seed 1000 from iter-14,
but ALL measurements in this session initially used that stale baseline for comparison. Blue had
since improved their DLL significantly (see below), making the true current baseline approximately
50.2% (3-seed avg).

---

## Phase 1 — Discovery: LoopState Baseline Is Stale

Running the "unchanged" baseline config (4 tanks, orbit at PR=160) against the CURRENT Blue DLL
showed markedly lower win rates than the LoopState claimed:

| Seed | LoopState (old Blue) | Actual (new Blue) |
|------|----------------------|-------------------|
| 1000 | 66.0% | 50.5% |
| 2000 | 67.0% | 55.5% |
| 3000 | 57.5% | 44.5% |
| **Avg** | **64.2%** | **~50.2%** |

**Cause:** Blue's recent commits changed `TankSwarmCode.AiCortex.Blue/Library/SwarmCoordinator.cs`
with two significant improvements:
1. Wolfpack angle-offset formation (+3.8pp from Blue's perspective, commit `871c931`)
2. Angle step changed from 72° to 60° (Blue's 5-tank orbit now uses 60° steps instead of equal
   72° spacing) (commit `00f7f08`)

Additionally, an earlier commit raised BlueGuard MaxFP=3.0 (iter-2 for Blue).

Blue's improvements turned what was a 64% Red win rate into a ~50% equilibrium.

---

## Phase 2 — Tests Against New Blue (Session Results)

All tests at seed 1000, 200 matches serial, true new baseline ≈ 50.5%.

| Hypothesis | Result | Δ vs new baseline |
|------------|--------|-------------------|
| Isolated target (max min-dist to Blue ally) | 51.5% | +1pp (noise) |
| Kill-reactive epoch (trigger on target loss) | 49.0% | -1.5pp |
| **5th tank (RedTrooper, 1st test)** | **60.5%** | **+10pp** |
| Orbit drift 0.2°/tick | 57.5% | +7pp |
| All tanks MaxFP=1.5 | 59.0% | +8.5pp |
| LeadershipEpochTicks=20 | 54.0% | +3.5pp |
| Highest-energy targeting | 48.5% | -2pp |
| Gun threshold 3° (tighter) | 52.0% | +1.5pp |
| Fast orbit drift 1°/tick | 50.5% | neutral |

**Key insight:** The 5th tank was the only change with a clear, large effect. All others are
noise or marginal. The true winner is structural parity: 5v5 instead of 4v5.

---

## Phase 3 — 5th Tank Breakthrough: 5v5 Parity

**Root Cause of Baseline Regression:** Blue's 5 tanks now orbit more effectively (60° steps =
denser coverage in a 240° arc). Against our 4 Red tanks, Blue could concentrate 5-on-1 effectively.
With 5 Red tanks (equal numbers), both sides orbit at similar parity and Red's coordination wins.

**Code Change:**
- `RedTrooper.cs`: Added parameterless constructor `public RedTrooper() : this(4) { }` so
  the arena engine instantiates it automatically.
- `RedTrooperCortex.cs`: Updated to `MaxFP=2.0, PreferredRange=160.0, RetreatEnergyThreshold=20.0`
  (matching the other tanks' optimal configuration from iter-14).

**Results (5-seed serial, 200 matches each):**

| Seed | 4-tank (new Blue) | 5-tank (new Blue) | Δ |
|------|-------------------|-------------------|---|
| 1000 | ~50.5% | **63.5%** | +13pp |
| 2000 | ~55.5% | **60.5%** | +5pp |
| 3000 | ~44.5% | **70.0%** | +25.5pp |
| 4000 | (unknown) | **62.5%** | — |
| 5000 | (unknown) | **58.0%** | — |
| **Avg** | **~50.2%** | **62.9%** | **+12.7pp** |

**400-match validation at seed 1000: 65.8% (263/400) — confirmed stable.**

**CONFIRMED BREAKTHROUGH: +12.7pp above new baseline**

---

## Final Configuration

| Tank | Slot | MaxFP | PR | HasEcm | Retreat |
|------|------|-------|----|--------|---------|
| Hammer | 0 | 2.0 | 160 | false | 25 |
| Blade | 1 | 2.0 | 160 | false | 20 |
| Arrow | 2 | 1.5 | 160 | false | 20 |
| Ghost | 3 | 2.0 | 160 | false | 20 |
| **Trooper** | **4** | **2.0** | **160** | **false** | **20** |

**Wolfpack orbit:** 5 tanks at 72° spacing (slot%aliveCount * 72°), radius = config.PreferredRange

---

## Key Findings

1. **Blue improved significantly.** Two commits to Blue's SwarmCoordinator brought them from
   losing 66% to approximately 50/50. Always re-baseline before claiming improvement.

2. **5v5 parity restores Red advantage.** When Blue had 5 well-coordinated tanks vs our 4,
   Blue's per-tank numerical advantage overcame our orbit coordination. Adding Trooper
   eliminates this structural disadvantage.

3. **Trooper was already in the project** (`TankSwarmCode.SwarmTanks.Red/`) — just needed
   a parameterless constructor to be auto-instantiated by the arena engine.

4. **The 64.2% "ceiling" was against old Blue, not a true ceiling.** Many of the changes
   tested this session were statistically neutral vs the new true baseline (~50%). The
   real ceiling against new Blue is now ~63% with 5 tanks.
