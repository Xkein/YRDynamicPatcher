using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DynamicPatcher;

namespace PatcherLauncher
{
    class Program
    {
        public static bool FindYR(out Process yrProcess)
        {
            Process[] processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (process.ProcessName.Contains("gamemd"))
                {
                    yrProcess = process;
                    return true;
                }
            }

            yrProcess = null;
            return false;
        }

        public static bool HasLoader(Process process)
        {
            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName == PatcherLoader)
                {
                    return true;
                }
            }
            return false;
        }

        const string DynamicPatcher = "DynamicPatcher.dll";
        const string PatcherLoader = "PatcherLoader.dll";

        public static void Main(string[] args)
        {
            Process yrProcess;
            while (FindYR(out yrProcess) == false)
            {
                Thread.Sleep(100);
            }

            try
            {
                Registration.Register(DynamicPatcher);

                Remote remote = new Remote(yrProcess);
                remote.LoadLibrary(PatcherLoader);

                yrProcess.WaitForExit();
                Registration.Unregister(DynamicPatcher);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Patcher Launcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
