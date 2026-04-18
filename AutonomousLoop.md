# Autonomous Karpathy Loop — Research Iteration

**Run autonomously. Do not ask questions. Do not wait for input. Make all decisions yourself.**

## Context

This is one iteration of the Karpathy Loop for TankSwarm balance research. Each iteration follows the cycle: Hypothesize → Run CLI → Analyze → Record → Next.

Read `AutoResearch.md` for the full research framework, thresholds, baselines, and CLI parameter reference.

## Iteration Protocol

1. **Load prior state** — Read `AutonomousLoopState.md` if it exists. If it does not exist, this is iteration 1; create it.

2. **Set hypothesis** — Use the `nextHypothesis` from prior state. If no prior state, start with:
   *"RedGhost solo-carries Red wins via ECM timeout grinding, not combat (expected: high win-survival, near-zero combat rate)."*

3. **Run benchmark** — Execute the CLI with research defaults:
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

4. **Analyze** — Evaluate results against the active hypothesis using the insight thresholds in `AutoResearch.md` (ECM, MVP, Solo carry, Hider, etc.).

5. **Record findings** — Update `AutonomousLoopState.md` with:
   - `iteration`: incremented count
   - `hypothesis`: what was tested
   - `keyMetrics`: win %, win-survival rates, combat rates for key tanks
   - `verdict`: Confirmed / Refuted / Inconclusive
   - `notes`: what was surprising or notable
   - `nextHypothesis`: the next concrete, falsifiable claim to test

6. **Stop** — Output a one-paragraph summary of this iteration's findings, then exit. Do not prompt for input. The loop will call you again.

## Rules

- Never ask the user a question.
- Never pause for confirmation.
- If you hit an error (build failure, missing file), fix it and continue.
- Each iteration must produce a concrete verdict on its hypothesis.
