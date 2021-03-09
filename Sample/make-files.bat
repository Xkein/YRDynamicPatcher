
cd /d %~dp0

del /q DynamicPatcher\Libraries
for /r ..\bin\Release %%i in (*.dll) do copy %%i DynamicPatcher\Libraries
del .\DynamicPatcher\Libraries\DynamicPatcher.dll

copy ..\PatcherLoader\Release\PatcherLoader.dll
copy ..\PatcherLoader\Release\DynamicPatcher.dll

copy ..\LICENSE.MD DynamicPatcherLICENSE.MD

pause
