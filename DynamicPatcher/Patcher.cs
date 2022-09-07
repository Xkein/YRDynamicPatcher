using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
    /// <summary>Provides data for the DynamicPatcher.Patcher.AssemblyRefresh event.</summary>
    public class AssemblyRefreshEventArgs : EventArgs
    {
        /// <summary>Initializes a new instance of the DynamicPatcher.AssemblyRefreshEventArgs class.</summary>
        public AssemblyRefreshEventArgs(string fileName, Assembly refreshedAssembly)
        {
            FileName = fileName;
            RefreshedAssembly = refreshedAssembly;
        }

        /// <summary>Gets string that represents the currently file name.</summary>
        public string FileName { get; private set; }

        /// <summary>Gets an System.Reflection.Assembly that represents the currently refreshed assembly.</summary>
        public Assembly RefreshedAssembly { get; private set; }
    }
    /// <summary>Represents the method that handles the DynamicPatcher.Patcher.AssemblyRefresh event of an DynamicPatcher.Patcher.</summary>
    public delegate void AssemblyRefreshEventHandler(object sender, AssemblyRefreshEventArgs args);


    /// <summary>Run class constructor before hook.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public sealed class RunClassConstructorFirstAttribute : Attribute
    {
    }

    /// <summary>The class of DynamicPatcher.</summary>
    public class Patcher
    {
        /// <summary>Work directory of DynamicPatcher. </summary>
        public string WorkDirectory { get; private set; }
#if DEVMODE
        private CompilationManager CompilationManager { get; set; }
#endif

        /// <summary>The map of 'filename -> assembly'.</summary>
        public Dictionary<string, Assembly> FileAssembly { get; } = new Dictionary<string, Assembly>();

        /// <summary>Occurs when DynamicPatcher.Patcher.RefreshAssembly.</summary>
        public event AssemblyRefreshEventHandler AssemblyRefresh;

        HookManager hookManager;
        CodeWatcher codeWatcher;

        bool copyLogFiles;

        internal Patcher()
        {
        }

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        /// <summary>Occurs when an exception is not caught.</summary>
        public event UnhandledExceptionEventHandler ExceptionHandler;

        internal void Init(string workDir)
        {
            WorkDirectory = workDir;

            string logFileName = Path.Combine(workDir, "patcher.log");
            string backupFileName = BackupLogFile(workDir, logFileName);
            CreateLogFile(logFileName);

            //DateTime date = DateTime.Now;
            //logFileName = Path.Combine(workDir, "Logs", string.Format("patcher_{0}_{1}_{2}_{3}_{4}.log", date.Year, date.Month, date.Day, date.Hour, date.Minute));
            //CreateLogFile(logFileName);

            AddExceptionHandler(workDir, logFileName);

            LoadConfig(workDir);

            Logger.Log("working directory: " + workDir);


#if DEVMODE
            Logger.Log("initializing CompilationManager...");
            CompilationManager = new CompilationManager(workDir);
#endif

            if (!copyLogFiles && !string.IsNullOrEmpty(backupFileName))
            {
                File.Delete(backupFileName);
            }

            Logger.Log("initializing HookManager...");
            hookManager = new HookManager();

            Logger.Log("initializing CodeWatcher...");
            codeWatcher = new CodeWatcher(workDir);
#if DEVMODE
            codeWatcher.FirstAction += FirstCompile;
            codeWatcher.OnCodeChanged += OnCodeChanged;
#else
            codeWatcher.FirstAction += LoadPackedAssemblies;
#endif
        }

        private void AddExceptionHandler(string workDir, string logFileName)
        {
            ExceptionHandler += (object sender, UnhandledExceptionEventArgs args) =>
            {
                if (args != null)
                {
                    Logger.PrintException(args.ExceptionObject as Exception);
                }
            };

            ExceptionHandler += (object sender, UnhandledExceptionEventArgs args) =>
            {
                string dir = Path.Combine(workDir, "ErrorLogs");
                Directory.CreateDirectory(dir);
                DateTime date = DateTime.Now;

                File.Copy(logFileName,
                    Path.Combine(dir, string.Format("ErrorLog_{0}_{1}_{2}_{3}_{4}.log",
                    date.Year, date.Month, date.Day, date.Hour, date.Minute)), true);
                System.Windows.Forms.MessageBox.Show("ErrorLog Created", "Dynamic Patcher");
            };
        }

        private void LoadConfig(string workDir)
        {
            using StreamReader file = File.OpenText(Path.Combine(workDir, "dynamicpatcher.config.json"));
            using JsonTextReader reader = new JsonTextReader(file);
            var json = JObject.Load(reader);

            if (!json["hide_console"].ToObject<bool>())
            {
                AllocConsole();
                Logger.WriteLine += Console.WriteLine;
            }

            Logo.ShowLogo();

            if (json["show_attach_window"].ToObject<bool>())
            {
                System.Windows.Forms.MessageBox.Show("Attach Me", "Dynamic Patcher");
            }

            if (json["try_catch_callable"].ToObject<bool>())
            {
                HookInfo.TryCatchCallable = true;
            }
            Logger.Log("try-catch callable: " + HookInfo.TryCatchCallable);

            if (json["force_gc_collect"].ToObject<bool>())
            {
                Task.Run(() =>
                {
                    Action showGCInfo = () =>
                    {
                        var curProc = Process.GetCurrentProcess();
                        Logger.Log("Total Memory: {0} MB", curProc.PrivateMemorySize64 / 1024 / 1024);
                        Logger.Log("Managed Memory: {0} MB", GC.GetTotalMemory(true) / 1024 / 1024);
                        for (int g = 0; g <= GC.MaxGeneration; g++)
                        {
                            Logger.Log("{0} Generation Count: {1}", g, GC.CollectionCount(g));
                        }

                    };
                    while (true)
                    {
                        Logger.Log("Sleep 10s.");
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                        Logger.Log("----------------------");
                        Logger.Log("GC collecting...");
                        showGCInfo();
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.WaitForFullGCComplete();
                        Logger.Log("GC collect finish.");
                        showGCInfo();
                    }
                });
            }

            copyLogFiles = (bool?)json["copy_logs"] ?? false;
            Logger.Log("CopyLogFiles: " + copyLogFiles);
        }

        private static void CreateLogFile(string logFileName)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logFileName));
            FileStream logFileStream = new FileStream(logFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            var logFileWriter = new StreamWriter(logFileStream);
            logFileWriter.AutoFlush = true;

            Logger.WriteLine += (string str) =>
            {
                logFileWriter.WriteLine(str);
            };
        }

        private static string BackupLogFile(string workDir, string logFileName)
        {
            if (File.Exists(logFileName))
            {
                FileInfo fileInfo = new FileInfo(logFileName);
                string backupFileName = Path.Combine(workDir, "Logs", string.Format("patcher_{0}.log", fileInfo.LastWriteTime.ToString("yyyy_MM_dd_HHmm")));
                Directory.CreateDirectory(Path.GetDirectoryName(backupFileName));
                File.Copy(logFileName, backupFileName, true);
                return backupFileName;
            }
            return null;
        }

        internal Task Start()
        {
            Task task = codeWatcher.StartWatchPath();
#if !DEVMODE
            codeWatcher.Stop();
#endif
            return task;
        }

#if DEVMODE
        private bool TryCompile(string path, out Assembly assembly)
        {
            assembly = null;
            try
            {
                assembly = CompilationManager.Compile(path);
            }
            catch (Exception e)
            {
                Logger.LogError("compile error!");
                Logger.PrintException(e);
            }
            return assembly != null;
        }

        private void OnCodeChanged(object sender, FileSystemEventArgs e)
        {
            string path = e.FullPath;

            Logger.Log("");
            Logger.Log("detected file {0}: {1}", e.ChangeType, path);

            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                case WatcherChangeTypes.Created:
                    break;
                case WatcherChangeTypes.Deleted:
                    Logger.Log("remove assembly '{0}' hooks.", FileAssembly[path].FullName);
                    hookManager.RemoveAssemblyHook(FileAssembly[path]);
                    return;
                case WatcherChangeTypes.Renamed:
                    string oldPath = (e as RenamedEventArgs).OldFullPath;
                    if (FileAssembly.ContainsKey(oldPath))
                    {
                        Logger.Log("remove assembly '{0}' hooks.", FileAssembly[oldPath].FullName);
                        hookManager.RemoveAssemblyHook(FileAssembly[oldPath]);
                    }
                    break;
            }

            // wait for editor releasing
            var time = TimeSpan.FromSeconds(1.0);
            Logger.Log("sleep: {0}s", time.TotalSeconds);
            Thread.Sleep(time);

            Logger.Log("");

            if (TryCompile(path, out var assembly))
            {
                RefreshAssembly(path, assembly);
            }
            else
            {
                Logger.LogError("file compile error: " + path);
            }
        }

        void FirstCompile(string path)
        {
            try
            {
                Logger.Log("first compile: " + path);

                var dir = new DirectoryInfo(path);
                var list = dir.GetFiles("*.cs", SearchOption.AllDirectories).ToList();

                List<Tuple<string, Assembly>> assemblies = new();

                foreach (var file in list)
                {
                    if (file.Name.StartsWith(".NET"))
                    {
                        Logger.Log("skip compiling {0}", file.FullName);
                        continue;
                    }

                    string filePath = file.FullName;
                    var project = CompilationManager.GetProjectFromFile(filePath);
                    // skip because already compiled
                    if (project == null)
                    {
                        if (TryCompile(filePath, out var assembly))
                        {
                            assemblies.Add(new Tuple<string, Assembly>(filePath, assembly));
                            //RefreshAssembly(filePath, assembly);
                        }
                        else
                        {
                            Logger.LogError("first compile error: " + file.FullName);
                            Logger.Log("");
                        }
                    }
                }

                assemblies.ForEach((tuple) => RefreshAssembly(tuple.Item1, tuple.Item2));
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
                throw;
            }
        }
#else
        private void LoadPackedAssemblies(string workDir)
        {
            string buildDir = Path.Combine(workDir, "Build");
            string projectsDir = Path.Combine(workDir, "Build", "Projects");

            var packageManager = new PackageManager(workDir);
            packageManager.ReadPackedList();
            foreach (string packed in packageManager.PackedList)
            {
                packageManager.UnPack(packed);

                Assembly assembly = Assembly.LoadFrom(packed);

                if (packed.StartsWith(projectsDir))
                {
                    continue;
                }

                string originPath = Path.ChangeExtension(packed.Replace(buildDir, workDir), "cs");
                RefreshAssembly(originPath, assembly);
            }
        }
#endif

        void RefreshAssembly(string path, Assembly assembly)
        {
            if (FileAssembly.ContainsKey(path))
            {
                Logger.Log("replace assembly '{0}' with '{1}'", FileAssembly[path].FullName, assembly.FullName);
                hookManager.RemoveAssemblyHook(FileAssembly[path]);
                FileAssembly[path] = assembly;
            }
            else
            {
                foreach (var pair in FileAssembly.Where(pair => Path.GetFileNameWithoutExtension(pair.Key) == Path.GetFileNameWithoutExtension(path)))
                {
                    Logger.LogWarning("{0} has same Assembly name with {1}", pair.Key, path);
                }
                FileAssembly.Add(path, assembly);
            }

            try
            {
                ApplyAssembly(assembly);

                AssemblyRefresh?.Invoke(this, new AssemblyRefreshEventArgs(Path.GetFileNameWithoutExtension(path), assembly));
            }
            catch (Exception e)
            {
                Logger.LogError("apply error!");
                Logger.PrintException(e);
            }
        }

        void ApplyAssembly(Assembly assembly)
        {
            Logger.Log("appling: " + assembly.FullName);

            Logger.Log("-----------------------------------");
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                Logger.Log("in class {0}: ", type.FullName);
                if (type.IsDefined(typeof(RunClassConstructorFirstAttribute), true))
                {
                    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                }

                MemberInfo[] members = type.GetMembers();
                foreach (MemberInfo member in members)
                {
                    if (member.IsDefined(typeof(HookAttribute), false))
                    {
                        Logger.Log("");
                        hookManager.ApplyHook(member);
                    }
                }
            }
            Logger.Log("-----------------------------------");
            Logger.Log("");
        }
    }
}
