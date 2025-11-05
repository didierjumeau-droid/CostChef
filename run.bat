@echo off
echo Starting CostChef...
echo.

if exist "bin\Release\net8.0-windows\CostChef.exe" (
    bin\Release\net8.0-windows\CostChef.exe
) else (
    echo Application not found. Please run compile.bat first.
)

pause