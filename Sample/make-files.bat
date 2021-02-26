
cd /d %~dp0

del /q DynamicPatcher\Libraries
for /r ..\bin\Debug %%i in (*.dll) do copy %%i DynamicPatcher\Libraries
del .\DynamicPatcher\Libraries\DynamicPatcher.dll

copy ..\PatcherLoader\Debug\PatcherLoader.dll
copy ..\PatcherLoader\Debug\DynamicPatcher.dll

copy ..\LICENSE.MD DynamicPatcherLICENSE.MD

pause
