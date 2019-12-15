@echo off
mkdir Builds

call build.bat AI
call build.bat Audio
call build.bat GettingStarted
call build.bat OpenCL_Begin
call build.bat OpenFL_Begin
call build.bat OpenFL_Runner
call build.bat Physics
call build.bat Raycasting
call build.bat RenderTargets

echo Building Example Runner
C:\Engine.Player\Engine.BuildTools.Builder.CLI.exe --xml ExampleRunner\BuildSettings.xml
move /Y ExampleRunner\Build\ExampleRunner.game ExampleRunner.game
@rd /S /Q ExampleRunner\Build

pause