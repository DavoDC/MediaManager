@echo off
:: Build + run tests. Claude mode: --no-pause (clean exit). Human mode: no args (window stays open).
set EXE=%~dp0MediaManager\bin\Release\MediaManager.exe
set MSBUILD="C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
set START_TIME=%TIME%

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
