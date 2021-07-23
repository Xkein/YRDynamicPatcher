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
        private static IntPtr processHandle = IntPtr.Zero;

        static IntPtr ProcessHandle {
            get
            {
                if(processHandle == IntPtr.Zero)
                {
                    processHandle = FindProcessHandle();
                }
                return processHandle;
            }
            set => processHandle = value;
        }
        static private IntPtr FindProcessHandle()
        {

            Process[] processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (process.Id == Process.GetCurrentProcess().Id)
                {
                    var targetProcess = Process.GetCurrentProcess();
                    try
                    {
                        Logger.Log("find process: {0} ({1})", targetProcess.MainWindowTitle, targetProcess.Id);
                        targetProcess.EnableRaisingEvents = true;
                        targetProcess.Exited += (object sender, EventArgs e) => {
                            ProcessHandle = IntPtr.Zero;
                            Logger.Log("{0} ({1}) exited.", targetProcess.MainWindowTitle, targetProcess.Id);
                            };
                        return targetProcess.Handle;
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("get process handle error: " + e.Message);
                        throw e;
                    }
                }
            }

            throw new InvalidOperationException("could not find process handle");
        }

        static public IntPtr GetProcessHandle()
        {
            return ProcessHandle;
        }

        public static List<string> AdditionalSearchPath { get; } = new List<string>();
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

            foreach (string dir in AdditionalSearchPath)
            {
                fullPath = Path.Combine(dir, fileName);
                // found in additional directory
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            string variable = Environment.GetEnvironmentVariable("Path");
            string[] dirs = variable.Split(';');

            foreach (string dir in dirs)
            {
                fullPath = Path.Combine(dir, fileName);
                // found in environment variable directory
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return null;
        }

        public static Assembly GetLoadedAssembly(string name)
        {
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in loadedAssemblies)
            {
                AssemblyName assemblyName = assembly.GetName();
                if (string.Equals(assemblyName.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return assembly;
                }
            }

            return null;
        }

        public static string GetAssemblyPath(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);

            Assembly assembly = GetLoadedAssembly(name);
            if (assembly != null)
            {
                return assembly.Location;
            }

            string path = Helpers.GetValidFullPath(fileName);
            return path;
        }

        public static ProcessModule GetProcessModule(string moduleName = null)
        {
            Process process = Process.GetCurrentProcess();
            if (string.IsNullOrEmpty(moduleName))
            {
                return process.MainModule;
            }

            foreach (ProcessModule module in process.Modules)
            {
                if(string.Equals(module.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase))
                {
                    return module;
                }
            }

            throw new DllNotFoundException($"Could not find module '{moduleName}'.");
        }

        public static bool GetProcessModuleAt(int address, out ProcessModule processModule)
        {
            processModule = null;
            Process process = Process.GetCurrentProcess();
            foreach (ProcessModule module in process.Modules)
            {
                if (AddressInModule(address, module))
                {
                    processModule = module;
                    return true;
                }
            }

            return false;
        }

        public static bool AddressInModule(int address, ProcessModule module)
        {
            return (int)module.BaseAddress <= address && address <= (int)module.BaseAddress + module.ModuleMemorySize;
        }
    }
};


