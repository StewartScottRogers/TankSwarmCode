@echo off
setlocal enabledelayedexpansion

::  War.cmd  -  The Endless Tank Swarm War
::
::  Red and Blue fight, learn from every battle, improve, and
::  fight again. Forever. Blood sport.
::
::  Each round:
::    1. Red Engineering takes one turn  (fight, learn, evolve)
::    2. Blue Engineering takes one turn (fight, learn, evolve)
::    3. Neutral score check (50 matches, seed 9999)
::    4. Every GUI_INTERVAL rounds: launch GUI spectacle
::
::  Usage:
::    War.cmd                     Run forever, GUI every 5 rounds
::    War.cmd 20                  Run 20 rounds then stop
::    War.cmd 0 10                Run forever, GUI every 10 rounds
::    War.cmd 50 5                Run 50 rounds, GUI every 5
::
::  Arguments:
::    %1  MAX_ROUNDS    0 = run forever (default: 0)
::    %2  GUI_INTERVAL  Show GUI every N rounds (default: 5)
::
::  Press Ctrl+C at any time to end the war.

set "MAX_ROUNDS=0"
set "GUI_INTERVAL=5"

if not "%~1"=="" set "MAX_ROUNDS=%~1"
if not "%~2"=="" set "GUI_INTERVAL=%~2"

set "ROUND=0"

:: Ensure we always run from the repo root (where this file lives)
pushd "%~dp0"

cls
echo.
echo  ============================================================
echo   TANK SWARM WAR  -  BLOOD SPORT
echo   Red vs Blue  -  They live to fight
echo   GUI spectacle every !GUI_INTERVAL! rounds
echo   MAX_ROUNDS: !MAX_ROUNDS! (0 = infinite)
echo   Press Ctrl+C to end the war
echo  ============================================================
echo.
echo  Team state files:
echo    Red:  TankSwarmCode.SwarmTanks.Red.Engineering\LoopState.md
echo    Blue: TankSwarmCode.SwarmTanks.Blue.Engineering\LoopState.md
echo.

:: Pre-build the CLI runner once so score checks are fast
echo [%TIME%] Pre-building CLI runner...
dotnet build TankSwarmCode.Cli/TankSwarmCode.Cli.csproj -c Release -v quiet
if errorlevel 1 (
    echo [%TIME%] WARNING: CLI pre-build failed. Score checks may be slow.
)
echo.

:: ============================================================
:warloop
:: ============================================================

:: Check if we've hit max rounds
if not "!MAX_ROUNDS!"=="0" (
    if !ROUND! geq !MAX_ROUNDS! goto :warend
)

set /a ROUND+=1

echo.
echo  ============================================================
echo   ROUND !ROUND!   -   %DATE%  %TIME%
echo  ============================================================
echo.

:: --- RED'S TURN ----------------------------------------------
echo  --- RED ENGINEERING --- Taking the field ---
echo.
call TankSwarmCode.SwarmTanks.Red.Engineering\AutonomousLoop.cmd 1
if errorlevel 1 (
    echo.
    echo  [!TIME!] WARNING: Red loop reported an error in round !ROUND!.
    echo  Continuing war...
)
echo.
echo  [!TIME!] Red turn complete.
echo.

:: --- BLUE'S TURN ---------------------------------------------
echo  --- BLUE ENGINEERING --- Taking the field ---
echo.
call TankSwarmCode.SwarmTanks.Blue.Engineering\AutonomousLoop.cmd 1
if errorlevel 1 (
    echo.
    echo  [!TIME!] WARNING: Blue loop reported an error in round !ROUND!.
    echo  Continuing war...
)
echo.
echo  [!TIME!] Blue turn complete.
echo.

:: --- NEUTRAL SCORE CHECK -------------------------------------
echo  --- SCORE CHECK --- Round !ROUND! neutral battle ---
echo.
echo  [!TIME!] Running 50 neutral matches (seed 9999)...
echo.
dotnet run --project TankSwarmCode.Cli/TankSwarmCode.Cli.csproj --no-build -- ^
  --bot1 TankSwarmCode.SwarmTanks.Red/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Red.dll ^
  --bot2 TankSwarmCode.SwarmTanks.Blue/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Blue.dll ^
  --batch 50 --parallel 16 --seed 9999 --on-timeout energy --format table
echo.

:: --- PERIODIC GUI SPECTACLE ----------------------------------
set /a "GUI_CHECK=ROUND %% GUI_INTERVAL"
if !GUI_CHECK! == 0 (
    echo.
    echo  ============================================================
    echo   SPECTACLE - !ROUND! rounds of war complete
    echo   Watch them fight. Close the window to resume.
    echo  ============================================================
    echo.
    dotnet run --project TankSwarmCode.Gui/TankSwarmCode.Gui.csproj
    echo.
    echo  ============================================================
    echo   GUI closed. The war continues.
    echo  ============================================================
    echo.
)

goto :warloop

:: ============================================================
:warend
:: ============================================================
echo.
echo  ============================================================
echo   WAR COMPLETE - !ROUND! rounds fought
echo  ============================================================
echo.
echo  Final state:
echo    Red:  TankSwarmCode.SwarmTanks.Red.Engineering\LoopState.md
echo    Blue: TankSwarmCode.SwarmTanks.Blue.Engineering\LoopState.md
echo.
echo  [!TIME!] Running final score check (200 matches)...
echo.
dotnet run --project TankSwarmCode.Cli/TankSwarmCode.Cli.csproj --no-build -- ^
  --bot1 TankSwarmCode.SwarmTanks.Red/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Red.dll ^
  --bot2 TankSwarmCode.SwarmTanks.Blue/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Blue.dll ^
  --batch 200 --parallel 16 --seed 9999 --on-timeout energy --format table
echo.
echo  Launching final spectacle...
dotnet run --project TankSwarmCode.Gui/TankSwarmCode.Gui.csproj
echo.
echo  The strongest survived.

popd
endlocal
