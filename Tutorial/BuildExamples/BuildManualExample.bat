@echo off
echo Example Build Steps for Building with Copying Asset folder directly.

echo Building Project
call ebuild --build ..\ExampleRunner\ExampleRunner.csproj .\Build

echo Copying Asset Files:
xcopy ..\ExampleRunner\assets .\Build\assets /I /Y

echo Packing Game File
call ebuild --create-package .\Build ExampleRunner .\ExampleRunner.game True False --packager-version v2