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
    /// <summary>Provides activation way for DynamicPatcher.</summary>
    [ComVisible(true), Guid("531A1F37-EA8F-4E60-975E-11D61EE68702")]
    public interface IPatcher
    {
    }

    /// <summary>The class to activate DynamicPatcher</summary>
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true), Guid("4BC759CC-5BB6-4E10-A14E-C813C869CE2F")]
    [ProgId("DynamicPatcher")]
    public class Program : IPatcher
    {
        /// <summary>The instance of DynamicPatcher.</summary>
        public static Patcher Patcher { get; private set; }

        static Program()
        {
            DllMain();
        }

        private static void DllMain()
        {
            string workDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DynamicPatcher");
            librariesDirectory = Path.Combine(workDir, "Libraries");
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            //AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

            Patcher = new Patcher();
            Patcher.Init(workDir);
            Task task = Patcher.Start();
            task.Wait(/*TimeSpan.FromSeconds(1.114514)*/);
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

            string fileName = args.Name.Split(',')[0] + ".dll";// Console.WriteLine("try loading assembly: " + args.Name);

            if (TryLoad(Path.Combine(librariesDirectory, fileName)))
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

        /// <summary>Init the class Program and invoke ctor.</summary>
        static public void Activate() { }
        /// <summary>Init the class Program and invoke ctor.</summary>
        static public int ActivateFromCLR(string _)
        {
            return 1919810;
        }
    }
}
