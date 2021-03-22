using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherLauncher
{
    class Registration
    {
        public static bool Register(string dllPath)
        {
            Process regasm = new Process();
            regasm.StartInfo.FileName = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe";
            regasm.StartInfo.Arguments = dllPath;
            regasm.StartInfo.UseShellExecute = false;
            regasm.StartInfo.CreateNoWindow = false;
            regasm.EnableRaisingEvents = true;
            regasm.Start();
            regasm.WaitForExit();

            return regasm.ExitCode == 0;
        }

        public static bool Unregister(string dllPath)
        {
            Process regasm = new Process();
            regasm.StartInfo.FileName = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe";
            regasm.StartInfo.Arguments = "/u " + dllPath;
            regasm.StartInfo.UseShellExecute = false;
            regasm.StartInfo.CreateNoWindow = false;
            regasm.EnableRaisingEvents = true;
            regasm.Start();
            regasm.WaitForExit();

            return regasm.ExitCode == 0;
        }
    }
}
