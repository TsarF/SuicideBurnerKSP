@echo off
set SOURCE="C:\Users\NIK\Documents\Visual Studio 2017\Projects\SuicideBurnerKSP\SuicideBurnerKSP\Output"
set DESTINATION="E:\Games\KSP\KSP v1.5.1.2335 - DEV\Kerbal Space Program v1.5.1.2335\Kerbal Space Program v1.5.1.2335"
xcopy %SOURCE% %DESTINATION% /D /E /C /R /I /K /Y
call KSP_x64.exe