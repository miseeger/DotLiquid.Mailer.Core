@echo off
set NUGETOUTPUTDIR=packages
@nuget pack DotLiquid.Mailer.Core.nuspec -OutputDirectory %NUGETOUTPUTDIR%

@echo.
dir %NUGETOUTPUTDIR%
@echo.
@echo Done!
set NUGETOUTPUTDIR=