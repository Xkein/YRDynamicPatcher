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
        Dictionary<string, TimeSpan> lastModifications = new Dictionary<string, TimeSpan>();
        Stopwatch stopwatch = new Stopwatch();

        public void Init(string workDir)
        {
            Logger.OutputStream = new FileStream(Path.Combine(workDir, "patcher.log"), FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

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

            Thread.Sleep(TimeSpan.FromSeconds(1.0));

            var assembly = TryCompile(path);

            if (assembly != null)
            {
                if (fileAssembly.ContainsKey(path))
                {
                    RemoveAssemblyHook(fileAssembly[path]);
                }
                else
                {
                    fileAssembly.Add(path, assembly);
                }
                ApplyAssembly(assembly);
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
                string fileName = file.FullName;
                var assembly = TryCompile(fileName);

                if (assembly != null)
                {
                    fileAssembly.Add(path, assembly);
                    ApplyAssembly(assembly);
                }
                else
                {
                    Logger.Log("first compile error: " + file.FullName);
                }
            }
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

        void RemoveAssemblyHook(Assembly assembly)
        {

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

        private void ApplyAresHook(HookInfo info)
        {
            HookAttribute hook = info.GetHookAttribute();
            byte INIT = ASM.INIT;

            byte[] code_call =
            {
                0x60, 0x9C, //PUSHAD, PUSHFD
		        0x68, INIT, INIT, INIT, INIT, //PUSH HookAddress
		        0x83, 0xEC, 0x04,//SUB ESP, 4
		        0x8D, 0x44, 0x24, 0x04,//LEA EAX,[ESP + 4]
		        0x50, //PUSH EAX
		        0xE8, INIT, INIT, INIT, INIT,  //CALL ProcAddress
		        0x83, 0xC4, 0x0C, //ADD ESP, 0Ch
		        0x89, 0x44, 0x24, 0xF8,//MOV ss:[ESP - 8], EAX
		        0x9D, 0x61, //POPFD, POPAD
		        0x83, 0x7C, 0x24, 0xD4, 0x00,//CMP ss:[ESP - 2Ch], 0
		        0x74, 0x04, //JZ .proceed
		        0xFF, 0x64, 0x24, 0xD4 //JMP ss:[ESP - 2Ch]
            };

            var callable = (int)info.GetCallable();

            Logger.Log("ares hook callable: 0x{0:X}", callable);

            var pMemory = MemoryHelper.AllocMemory(code_call.Length + hook.Size + ASM.Jmp.Length);

            Logger.Log("ares hook alloc: 0x{0:X}", pMemory);

            if (pMemory != (int)IntPtr.Zero) {

                MemoryHelper.Write(pMemory, code_call, code_call.Length);

                MemoryHelper.Write(pMemory + 3, hook.Address);
                ASMWriter.WriteCall(new JumpStruct(pMemory + 0xF, callable));

                var origin_code_offset = pMemory + code_call.Length;

                if (hook.Size > 0)
                {
                    byte[] over = new byte[hook.Size];
                    MemoryHelper.Read(hook.Address, over, hook.Size);
                    MemoryHelper.Write(origin_code_offset, over, hook.Size);
                }

                var jmp_back_offset = origin_code_offset + hook.Size;

                ASMWriter.WriteJump(new JumpStruct(jmp_back_offset, hook.Address + hook.Size));

                ASMWriter.WriteJump(new JumpStruct(hook.Address, pMemory));

                //_hook_info[hook.Name].caller = pMemory;
            }
        }
    }
}
