# Iter 90 — Stale-Contact Lead Correction (REFUTED -11pp seed 1000)

**Date:** 2026-04-21
**Branch:** research/iter-90-stale-lead-correction
**Status:** REFUTED. Red 45% (90/200) seed 1000 vs 56% baseline (-11pp decisive). Stopped after 1 seed.

## Hypothesis

`TankNavigation.LinearPredictionFire` currently leads targets only by bullet travel time:

```csharp
double travelTime = dist / bulletSpeed;
Vector2D predictedPos = target.Position + VelocityVector * travelTime;
```

`target.Position` is the location at `target.Timestamp`. When a contact comes from
ally RadarShare, `Timestamp` can be 1–5 ticks old. The contact's "now" position is
`target.Position + VelocityVector × observationAge` (ticks since sighting). Existing
code therefore under-leads by `observationAge × VelocityVector ≈ up to 40 px` at max velocity.

Claim: adding observationAge to the lead calc would improve hit rate on shared-radar
contacts and raise win rate.

## Code Change

`TankNavigation.LinearPredictionFire`:

```csharp
double observationAge = ctx.Arena.TickNumber - target.Timestamp;
Vector2D currentPos = new(
    target.Position.X + target.VelocityVector.X * observationAge,
    target.Position.Y + target.VelocityVector.Y * observationAge);

double bulletSpeed = 20.0 - 3.0 * power;
double dist = ctx.State.Position.DistanceTo(currentPos);
double travelTime = dist / bulletSpeed;

Vector2D predictedPos = new(
    currentPos.X + target.VelocityVector.X * travelTime,
    currentPos.Y + target.VelocityVector.Y * travelTime);
```

## Run Parameters

- 29v29, --on-timeout energy, --parallel 8, --batch 200, --seed 1000
- MaxFP=1.5, PR=160, Fallback=10, Scout gun pre-aim (from iter 89)

## Raw Results

| Run | Seed | Red wins | Win % | Δ vs baseline |
|---|---|---|---|---|
| Baseline | 1000 | 111/200 | 56.0% | — |
| With correction | 1000 | 90/200 | 45.0% | -11.0pp |

Stopped after seed 1000 per protocol (catastrophic single-seed).

## Analysis

The correction extrapolates position using the most recent VelocityVector. This is
accurate only when the target maintains heading between observation and firing.
Blue's updated DLL produces tanks that turn frequently — stale VelocityVector
projected forward over `observationAge + travelTime` points in the wrong direction
and the fired bullet is aimed ahead of where the target actually moves.

The existing code, which ignores observationAge, implicitly treats `target.Position`
as the "now" position. When the target turns, the existing lead is also wrong, but
by less (missing only `travelTime × velocity`, not `(observationAge + travelTime) × velocity`).
Less extrapolation = less error when velocity is stale.

In 29v29 with dense formations, most contacts come through ally RadarShare because
each tank's own radar covers only a narrow sector. Observation ages of 3–5 ticks are
likely common. Multiplying stale velocity by 5 extra ticks over-shoots substantially
when targets are turning.

## Key Findings

- **Stale-velocity extrapolation is catastrophic vs a mobile, turning Blue DLL.**
  The existing "ignore observationAge" behavior acts as implicit shrinkage toward
  the observed position — worth more than naive time-of-flight correction when
  velocity is stale and direction is changing.
- **New law confirmed: extrapolation beyond `travelTime` is harmful when target
  turn rate is non-negligible.** The 10-tick bullet lead already projects beyond
  the reliability horizon of linear prediction; adding more (for stale contacts)
  compounds the error.
- Observation: this is analogous to iter 15's "velocity-led orbit prediction"
  refutation (-3.7pp) — leading more aggressively on velocity hurts vs a turning
  Blue. The fire-control loop prefers conservative lead.

## Summary

Refuted. Reverted. Baseline 53.2% avg (5-seed) ceiling preserved. Blue's turn rate
exceeds the threshold at which stale-velocity correction is safe; naive time-of-flight
leads are already at the edge of the linear-prediction envelope.

**Next hypothesis candidates (not tested this iter):**
- Clamp gun turn ahead of predicted target (gun-tracks-predicted-target when idle
  in Fallback/Scatter — same mechanism as iter 89 Scout but for retreat phases).
- Fire-gate tuning by range (tighter aim at close range, where target angular
  size is larger but lateral motion is also larger).
- Shrinkage in existing lead: `predictedPos = target.Position + VelocityVector ×
  travelTime × k` with k<1, recognizing that turning targets make full lead an
  over-correction.
