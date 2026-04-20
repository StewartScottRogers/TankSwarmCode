# Autonomous Karpathy Loop — Research Iteration

**Run autonomously. Do not ask questions. Do not wait for input. Make all decisions yourself.**

## Context

This is one iteration of the Karpathy Loop for TankSwarm balance research. Each iteration follows the cycle: Hypothesize → Run CLI → Analyze → Record → Next.

Read `AutoResearch.md` for the full research framework, thresholds, baselines, and CLI parameter reference.

## Branch Strategy

Every iteration works on its own branch. **Only a human may merge to `master`.**

```
master  (human-only merges)
  ├── research/iter-1-ghost-solo-carry      ← Confirmed → human merges
  ├── research/iter-2-blueecm-firepower     ← Refuted   → code reverted; human discards
  └── research/iter-3-wolfpack-volley-range ← in progress
```

Branch names follow the pattern: `research/iter-{N}-{short-slug}` where the slug is 2–4 words from the hypothesis (lowercase, hyphens).

---

## Iteration Protocol

### 1. Load prior state

Read `AutonomousLoopState.md` on `master`. If it does not exist, this is iteration 1; create it on `master`.

### 2. Set hypothesis

Use the `nextHypothesis` from prior state. If no prior state, start with:
*"RedGhost solo-carries Red wins via ECM timeout grinding, not combat (expected: high win-survival, near-zero combat rate)."*

### 3. Create iteration branch

From `master`, create and switch to the iteration branch:
```
git checkout master
git pull
git checkout -b research/iter-{N}-{slug}
```
All work from this point happens on this branch. Never commit directly to `master`.

### 4. Run baseline benchmark (bench-only)

Before changing any code, run the benchmark to capture the current baseline for this iteration:
```
dotnet run --project TankSwarmCode.Cli/TankSwarmCode.Cli.csproj -- \
  --bot1 TankSwarmCode.SwarmTanks.Red/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Red.dll \
  --bot2 TankSwarmCode.SwarmTanks.Blue/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Blue.dll \
  --batch 100 --parallel 8 --seed 1000 --on-timeout energy --format table
```
If either DLL is missing, publish it first:
```
dotnet publish TankSwarmCode.SwarmTanks.Red/TankSwarmCode.SwarmTanks.Red.csproj -c Release
dotnet publish TankSwarmCode.SwarmTanks.Blue/TankSwarmCode.SwarmTanks.Blue.csproj -c Release
```

### 5. Make code changes (if the hypothesis requires them)

If the hypothesis is purely observational (no code change needed), skip to step 6.

If the hypothesis requires a code change (e.g. tuning a `TankConfiguration` value):
- Make the minimal change needed to test the hypothesis.
- Rebuild and re-run the benchmark.

### 6. Analyze

Evaluate results against the active hypothesis using the insight thresholds in `AutoResearch.md` (ECM, MVP, Solo carry, Hider, etc.).

Determine verdict: **Confirmed** / **Refuted** / **Inconclusive**.

### 7. Update documentation

If code was changed in step 5, update the affected documentation files now. Apply the minimum edits needed to keep docs accurate:

| Code area changed | Documentation to update |
|------------------|------------------------|
| `TankConfiguration` values in any tank | `ch09-builtin-tanks.md` roster table + tank section; Engineering READMEs |
| New tank class added or removed | `ch09`, `ch01` solution layout, `ch02` Add Tanks walkthrough, Engineering README |
| `ArenaConstants` values | `ch12-configuration.md`, `ch04-physics-engine.md`, `ch13-ecm-system.md`, `ch14-tank-energy.md` |
| `SwarmTankCortexCradleBase` strategy logic | `ch09-builtin-tanks.md` Epoch Strategies table; Blue/Red Engineering READMEs |
| CLI flags or output format | `ch15-cli.md`; Engineering READMEs |
| `ISwarmTank` / `IArenaContext` interface | `ch05-tank-ai-framework.md`, `ch08-data-models.md` |
| New `SwarmMessageType` enum value | `ch06-swarm-communication.md` message table |
| ECM mode behaviour | `ch13-ecm-system.md` |

Do not update documentation for code that was not changed. If no code changed, skip this step.

### 8. Commit code + docs to the branch

If code or documentation changed, commit them together in a single commit on the iteration branch:
```
git add <changed-source-files> <changed-doc-files>
git commit -m "[iter-{N}] <what changed and why>"
```
Never commit source changes without the corresponding documentation updates in the same commit.

### 9. Apply verdict — Confirmed or Refuted

#### If Confirmed or Inconclusive

Leave the branch as-is. The human will review and decide whether to merge to `master`.

#### If Refuted

Revert the code + documentation commit so the branch tip contains no harmful changes:
```
git revert HEAD --no-edit
```
This adds a revert commit. The branch history preserves what was tried and why it failed. The human can review the branch to understand what was attempted, and then discard it.

### 10. Record findings

Update `AutonomousLoopState.md` **on the iteration branch** with:
- `iteration`: incremented count
- `branch`: the branch name
- `hypothesis`: what was tested
- `keyMetrics`: win %, win-survival rates, combat rates for key tanks
- `verdict`: Confirmed / Refuted / Inconclusive
- `notes`: what was surprising or notable
- `nextHypothesis`: the next concrete, falsifiable claim to test

Commit the state file:
```
git add AutonomousLoopState.md
git commit -m "[iter-{N}] record findings — {verdict}"
```

### 11. Propagate state to master

Cherry-pick only the `AutonomousLoopState.md` commit back to `master` so the next iteration starts with current state:
```
git checkout master
git cherry-pick <state-commit-hash>
git checkout research/iter-{N}-{slug}
```
This is the **only** write the loop makes directly to `master`. It carries no code — only the research log.

### 12. Stop

Output a one-paragraph summary of this iteration's findings, then exit. Do not prompt for input. The loop will call you again for the next iteration, which will branch from the updated `master`.

---

## Branch Lifecycle Summary

| Verdict | Code commit | Revert commit | State commit | Master receives | Human action |
|---------|-------------|---------------|--------------|-----------------|--------------|
| Confirmed | ✅ | — | ✅ | state only | Review branch; merge if desired |
| Inconclusive | ✅ (if any) | — | ✅ | state only | Review branch; merge, discard, or iterate |
| Refuted | ✅ | ✅ | ✅ | state only | Discard branch |

---

## Rules

- Never ask the user a question.
- Never pause for confirmation.
- If you hit an error (build failure, missing file), fix it and continue.
- Each iteration must produce a concrete verdict on its hypothesis.
- **Never commit directly to `master`.** All experimental work goes on the iteration branch.
- **Only a human merges to `master`.** The loop cherry-picks only `AutonomousLoopState.md`.
- **Documentation and code are committed together.** A commit that changes tank behaviour without updating the relevant docs is not acceptable.
- **Refuted changes are always reverted** before the state commit. The branch is left as a readable audit trail, not a pending merge.
