# Iter 65 — Leader-change triggers immediate epoch (REFUTED, −2.2pp)

## Hypothesis
When the prior leader (lowest-slot tank in AllyEntryMap) dies, the successor waits up to ~70 ticks before broadcasting a new strategy:
- Up to `AllyStaleTicks` (30) for the prior leader to be pruned from AllyEntryMap, plus
- Up to `LeadershipEpochTicks` (40) for the next `tick % 40 == 0` epoch boundary.

During this gap, all tanks either hold the last broadcast (now-stale) strategy or fall through to Scout. Triggering an epoch immediately upon becoming leader should shrink the "uncoordinated window" from up to ~70 ticks to ~30 ticks.

**Falsifiable claim:** Blue 5-seed average ≥ 49.3% (maintain). Target ≥ 50.3%.
**Refute trigger:** Any seed regressing ≥ 3pp OR average below 47.3%.

## Code Change
`TankSwarmCode.AiCortex.Blue/AiCortexes/Library/BlueCortexBase.cs`:

Added a `_prevLeader` field. Epoch-trigger condition changed from
```csharp
(tick % 40 == 0 || _swarm.StrategyEpoch < 0)
```
to
```csharp
(tick % 40 == 0 || _swarm.StrategyEpoch < 0 || _prevLeader != ctx.Name)
```
`_prevLeader = leader` set at end of `OnTick`.

On the first tick a tank becomes leader (either because it's the initial leader or because all prior-slot tanks died), it runs `RunEpochLogic` immediately.

## Run Parameters
- 29v29. 5 seeds × 200 matches each. `--parallel 8 --on-timeout energy`.
- Red DLL built from current HEAD (post-Red-iter-89, post-Red-iter-90-revert).
- Blue DLL built from iter-64 code (Scout gun-tracks-radar) + this change.

## Raw Results

| Seed | Baseline (iter-64) | iter-65 | Δ |
|------|--------------------|---------|---|
| 1000 | 50.0% | 46.5% | **−3.5pp** |
| 2000 | 48.0% | 45.0% | **−3.0pp** |
| 3000 | 48.5% | 50.0% | +1.5pp |
| 4000 | 48.0% | 44.5% | **−3.5pp** |
| 5000 | 52.0% | 49.5% | −2.5pp |
| **avg** | **49.3%** | **47.1%** | **−2.2pp** |

## Analysis
Three of five seeds regressed ≥ 3pp, triggering the refute criterion on multiple axes. Only seed 3000 improved.

**Mechanism of regression (hypothesized):**
1. **Mid-engagement strategy churn.** When Strike (slot 0) dies during an active Wolfpack engagement, Guard (slot 1) takes over immediately and re-picks a priority target from *its own* current RadarMap. Guard's view may differ from Strike's last broadcast — triggering a sudden, unsynchronized target swap for all tanks that receive the new broadcast.
2. **Out-of-phase volley coordination.** `RunEpochLogic` also schedules volleys (`fireAt = tick + 20`). Running epoch off-cycle can cause a volley scheduled mid-engagement to land at an unnatural time, with surviving tanks at varied distances/angles → weaker synchronized alpha-strike.
3. **Echoes prior DO-NOT.** LoopState has "DO NOT reduce LeadershipEpochTicks below 40" and "35 gave -2.3pp". The leader-change trigger functionally lowers the effective cadence whenever Strike dies. Same class of effect: coordination churn > coordination freshness.

Interestingly seed 3000 (+1.5pp) was the one exception. Seed 3000's existing baseline was 48.5% — possibly a seed where Strike dies reliably early and fresh coordination helps more than it hurts. But the three bad seeds dominate the average.

## Key Findings
- **DO NOT trigger epoch on leader-change.** −2.2pp average; seeds 1000/2000/4000 all −3pp or worse.
- Confirms a broader pattern: off-cycle epoch triggering causes strategy/volley churn that hurts more than the "stale strategy" window it closes.
- `LeadershipEpochTicks = 40` with no off-cycle exceptions remains the calibrated optimum.

## Summary
Leader-change immediate epoch was expected to reduce ~40 ticks of stale coordination after leader death. Instead, it caused −3.5pp on seeds 1000/4000, −3pp on seed 2000, −2.5pp on seed 5000 — only seed 3000 improved. 5-seed avg dropped from 49.3% to 47.1% (−2.2pp). REFUTED and reverted. Add to DO-NOT list: "off-cycle epoch triggers cause coordination churn".
