@echo off
echo Example Build Steps for Building with Copying Asset folder directly.

echo Building Project
call ebuild --build ..\ExampleRunner\ExampleRunner.csproj .\StandaloneBuild

echo Copying Asset Files:
xcopy ..\ExampleRunner\assets .\StandaloneBuild\assets /I /Y

echo Packing Game File
call ebuild --create-package .\StandaloneBuild ExampleRunner .\ExampleRunner_Standalone.game True False --packager-version v2 --packer-override-engine-version standalone