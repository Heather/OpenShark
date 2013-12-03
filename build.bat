@echo off

::Env
if %PROCESSOR_ARCHITECTURE%==x86 (
         set MSBuild="%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
) else ( set MSBuild="%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
)


::generate resources
"C:\Program Files\Microsoft SDKs\Windows\v7.0A\bin\RC.Exe" "OpenShark.rc"

::build
%MSBuild% OpenShark.fsproj /p:TargetFramework=net45 /p:Configuration=Release

::handle errors / show output
pause