@echo off
echo Building CostChef Application...
echo.

dotnet build --configuration Release

if %errorlevel% equ 0 (
    echo.
    echo ✅ Build successful!
    echo 📁 Output: bin\Release\net8.0-windows\CostChef.exe
    echo.
    echo Run the application with: run.bat
) else (
    echo.
    echo ❌ Build failed!
    echo Please check for errors above.
)

pause