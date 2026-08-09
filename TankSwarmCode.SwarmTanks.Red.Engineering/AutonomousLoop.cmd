@echo off
setlocal enabledelayedexpansion
set "LOOP_COUNT=3"
if not "%~1"=="" set "LOOP_COUNT=%~1"

pushd "%~dp0\.."

echo AutonomousLoop [Red Engineering] starting: %LOOP_COUNT% iterations
echo.

for /l %%i in (1,1,%LOOP_COUNT%) do (
    echo === Iteration %%i / %LOOP_COUNT% ===
    powershell -NoProfile -Command "$p = Get-Content -Path 'TankSwarmCode.SwarmTanks.Red.Engineering\AutonomousLoop.md' -Raw; claude --dangerously-skip-permissions -p $p"
    if errorlevel 1 (
        echo Iteration %%i failed. Stopping.
        goto :done
    )
    echo.
)

:done
echo AutonomousLoop [Red Engineering] complete.
popd
endlocal
