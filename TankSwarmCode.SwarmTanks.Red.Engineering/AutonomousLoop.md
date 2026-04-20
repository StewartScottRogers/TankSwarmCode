# Autonomous Karpathy Loop — Red Engineering Research Iteration

**Run autonomously. Do not ask questions. Do not wait for input. Make all decisions yourself.**

## Context

This is one iteration of the Karpathy Loop for Red Engineering research. Each iteration follows the cycle: Hypothesize → Run CLI → Analyze → Record → Next.

You are running from the **repo root** (the parent of `TankSwarmCode.SwarmTanks.Red.Engineering`).

Read `TankSwarmCode.SwarmTanks.Red.Engineering/AutoResearch.md` for the research framework, thresholds, baselines, and CLI parameter reference.

Read `AutonomousLoopState.md` for the current iteration count and next hypothesis.

## Focus

Red Engineering owns the Red swarm. Research goals, in priority order:

1. **Validate balance holds** — confirm the 51%/49% fix (BlueEcm MaxFirePower=1.0, iter-11) generalises across seeds.
2. **Improve Red combat tanks** — RedArrow and RedBlade are parameter-insensitive to the changes tested so far; find a lever that moves their win contribution.
3. **Protect Ghost's hider role** — Ghost (MaxFirePower=0.1, RetreatEnergyThreshold=40) is Red's structural advantage. Do not weaken it. Test only additive improvements to other tanks.

## Red Swarm Profile (from research log)

| Tank | Role | Key stat | Notes |
|------|------|----------|-------|
| RedGhost | ECM, MVP, Hider | WinSurv 54%, solo carry 32% | MaxFirePower=0.1 — passive hider, structural carry |
| RedArrow | Combat | First kills 35–36 | Retreat threshold (20→35) had zero effect; first-blood is positional |
| RedBlade | Combat | Combo with Ghost | Blade+Ghost combo ~16% of Red wins in cramped arena |
| RedStrike | Combat | — | Contributes to early pressure; not individually analysed |

## Suggested Next Hypotheses (pick the first untested one)

1. **Cross-seed validation:** The 51%/49% balance (seed 1000, 200 matches) holds at seed 2000. Success: Red 45–55% at seed 2000.
2. **Arrow PreferredRange sweep:** Reducing RedArrow's `PreferredRange` by 20% increases first kills from 35 to ≥42 and raises Red win rate by ≥2pp. Success: first-kill count increases, win rate shifts in Red's favour.
3. **Blade combat buff:** Increasing RedBlade's `MaxFirePower` or reducing its `RetreatEnergyThreshold` raises Blade solo carry from its current low baseline to ≥15% of Red wins without harming Ghost's carry. Success: Blade appears in Solo carry insights, Red win rate stays ≥48%.

---

## Iteration Protocol

### 1. Load prior state

Read `AutonomousLoopState.md` on `master`. Use the `nextHypothesis` field. If none, use hypothesis #1 from the list above.

### 2. Set hypothesis

State the concrete, falsifiable claim with measurable success criteria before touching any code.

### 3. Create iteration branch

```
git checkout master
git pull
git checkout -b research/iter-{N}-{slug}
```

All work from this point happens on this branch. Never commit directly to `master`.

### 4. Run baseline benchmark

Before changing any code, capture the current baseline:

```
dotnet run --project TankSwarmCode.Cli/TankSwarmCode.Cli.csproj -- \
  --bot1 TankSwarmCode.SwarmTanks.Red/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Red.dll \
  --bot2 TankSwarmCode.SwarmTanks.Blue/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Blue.dll \
  --batch 200 --parallel 8 --seed 1000 --on-timeout energy --format table
```

If either DLL is missing, publish it first:

```
dotnet publish TankSwarmCode.SwarmTanks.Red/TankSwarmCode.SwarmTanks.Red.csproj -c Release
dotnet publish TankSwarmCode.SwarmTanks.Blue/TankSwarmCode.SwarmTanks.Blue.csproj -c Release
```

### 5. Make code changes (if the hypothesis requires them)

If the hypothesis is purely observational (e.g. cross-seed validation), skip to step 6.

If the hypothesis requires a code change:
- Make the minimal change to the relevant `TankConfiguration` value.
- **Rebuild BOTH DLLs after any change.** Stale DLLs have contaminated runs before (iter-7, iter-9).
- Re-run the benchmark.

### 6. Analyze

Evaluate against the hypothesis using the thresholds in `TankSwarmCode.SwarmTanks.Red.Engineering/AutoResearch.md`.

Verdict: **Confirmed** / **Refuted** / **Inconclusive**.

Also check:
- Did Ghost's WinSurv or solo carry drop? If so, the change harmed Red's structural carry — Refuted regardless of win rate.
- Did BlueEcm's MaxFirePower remain at 1.0? If not, note the contamination.

### 7. Update documentation

If code changed, update the minimum set of docs:

| Code area changed | Documentation to update |
|---|---|
| `TankConfiguration` values in any Red tank | `Documentation/ch09-builtin-tanks.md` roster table + tank section; `TankSwarmCode.SwarmTanks.Red.Engineering/README.md` |
| New tank class added or removed | `ch09`, `ch01`, `ch02`, Red Engineering README |
| `SwarmTankCortexCradleBase` strategy logic | `ch09-builtin-tanks.md` Epoch Strategies table; Red Engineering README |

Do not update docs for code that did not change.

### 8. Commit code + docs

```
git add <changed-source-files> <changed-doc-files>
git commit -m "[iter-{N}] <what changed and why>"
```

### 9. Apply verdict

#### If Confirmed or Inconclusive

Leave the branch as-is. Human reviews and decides whether to merge to `master`.

#### If Refuted

Revert the code + docs commit:

```
git revert HEAD --no-edit
```

The revert preserves the audit trail. Human can discard the branch afterward.

### 10. Record findings

Update `AutonomousLoopState.md` **on the iteration branch**:

- `iteration`: incremented count
- `branch`: branch name
- `hypothesis`: what was tested
- `keyMetrics`: Red win %, Ghost WinSurv, Ghost solo carry, affected tank stats
- `verdict`: Confirmed / Refuted / Inconclusive
- `notes`: surprises, Ghost impact, anything non-obvious
- `nextHypothesis`: next concrete, falsifiable claim

```
git add AutonomousLoopState.md
git commit -m "[iter-{N}] record findings — {verdict}"
```

### 11. Propagate state to master

Cherry-pick only the `AutonomousLoopState.md` commit to `master`:

```
git checkout master
git cherry-pick <state-commit-hash>
git checkout research/iter-{N}-{slug}
```

This is the **only** write the loop makes directly to `master`.

### 12. Stop

Output a one-paragraph summary: what was tested, what the result was, and what the next hypothesis is. Then exit.

---

## Rules

- Never ask the user a question. Never pause for confirmation.
- If you hit a build failure or missing file, fix it and continue.
- Each iteration must produce a concrete verdict.
- **Never commit directly to `master`.** All experimental work goes on the iteration branch.
- **Rebuild BOTH DLLs** after any code revert or change.
- **Ghost is sacrosanct.** If a change drops Ghost's WinSurv below 50% or solo carry below 25%, treat it as Refuted — the structural carry is the foundation of Red's strategy.
- **Documentation and code are committed together.** Never commit a behaviour change without the corresponding doc update.
- **Refuted changes are always reverted** before the state commit.
