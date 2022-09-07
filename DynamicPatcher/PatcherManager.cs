using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DynamicPatcher
{
    /// <summary>Dynamic Patcher Manager.</summary>
    public class PatcherManager
    {
        /// <summary>The instance of DynamicPatcher.</summary>
        public static Patcher Patcher { get; private set; }

        /// <summary>Initialize Dynamic Patcher.</summary>
        public static void Init()
        {
            try
            {
                string workDir = Path.Combine(Environment.CurrentDirectory, "DynamicPatcher");
                librariesDirectory = Path.Combine(workDir, "Libraries");
                AddDllDirectories();

                var patcher = Patcher = new Patcher();
                patcher.Init(workDir);
            }
            catch (Exception e)
            {
                Logger.WriteLine -= Console.WriteLine;
                Logger.WriteLine += Console.WriteLine;
                Logger.PrintException(e);
            }
        }

        /// <summary>Activate Dynamic Patcher.</summary>
        public static void Activate()
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

            if (fileName == "DynamicPatcher.dll")
            {
                return Assembly.Load(typeof(PatcherManager).Assembly.FullName);
            }

            string filePath = Helpers.SearchFileInDirectory(librariesDirectory, fileName);
            if (!string.IsNullOrEmpty(filePath) && TryLoad(filePath))
            {
                return assembly;
            }

            if (TryLoad(Helpers.GetAssemblyPath(fileName)))
            {
                return assembly;
            }

            // TOCHECK
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            assembly = loadedAssemblies.LastOrDefault(a => a.FullName == assemblyName.FullName);

            return assembly;
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
