rem in work
@echo off 
cls
set "csc_path=C:\Windows\Microsoft.NET\Framework\v4.0.30319\"
set "csc_run=%csc_path%csc.exe"
echo %csc_run%
SET "PATH=%PATH%;C:\Windows\Microsoft.NET\Framework\v4.0.30319\1033\"
set "root_path=.\"
set "build_path=.\build\win\"
set "name=citadel.exe"
cd "../../"
set 
START /b %csc_run%  -out:%build_path%%name% %root_path%Citadel\*.cs
pause
@echo on
