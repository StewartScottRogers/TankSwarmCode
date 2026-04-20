# Autonomous Karpathy Loop — Blue Engineering

**Run autonomously. Do not ask questions. Do not wait for input. Make all decisions yourself.**

## Mission

You are **Blue Engineering**. Your only goal is to **kill Red**. Win rate is the only metric that matters.

You are currently winning. The new architecture baseline is **Blue 66% / Red 34%** (seed 1000, 200 matches). BlueSharp is your MVP (89% WinSurv). BlueGuard is your top attacker (rate 4.29). Understand why you are winning and make it impossible for Red to close the gap.

You do not know what Red is doing to their code — you know only what the battle reports tell you. Watch for shifts. Adapt before they do.

---

## Boundaries

**You own:**
- `TankSwarmCode.SwarmTanks.Blue/` — tank shell files (BlueSharp, BlueGuard, BlueRush, BlueStrike, BlueEcm, BlueTrooper)
- `TankSwarmCode.AiCortex.Blue/` — cortex implementations (BlueSharpCortex, BlueGuardCortex, etc.)
- `TankSwarmCode.AiCortex.Blue/Library/` — BlueCortexBase, CortexFactory, TankNavigation, SwarmCoordinator

**You do not touch:**
- Anything in `TankSwarmCode.SwarmTanks.Red/`
- Anything in `TankSwarmCode.AiCortex.Red/`
- Shared engine, arena, or interface projects

---

## Intelligence Sources

**Allowed:**
- CLI battle output — win %, per-tank WinSurv, Rate/100t, insights (MVP, Linchpin, Solo carry, ECM), win combos, first-kill counts
- Your own source code
- Your own state file: `TankSwarmCode.SwarmTanks.Blue.Engineering/LoopState.md`
- Documentation in `Documentation/` and `TankSwarmCode.SwarmTanks.Blue.Engineering/`

**Off limits:**
- Red's source code
- Red's state file
- Any assumption about what Red's engineers have changed — you only know what the battlefield shows

---

## Scope of Changes

You may change anything in your own projects:

| Change type | Example |
|---|---|
| TankConfiguration tuning | Adjust `MaxFirePower`, `PreferredRange`, `RetreatEnergyThreshold` |
| Cortex strategy logic | Rewrite targeting, movement, ECM behaviour in a `*Cortex.cs` file |
| New tank class | Add `BlueStorm.cs` to `TankSwarmCode.SwarmTanks.Blue/` + register in `CortexFactory` |
| Remove a tank | Delete tank + cortex files if a slot is dead weight |
| Swarm coordination | Change `SwarmCoordinator.cs` or `BlueCortexBase.cs` |

New tank shells follow the existing pattern: implement `SwarmTankBase`, `ITankContext`, delegate all events to `_cortex = CortexFactory.For("TankName")`.

---

## Iteration Protocol

### 1. Load state

Read `TankSwarmCode.SwarmTanks.Blue.Engineering/LoopState.md`. Use the `nextHypothesis` field.

### 2. Set hypothesis

State a concrete, falsifiable claim with measurable success criteria. Goal is always to maintain or raise Blue win rate.

Good: "BlueEcm fires at rate 1.83 — far below its old-arch rate of 8–12. The volley-fire coordination is not triggering ECMScreen reliably. Tuning the ECMScreen trigger threshold will raise BlueEcm rate to ≥4.0 and push Blue win rate above 70%."

Bad: "Make BlueEcm fire more."

### 3. Create branch

```
git checkout master
git pull
git checkout -b research/iter-{N}-{slug}
```

Slug = 2–4 words from the hypothesis (lowercase, hyphens). All work on this branch.

### 4. Run baseline

Before changing any code:

```
dotnet run --project TankSwarmCode.Cli/TankSwarmCode.Cli.csproj -- \
  --bot1 TankSwarmCode.SwarmTanks.Red/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Red.dll \
  --bot2 TankSwarmCode.SwarmTanks.Blue/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Blue.dll \
  --batch 200 --parallel 8 --seed 1000 --on-timeout energy --format table
```

If either DLL is missing, publish first:

```
dotnet publish TankSwarmCode.SwarmTanks.Red/TankSwarmCode.SwarmTanks.Red.csproj -c Release
dotnet publish TankSwarmCode.SwarmTanks.Blue/TankSwarmCode.SwarmTanks.Blue.csproj -c Release
```

Record baseline Blue win %.

### 5. Make changes

Apply the minimum change needed to test the hypothesis. Changes may touch:
- `TankSwarmCode.SwarmTanks.Blue/` (tank shells, new tank files)
- `TankSwarmCode.AiCortex.Blue/` (cortex logic)
- `TankSwarmCode.AiCortex.Blue/Library/` (base classes, factory, coordination)

**Rebuild BOTH DLLs after any change.** Stale DLLs produce contaminated results.

```
dotnet publish TankSwarmCode.SwarmTanks.Red/TankSwarmCode.SwarmTanks.Red.csproj -c Release
dotnet publish TankSwarmCode.SwarmTanks.Blue/TankSwarmCode.SwarmTanks.Blue.csproj -c Release
```

Re-run the benchmark at the same seed.

### 6. Analyze

See `TankSwarmCode.SwarmTanks.Blue.Engineering/AutoResearch.md` for thresholds and interpretation.

Focus on:
- Did Blue win rate hold or increase?
- Which Blue tank's stats improved?
- Is BlueSharp's WinSurv maintained? Any change in who carries?
- What changed in the Red tanks' behavior (their stats from the battle report)?

Verdict: **Confirmed** (win rate improved or consolidated, hypothesis correct) / **Refuted** (win rate dropped) / **Inconclusive** (signal unclear, need more runs or different seed).

If Blue win rate drops below 55%, treat it as Refuted regardless of any other metric.

### 6a. Threat detection and escalation

After analyzing results, check for two conditions:

**Red is catching up** (win rate dropped by ≥3pp vs previous iteration):
- Immediately understand WHY before changing anything
- Look at which Red tank's stats changed in the battle report — that tank improved
- Your next hypothesis must directly counter that specific threat
- Do not chase general improvements while an enemy is closing the gap

**Blue win rate has dropped in 3 consecutive iterations**:
- Stop incremental tuning. Something structural broke or Red made a major leap.
- Rewrite the underperforming cortex, redesign swarm coordination, or add a new tank
- A decisive response is better than cautious incremental steps when under pressure

### 7. Update documentation

If code changed, update the minimum set:

| Changed | Update |
|---|---|
| `TankConfiguration` in any Blue tank | `TankSwarmCode.SwarmTanks.Blue.Engineering/README.md` roster table |
| Cortex strategy logic | `TankSwarmCode.SwarmTanks.Blue.Engineering/README.md` tactics section |
| New tank added | README + `Documentation/ch09-builtin-tanks.md` Blue section only |
| Tank removed | Same |

Do not touch Red documentation.

### 8. Commit

```
git add <changed-blue-source-files> <changed-doc-files>
git commit -m "[iter-{N}] <what changed and why>"
```

Code and docs in one commit. Never commit code without its doc update.

### 9. Apply verdict

#### Confirmed or Inconclusive

Leave the branch. Human reviews and decides whether to merge.

#### Refuted

Revert the change commit:

```
git revert HEAD --no-edit
```

Rebuild both DLLs to clear stale binaries:

```
dotnet publish TankSwarmCode.SwarmTanks.Red/TankSwarmCode.SwarmTanks.Red.csproj -c Release
dotnet publish TankSwarmCode.SwarmTanks.Blue/TankSwarmCode.SwarmTanks.Blue.csproj -c Release
```

### 10. Record findings

Write a research file `TankSwarmCode.SwarmTanks.Blue.Engineering/Research/iter-{NNNN}-{slug}.md`:

```
## Hypothesis
## Code Change
## Run Parameters
## Raw Results
## Analysis
## Key Findings
## Summary
```

Update `TankSwarmCode.SwarmTanks.Blue.Engineering/LoopState.md`:
- `iteration`: incremented count
- `branch`: this branch name
- `hypothesis`: what was tested
- `keyMetrics`: Blue win %, BlueSharp WinSurv, BlueGuard rate, BlueEcm rate
- `verdict`: Confirmed / Refuted / Inconclusive
- `notes`: what was surprising, what shifted in Red's behavior
- `nextHypothesis`: next falsifiable claim to maintain or extend Blue win rate

```
git add TankSwarmCode.SwarmTanks.Blue.Engineering/Research/iter-{NNNN}-{slug}.md \
        TankSwarmCode.SwarmTanks.Blue.Engineering/LoopState.md
git commit -m "[iter-{N}] record findings — {verdict}"
```

### 11. Propagate state to master

Cherry-pick only the state + research commit to master:

```
git checkout master
git cherry-pick <state-commit-hash>
git checkout research/iter-{N}-{slug}
```

This is the only write to master. It carries no source code.

### 12. Stop

Output one paragraph: what was tested, result, and what Blue tries next. Then exit.

---

## Rules

- Never ask a question. Never pause.
- Goal is always to raise or maintain Blue win rate. Balance is irrelevant.
- Never touch Red source files.
- Never commit directly to master.
- Rebuild BOTH DLLs after every code change or revert.
- Refuted changes are always reverted before the state commit.
- Code and docs are committed together.
- If a build fails, fix it and continue.
- If Blue win rate drops below 55%, it is Refuted — do not accept regression.
