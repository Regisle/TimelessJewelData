@echo off
setlocal

if /I "%~1"=="/?" goto usage
if /I "%~1"=="-h" goto usage
if /I "%~1"=="--help" goto usage

set "REPO_ROOT=%~dp0"
pushd "%REPO_ROOT%" >nul

set "ADDITIONS_JSON=D:\PoB Dev Work\TimelessJewelData\AlternatePassiveAdditions.json"
set "SKILLS_JSON=D:\PoB Dev Work\TimelessJewelData\AlternatePassiveSkills.json"
set "TREE_JSON=D:\PoB Dev Work\GGG Skill Tree\data.json"
set "OUTPUT_DIR=D:\PoB Dev Work\PathOfBuildingCommunity\src\Data\TimelessJewelData"
set "OUTPUT_TYPE=both"
set "REMOVE_NODE_MAPPING=0"

if /I "%~1"=="data" (
    set "OUTPUT_DIR=%REPO_ROOT%Data"
    set "OUTPUT_TYPE=uncompressed"
    set "REMOVE_NODE_MAPPING=1"
) else if /I "%~1"=="--repo-data" (
    set "OUTPUT_DIR=%REPO_ROOT%Data"
    set "OUTPUT_TYPE=uncompressed"
    set "REMOVE_NODE_MAPPING=1"
) else if not "%~1"=="" (
    set "OUTPUT_TYPE=%~1"
)

if not exist "%ADDITIONS_JSON%" goto missing_additions
if not exist "%SKILLS_JSON%" goto missing_skills
if not exist "%TREE_JSON%" goto missing_tree

echo Running timeless jewel data generation...
echo.

dotnet run --project "Datafile Generator\DatafileGenerator\DataFileGenerator.csproj" -c Release -- "%ADDITIONS_JSON%" "%SKILLS_JSON%" "%TREE_JSON%" "%OUTPUT_DIR%" "%OUTPUT_TYPE%"

set "EXITCODE=%ERRORLEVEL%"
echo.
if not "%EXITCODE%"=="0" (
    echo Generation failed with exit code %EXITCODE%.
    goto done
)

if "%REMOVE_NODE_MAPPING%"=="1" (
    if exist "%OUTPUT_DIR%\NodeIndexMapping.lua" del /q "%OUTPUT_DIR%\NodeIndexMapping.lua"
)

echo Generation complete.
goto done

:missing_additions
echo Could not find additions file:
echo   %ADDITIONS_JSON%
set "EXITCODE=1"
goto done

:missing_skills
echo Could not find skills file:
echo   %SKILLS_JSON%
set "EXITCODE=1"
goto done

:missing_tree
echo Could not find tree file:
echo   %TREE_JSON%
set "EXITCODE=1"
goto done

:usage
echo Usage:
echo   GenerateTimelessData.cmd [compressed^|uncompressed^|both^|1^|2^|3]
echo   GenerateTimelessData.cmd [data^|--repo-data]
echo.
echo Examples:
echo   GenerateTimelessData.cmd
echo   GenerateTimelessData.cmd compressed
echo   GenerateTimelessData.cmd data
set "EXITCODE=0"

:done
popd >nul
echo.
pause
exit /b %EXITCODE%
