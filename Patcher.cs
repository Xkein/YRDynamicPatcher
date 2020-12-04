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
    class Patcher
    {

        List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();
        Compiler compiler = new Compiler();
        Dictionary<string, Assembly> fileAssembly = new Dictionary<string, Assembly>();

        public void Init(string workDir)
        {
            Logger.WriteLine += (string str) => Console.WriteLine(str);

            var logFileStream = new FileStream(Path.Combine(workDir, "patcher.log"), FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            var logFileWriter = new StreamWriter(logFileStream);
            Logger.WriteLine += (string str) => {
                logFileWriter.WriteLine(str); logFileWriter.Flush();
            };

            using (StreamReader file = File.OpenText(Path.Combine(workDir, "config.json")))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    var json = JObject.Load(reader);

                    if (json["show_attach_window"].ToObject<bool>())
                    {
                        System.Windows.Forms.MessageBox.Show("Attach Me", "Dynamic Patcher");
                    }

                    compiler.Load(json);
                }
            }

            stopwatch.Start();
        }

        public void StartWatchPath(string path)
        {
            Task.Run(() => WatchPath(path));
        }

        public void WatchPath(string path)
        {
            if (Directory.Exists(path) == false)
            {
                Logger.Log("direction not exists: " + path);
                return;
            }

            FirstCompile(path);

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
                return compiler.Compile(path);
            }
            catch (Exception e)
            {
                Logger.Log("compile error: " + e.Message);
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
            Logger.Log("sleep: " + time.TotalSeconds);
            Thread.Sleep(time);

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

        void RefreshAssembly(string path, Assembly assembly)
        {
            if (fileAssembly.ContainsKey(path))
            {
                Logger.Log("replace assembly {0} with {1}", fileAssembly[path].FullName, assembly.FullName);
                RemoveAssemblyHook(fileAssembly[path]);
                fileAssembly[path] = assembly;
            }
            else
            {
                fileAssembly.Add(path, assembly);
            }
            ApplyAssembly(assembly);
        }

        void ApplyAssembly(Assembly assembly)
        {
            Logger.Log("appling: " + assembly.FullName);

            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                MethodInfo[] methods = type.GetMethods();
                foreach (MethodInfo method in methods)
                {
                    if (method.IsDefined(typeof(HookAttribute), false))
                    {
                        ApplyHook(method);
                    }
                }
            }
        }

        void ApplyHook(MethodInfo method)
        {
            var info = new HookInfo(method);
            HookAttribute hook = info.GetHookAttribute();

            Logger.Log("appling {3} hook: {0:X}, {1}, {2:X}", hook.Address, method.Name, hook.Size, hook.Type);

            try
            {
                switch (hook.Type)
                {
                    case HookType.AresHook:
                        ApplyAresHook(info);
                        break;
                    case HookType.SimpleJumpToRet:
                        ASMWriter.WriteJump(new JumpStruct(hook.Address, info.GetReturnValue()));
                        break;
                    case HookType.DirectJumpToHook:
                        ASMWriter.WriteJump(new JumpStruct(hook.Address, (int)info.GetCallable()));
                        break;
                    default:
                        Logger.Log("found unkwnow hook: " + method.Name);
                        break;
                }

                ASMWriter.FlushInstructionCache(hook.Address, Math.Max(hook.Size, 5));
            }
            catch (Exception e)
            {
                Logger.Log("hook applied error: " + e.Message);
            }
        }

        Dictionary<string, AresHookTransferStation> transferStations = new Dictionary<string, AresHookTransferStation>();

        private void ApplyAresHook(HookInfo info)
        {
            string key = info.Method.Name;

            if (transferStations.ContainsKey(key))
            {
                transferStations[key].SetHook(info);
            }
            else
            {
                var station = new AresHookTransferStation(info);
                transferStations.Add(key, station);
            }
        }

        void RemoveAssemblyHook(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                MethodInfo[] methods = type.GetMethods();
                foreach (MethodInfo method in methods)
                {
                    if (method.IsDefined(typeof(HookAttribute), false))
                    {
                        var info = new HookInfo(method);
                        string key = info.Method.Name;

                        if (transferStations.ContainsKey(key))
                        {
                            Logger.Log("remove hook: " + info.Method.Name);
                            transferStations[key].UnHook();
                        }
                    }
                }
            }
        }
    }
}
