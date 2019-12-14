C:\Engine.Player\Engine.BuildTools.Builder.CLI.exe --xml %1\BuildSettings.xml
move /Y %1\Build\%1.game Builds\%1.game
@rd /S %1\Build