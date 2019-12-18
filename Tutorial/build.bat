echo Building Project %1
ebuild --xml %1\BuildSettings.xml
move /Y %1\Build\%1.game Builds\%1.game
move /Y %1\Build\build\%1.dll ExampleRunner/assets/%1.dll
@rd /S /Q %1\Build