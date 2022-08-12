using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DynamicPatcher
{
    /// <summary>Provides activation way for DynamicPatcher.</summary>
    [ComVisible(true), Guid("531A1F37-EA8F-4E60-975E-11D61EE68702")]
    public interface IPatcher
    {
        /// <summary>Activate Dynamic Patcher.</summary>
        [DispId(1)]
        void Activate();
    }

    /// <summary>The class to activate DynamicPatcher</summary>
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true), Guid("4BC759CC-5BB6-4E10-A14E-C813C869CE2F")]
    [ProgId("DynamicPatcher")]
    public class Program : IPatcher
    {
        /// <summary>The instance of DynamicPatcher.</summary>
        public static Patcher Patcher { get; private set; }

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

        static Program()
        {
            try
            {
                Process yrProcess;
                while (FindYR(out yrProcess) == false)
                {
                    Thread.Sleep(100);
                }

                WindowManager.SetTopomost(yrProcess.MainWindowHandle);

                string workDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DynamicPatcher");
                librariesDirectory = Path.Combine(workDir, "Libraries");
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                //AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

                Patcher = new Patcher();
                Patcher.Init(workDir);
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
        }

        /// <summary>Activate Dynamic Patcher.</summary>
        public void Activate()
        {
            try
            {
                Task task = Patcher.Start();
                task.Wait();
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
        }

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            Console.WriteLine("loading assembly: " + args.LoadedAssembly.FullName);
        }

        private static string librariesDirectory;
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly = null;
            Func<string, bool> TryLoad = (string path) =>
            {
                if (File.Exists(path))
                {
                    //Console.WriteLine("loading assembly: " + path);
                    assembly = Assembly.LoadFile(path);
                    return true;
                }

                return false;
            };

            AssemblyName assemblyName = new AssemblyName(args.Name);
            string fileName = assemblyName.Name + ".dll";// Console.WriteLine("try loading assembly: " + args.Name);
            string[] files = Directory.GetFiles(librariesDirectory, fileName, SearchOption.AllDirectories);
            if (files.Length > 0 && TryLoad(Path.Combine(librariesDirectory, files[0])))
            {
                return assembly;
            }

            if (TryLoad(Helpers.GetAssemblyPath(fileName)))
            {
                return assembly;
            }

            return null;
        }

        private static void LoadLibraries(string workDir)
        {
            string dir = Path.Combine(workDir, "Libraries");
            var info = new DirectoryInfo(dir);
            var files = info.GetFiles("*.dll");

            foreach (FileInfo file in files)
            {
                Assembly.LoadFile(file.FullName);
            }
        }

        private static void AddDllDirectories()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            //AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

            //NativeDll.EnableDllDirectories();

            List<string> dirs = Directory.GetDirectories(librariesDirectory, "*", SearchOption.AllDirectories).ToList();
            dirs.Add(librariesDirectory);
            foreach (var dir in dirs)
            {
                NativeDll.AddDllDirectory(dir);
            }
        }
    }
}