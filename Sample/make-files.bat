 
cd /d %~dp0

set Libraries=DynamicPatcher\Libraries

if exist %Libraries% (
	del /q %Libraries%
) else (
	mkdir %Libraries%
)
for /r ..\bin\Release %%i in (*.dll) do copy %%i %Libraries%
del .\%Libraries%\DynamicPatcher.dll

copy ..\PatcherLoader\Release\PatcherLoader.dll
copy ..\PatcherLoader\Release\DynamicPatcher.dll DynamicPatcher_RELEASE.dll
copy ..\PatcherLoader\Debug\DynamicPatcher.dll

copy ..\LICENSE.MD DynamicPatcherLICENSE.MD

