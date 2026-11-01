@echo off
setlocal

set "REPO_ROOT=%~dp0"
call "%REPO_ROOT%GenerateTimelessData.cmd" data
exit /b %ERRORLEVEL%
