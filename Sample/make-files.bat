 
cd /d %~dp0

copy ..\PatcherLoader\Release\PatcherLoader.dll
copy ..\PatcherLoader\Release\DynamicPatcher.dll DynamicPatcher_RELEASE.dll
copy ..\PatcherLoader\Debug\DynamicPatcher.dll

copy ..\LICENSE.MD DynamicPatcherLICENSE.MD

