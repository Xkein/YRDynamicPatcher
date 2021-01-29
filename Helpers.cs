using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Reflection;

namespace DynamicPatcher
{
    class Helpers
    {
        private static IntPtr yrHandle = IntPtr.Zero;

        static IntPtr YRHandle {
            get
            {
                if(yrHandle == IntPtr.Zero)
                {
                    yrHandle = FindYRProcessHandle();
                }
                return yrHandle;
            }
            set => yrHandle = value;
        }
        static private IntPtr FindYRProcessHandle()
        {

            Process[] processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (process.ProcessName.Contains("gamemd") && process.Id == Process.GetCurrentProcess().Id)
                {
                    var targetProcess = Process.GetCurrentProcess();
                    try
                    {
                        Logger.Log("find YR process: {0} ({1})", targetProcess.MainWindowTitle, targetProcess.Id);
                        targetProcess.EnableRaisingEvents = true;
                        targetProcess.Exited += (object sender, EventArgs e) => {
                            YRHandle = IntPtr.Zero;
                            Logger.Log("{0} ({1}) exited.", targetProcess.MainWindowTitle, targetProcess.Id);
                            };
                        return targetProcess.Handle;
                    }
                    catch (Exception e)
                    {
                        Logger.Log("get process handle error: " + e.Message);
                        throw e;
                    }
                }
            }

            throw new InvalidOperationException("could not find yr handle");
        }

        static public IntPtr GetProcessHandle()
        {
            return YRHandle;
        }

        public static string GetValidFullPath(string fileName)
        {
            string fullPath = Path.GetFullPath(fileName);

            // found in root directory
            if (File.Exists(fullPath))
            {
                return fullPath;
            }

            string directory = RuntimeEnvironment.GetRuntimeDirectory();
            fullPath = Path.Combine(directory, fileName);
            // found in runtime directory
            if (File.Exists(fullPath))
            {
                return fullPath;
            }

            string variable = Environment.GetEnvironmentVariable("Path");
            string[] dirs = variable.Split(';');

            foreach (string dir in dirs)
            {
                fullPath = Path.Combine(directory, fileName);
                // found in environment variable directory
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return fullPath;
        }

        public static string GetAssemblyPath(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in loadedAssemblies)
            {
                AssemblyName assemblyName = assembly.GetName();
                if (assemblyName.Name == name)
                {
                    return assembly.Location;
                }
            }

            string path = Helpers.GetValidFullPath(fileName);
            return path;
        }
    }
};


