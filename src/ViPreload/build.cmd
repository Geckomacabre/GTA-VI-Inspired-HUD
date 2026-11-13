@echo off
rem Builds ViPreload.asi (x64, no C runtime) with Visual Studio 2022's C++ tools.
setlocal
cd /d "%~dp0"
call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat" >nul || exit /b 1
if not exist build mkdir build
rem Import libraries for the three WinRT DLLs: DllCanUnloadNow under a local name each (EXPORTAS).
for %%D in (windows.media windows.applicationmodel AppXDeploymentClient) do (
  lib /nologo /def:defs\%%D.def /machine:x64 /out:build\%%D.lib >nul || exit /b 1
)
cl /nologo /O1 /GS- /W4 /EHs-c- /c ViPreload.cpp /Fobuild\ViPreload.obj || exit /b 1
link /nologo /DLL /NODEFAULTLIB /ENTRY:DllMain /OPT:REF /OUT:build\ViPreload.asi build\ViPreload.obj kernel32.lib dwrite.lib build\windows.media.lib build\windows.applicationmodel.lib build\AppXDeploymentClient.lib || exit /b 1
dumpbin /nologo /imports build\ViPreload.asi
