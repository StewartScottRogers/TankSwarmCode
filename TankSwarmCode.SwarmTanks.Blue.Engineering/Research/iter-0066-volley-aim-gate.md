# Iter 66 — Volley Fire Gun-Alignment Gate (5°)

## Hypothesis
The volley-fire block in `BlueCortexBase.OnTick` fires via `ctx.SetFire(power)` without checking whether the gun is aimed at the target. This contrasts with `TankNavigation.LinearPredictionFire`, which gates fire with `Math.Abs(gunDiff) < 5.0`. Misaligned volley shots waste energy on bullets that can't hit. Adding the same 5° gate will filter those wasted shots and raise or at least maintain Blue win rate ≥ 49.3% (5-seed avg).

Falsifiable: if 5-seed avg drops below 49.3%, refuted.

## Code Change
`TankSwarmCode.AiCortex.Blue/AiCortexes/Library/BlueCortexBase.cs`: added predicted-position + gun-alignment gate before volley `SetFire`, mirroring LinearPredictionFire's logic:

```csharp
double bulletSpeed = 20.0 - 3.0 * power;
double travelTime = ctx.State.Position.DistanceTo(volleyTarget.Position) / bulletSpeed;
Vector2D predictedPos = new(
    volleyTarget.Position.X + volleyTarget.VelocityVector.X * travelTime,
    volleyTarget.Position.Y + volleyTarget.VelocityVector.Y * travelTime);
double desiredGunBearing = ctx.State.Position.BearingTo(predictedPos);
double gunDiff = (desiredGunBearing - ctx.State.GunHeading).RelativeBearing();
if (Math.Abs(gunDiff) < 5.0 && !TankNavigation.IsWallInLineOfFire(ctx, predictedPos))
    ctx.SetFire(power);
```

Also uses predicted (not last-known) position for wall-LOS check.

## Run Parameters
- 29 Blue vs 29 Red
- 200 matches per seed, 5 seeds (1000/2000/3000/4000/5000)
- `--parallel 8 --on-timeout energy`
- Both DLLs rebuilt Release post-change
- Red DLL at current HEAD state (post Red iter-89, iter-90 reverted)

## Raw Results

| Seed | Iter-64 Baseline | Iter-66 (gate on) | Δ |
|------|------------------|-------------------|---|
| 1000 | 50.0% | 50.5% (101/200) | +0.5 |
| 2000 | 48.0% | 47.5% (95/200)  | -0.5 |
| 3000 | 48.5% | 48.5% (97/200)  |  0.0 |
| 4000 | 48.0% | 48.0% (96/200)  |  0.0 |
| 5000 | 52.0% | 51.5% (103/200) | -0.5 |
| **Avg** | **49.3%** | **49.2%** | **-0.1** |

## Analysis
The 5-seed average moved -0.1pp, well inside single-seed noise (σ ≈ 1.6pp for a 49% proportion over 200 matches). No seed moved by more than ±0.5pp, and direction is split (1 up, 2 flat, 2 down). This is functionally a null result.

Interpretation: under normal swarm operation, the per-tick `LinearPredictionFire` in `ExecuteWolfpack` already keeps the gun aimed tightly on the priority target between volley scheduling and volley firing. By the time `_scheduledFireTick` triggers (20 ticks after the leader broadcasts), the gun is nearly always within the 5° window already — so the gate filters almost nothing, and the rare misaligned shots it skips (a) cost little energy and (b) would miss anyway, producing a wash.

The gate is theoretically sound but contributes no measurable win-rate signal at 1000 total matches.

## Key Findings
- Volley fire without the 5° gate is effectively already aligned in practice — per-tick LPF tracking makes the gate redundant at volley time.
- This is the 4th neutral firing-logic change (joins iters 3, 13, 24, 25 pattern). Firing logic is at a saturation point; the calibrated state and per-tick aim loop already extract near-maximum fire efficiency.
- No asymmetry discovered between gated vs ungated volley. Supports a broader hypothesis: improvements will have to come from *coordination / geometry / composition*, not fire-control micro-tuning.

## Summary
Added 5° gun-alignment gate to volley fire. 5-seed avg 49.2% vs 49.3% baseline (-0.1pp). Neutral / inconclusive, reverted to preserve clean baseline. Adds another entry to the "firing micro-changes are saturated" pattern. Next iteration should explore coordination- or composition-level levers rather than per-shot gates.
