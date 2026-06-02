@echo off
:: Build + run tests. Claude mode: --no-pause (clean exit). Human mode: no args (window stays open).
set EXE=%~dp0MediaManager\bin\Release\MediaManager.exe
set START_TIME=%TIME%

:: Locate MSBuild via vswhere (works with any VS 2017+ installation)
for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do set MSBUILD="%%i"
if not defined MSBUILD (
    echo [ERROR] MSBuild not found. Install Visual Studio with the .NET workload.
    if not "%1"=="--no-pause" cmd /k
    exit /b 1
)

:: Build
%MSBUILD% "%~dp0MediaManager.sln" -p:Configuration=Release -verbosity:minimal
if errorlevel 1 (
    echo [ERROR] Build failed.
    if not "%1"=="--no-pause" cmd /k
    exit /b 1
)

:: Run tests
"%EXE%" --test
set TEST_EXIT=%ERRORLEVEL%
echo.
echo Start: %START_TIME%  End: %TIME%
if %TEST_EXIT% neq 0 (
    echo [ERROR] Tests failed.
    if not "%1"=="--no-pause" cmd /k
    exit /b 1
)
if not "%1"=="--no-pause" cmd /k
exit /b 0
