
%1 %2
ver|find "5.">nul&&goto :Admin
mshta vbscript:createobject("shell.application").shellexecute("%~s0","goto :Admin","","runas",1)(window.close)&goto :eof
:Admin
cd /d %~dp0
C:\Windows\Microsoft.NET\Framework\v4.0.30319\regasm  DynamicPatcher.dll
pause