using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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

    /// <summary>The class of DynamicPatcher.</summary>
    public class Patcher
    {
        List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();

        private CompilationManager CompilationManager { get; set; }

        /// <summary>The map of 'filename -> assembly'.</summary>
        public Dictionary<string, Assembly> FileAssembly { get; } = new Dictionary<string, Assembly>();

        /// <summary>Occurs when DynamicPatcher.Patcher.RefreshAssembly.</summary>
        public event AssemblyRefreshEventHandler AssemblyRefresh;

        internal Patcher()
        {
            Logger.WriteLine += (string str) => Console.WriteLine(str);
        }

        internal void Init(string workDir)
        {
            var logFileStream = new FileStream(Path.Combine(workDir, "patcher.log"), FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            var logFileWriter = new StreamWriter(logFileStream);
            Logger.WriteLine += (string str) =>
            {
                logFileWriter.WriteLine(str); logFileWriter.Flush();
            };

            try
            {
                using StreamReader file = File.OpenText(Path.Combine(workDir, "dynamicpatcher.config.json"));
                using JsonTextReader reader = new JsonTextReader(file);
                var json = JObject.Load(reader);

                if (json["show_attach_window"].ToObject<bool>())
                {
                    System.Windows.Forms.MessageBox.Show("Attach Me", "Dynamic Patcher");
                }

                if (json["try_catch_callable"].ToObject<bool>())
                {
                    HookInfo.TryCatchCallable = true;
                }

                if (json["force_gc_collect"].ToObject<bool>())
                {
                    Task.Run(() =>
                    {
                        while (true)
                        {
                            Logger.Log("Sleep 10s.");
                            Thread.Sleep(TimeSpan.FromSeconds(10));
                            Logger.Log("GC collect.");
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            GC.WaitForFullGCComplete();
                            Logger.Log("GC collect finish.");
                        }
                    });
                }

                CompilationManager = new CompilationManager(workDir);
            }
            catch (Exception e)
            {
                Helpers.PrintException(e);
            }

            stopwatch.Start();
        }

        /// <summary>Create a thread watching any changes of directory.</summary>
        public Task StartWatchPath(string path)
        {
            Task firstCompileTask = Task.Run(() =>
            {
                FirstCompile(path);
            });
            Task.Run(() =>
            {
                Logger.Log("waiting for first compile to complete");
                firstCompileTask.Wait();
                Logger.Log("first compile complete!");

                WatchPath(path);
            });

            return firstCompileTask;
        }

        /// <summary>Watch any changes of directory.</summary>
        public void WatchPath(string path)
        {
            if (Directory.Exists(path) == false)
            {
                Logger.Log("direction not exists: " + path);
                return;
            }

            var watcher = new FileSystemWatcher(path, "*.cs");

            watcher.Created += new FileSystemEventHandler(OnFileChanged);
            watcher.Changed += new FileSystemEventHandler(OnFileChanged);
            watcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            //watcher.Renamed += new RenamedEventHandler(OnRenamed);
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            watchers.Add(watcher);
        }

        Dictionary<string, TimeSpan> lastModifications = new Dictionary<string, TimeSpan>();
        Stopwatch stopwatch = new Stopwatch();

        private bool IsFileChanged(string path)
        {
            if (lastModifications.ContainsKey(path))
            {
                if (stopwatch.Elapsed - lastModifications[path] <= TimeSpan.FromSeconds(3.0))
                {
                    return false;
                }
                lastModifications[path] = stopwatch.Elapsed;
            }
            else
            {
                lastModifications.Add(path, stopwatch.Elapsed);
            }
            return true;
        }

        private Assembly TryCompile(string path)
        {
            try
            {
                return CompilationManager.Compile(path);
            }
            catch (Exception e)
            {
                Logger.Log("compile error!");
                Helpers.PrintException(e);
                return null;
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            string path = e.FullPath;

            if (IsFileChanged(path) == false)
            {
                return;
            }

            Logger.Log("");
            Logger.Log("detected file {0}: {1}", e.ChangeType, path);

            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                case WatcherChangeTypes.Created:
                    break;
                case WatcherChangeTypes.Deleted:
                    break;
            }

            // wait for editor releasing
            var time = TimeSpan.FromSeconds(1.0);
            Logger.Log("sleep: {0}s", time.TotalSeconds);
            Thread.Sleep(time);

            Logger.Log("");
            var assembly = TryCompile(path);

            if (assembly != null)
            {
                RefreshAssembly(path, assembly);
            }
            else
            {
                Logger.Log("file compile error: " + path);
            }
        }

        void FirstCompile(string path)
        {
            Logger.Log("first compile: " + path);

            var dir = new DirectoryInfo(path);
            var list = dir.GetFiles("*.cs", SearchOption.AllDirectories).ToList();

            foreach (var file in list)
            {
                string filePath = file.FullName;
                var project = CompilationManager.GetProjectFromFile(filePath);
                // skip because already compiled
                if (project == null)
                {
                    var assembly = TryCompile(filePath);

                    if (assembly != null)
                    {
                        RefreshAssembly(filePath, assembly);
                    }
                    else
                    {
                        Logger.Log("first compile error: " + file.FullName);
                    }
                }
            }
        }

        void RefreshAssembly(string path, Assembly assembly)
        {
            if (FileAssembly.ContainsKey(path))
            {
                Logger.Log("replace assembly '{0}' with '{1}'", FileAssembly[path].FullName, assembly.FullName);
                RemoveAssemblyHook(FileAssembly[path]);
                FileAssembly[path] = assembly;
            }
            else
            {
                FileAssembly.Add(path, assembly);
            }

            try
            {
                ApplyAssembly(assembly);

                AssemblyRefresh?.Invoke(this, new AssemblyRefreshEventArgs(Path.GetFileNameWithoutExtension(path), assembly));
            }
            catch (Exception e)
            {
                Logger.Log("apply error!");
                Helpers.PrintException(e);
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

                MemberInfo[] members = type.GetMembers();
                foreach (MemberInfo member in members)
                {
                    if (member.IsDefined(typeof(HookAttribute), false))
                    {
                        Logger.Log("");
                        ApplyHook(member);
                    }
                }
            }
            Logger.Log("-----------------------------------");
            Logger.Log("");
        }

        Dictionary<int, HookTransferStation> transferStations = new Dictionary<int, HookTransferStation>();

        void ApplyHook(MemberInfo member)
        {
            HookAttribute[] hooks = HookInfo.GetHookAttributes(member);
            foreach (var hook in hooks)
            {
                var info = new HookInfo(member, hook);

                Logger.Log("appling {3} hook: {0:X}, {1}, {2:X}", hook.Address, member.Name, hook.Size, hook.Type);

                try
                {
                    int key = hook.Address;

                    HookTransferStation station = null;

                    // use old station if exist
                    if (transferStations.ContainsKey(key))
                    {
                        station = transferStations[key];
                        if (station.HookInfos.Count <= 0 || hook.Type == station.MaxHookInfo.GetHookAttribute().Type)
                        {
                            Logger.Log("insert hook to key '{0:X}'", key);
                            station.SetHook(info);
                        }
                        else
                        {
                            Logger.Log("remove key '{0:X}' because of hook type mismatch. ", key);
                            transferStations.Remove(key);
                            station = null;
                        }
                    }

                    // create new station
                    if (station == null)
                    {
                        Logger.Log("add key '{0:X}'", key);
                        switch (hook.Type)
                        {
                            case HookType.AresHook:
                                station = new AresHookTransferStation(info);
                                break;
                            case HookType.SimpleJumpToRet:
                            case HookType.DirectJumpToHook:
                                station = new JumpHookTransferStation(info);
                                break;
                            default:
                                Logger.Log("found unkwnow hook: " + member.Name);
                                return;
                        }

                        transferStations.Add(key, station);
                    }

                    ASMWriter.FlushInstructionCache(hook.Address, Math.Max(hook.Size, 5));
                }
                catch (Exception e)
                {
                    Logger.Log("hook applied error!");
                    Helpers.PrintException(e);
                }
            }
        }

        void RemoveAssemblyHook(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                MemberInfo[] members = type.GetMembers();
                foreach (MemberInfo member in members)
                {
                    if (member.IsDefined(typeof(HookAttribute), false))
                    {
                        HookAttribute[] hooks = HookInfo.GetHookAttributes(member);
                        foreach (var hook in hooks)
                        {
                            var info = new HookInfo(member, hook);
                            int key = info.GetHookAttribute().Address;

                            if (transferStations.ContainsKey(key))
                            {
                                Logger.Log("remove hook: " + info.Method.Name);
                                info = transferStations[key].HookInfos.First(cur => cur.Member == member && cur.TransferStation == transferStations[key]);
                                transferStations[key].UnHook(info);
                            }
                        }
                    }
                }
            }
        }
    }
}
