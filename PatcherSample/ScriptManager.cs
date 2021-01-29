using DynamicPatcher;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherSample
{
    public interface IScriptable
    {
    }
    public interface IScriptable<T> : IScriptable
    {
        public T Owner { get; }
    }
    public class TechnoScriptable : IScriptable<TechnoExt>
    {
        public TechnoScriptable(TechnoExt owner)
        {
            Owner = owner;
        }

        public TechnoExt Owner { get; protected set; }
    }

    public enum ScriptEventType
    {
        OnUpdate
    }

    public class Script
    {
        public static readonly List<string> EventNames = new List<string>(Enum.GetNames(typeof(ScriptEventType)));
        public Script(string name)
        {
            Name = name;
        }

        public MethodInfo GetEvent(string eventName)
        {
            return Events[eventName];
        }
        public MethodInfo this[string eventName] => GetEvent(eventName);
        public MethodInfo this[ScriptEventType eventType] => GetEvent(eventType.ToString());

        public void SetEvents(Type type)
        {
            ScriptableType = type;
            foreach (string eventName in EventNames)
            {
                MethodInfo method = ScriptableType.GetMethod(eventName);
                SetEvent(eventName, method);
            }
        }

        private void SetEvent(string eventName, MethodInfo method)
        {
            if (Events.ContainsKey(eventName))
            {
                Events[eventName] = method;
            }
            else
            {
                Events.Add(eventName, method);
            }
        }

        public string Name;
        public Type ScriptableType;
        public Dictionary<string, MethodInfo> Events = new Dictionary<string, MethodInfo>();
    }

    public class ScriptManager
    {
        static Dictionary<string, Script> Scripts = new Dictionary<string, Script>();

        static public Script GetScript(string filename)
        {
            Script script = new Script(filename);

            var pair = Program.Patcher.FileAssembly.First((pair) => Path.GetFileNameWithoutExtension(pair.Key) == filename);
            Assembly assembly = pair.Value;

            RefreshScript(script, assembly);

            if (Scripts.ContainsKey(filename))
            {
                Scripts[filename] = script;
            }
            else
            {
                Scripts.Add(filename, script);
            }

            return script;
        }
        private static void Patcher_AssemblyRefresh(object sender, AssemblyRefreshEventArgs args)
        {
            if (Scripts.ContainsKey(args.FileName))
            {
                RefreshScript(Scripts[args.FileName], args.RefreshedAssembly);

                // [warning!] unsafe change to scriptable
                ref var technoArray = ref TechnoClass.Array;
                for (int i = 0; i < technoArray.Count; i++)
                {
                    Pointer<TechnoClass> pTechno = technoArray[i];
                    TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                    if(ext.Scriptable != null)
                    {
                        TechnoTypeExt extType = ext.Type;
                        ext.scriptable = GetScriptable(extType.Script, ext) as TechnoScriptable;
                    }
                }
            }
        }

        private static void RefreshScript(Script script, Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (typeof(IScriptable).IsAssignableFrom(type))
                {
                    script.SetEvents(type);
                    break;
                }
            }
        }

        static ScriptManager()
        {
            Program.Patcher.AssemblyRefresh += Patcher_AssemblyRefresh;
        }

        static public IScriptable<T> GetScriptable<T>(Script script, T owner)
        {
            return Activator.CreateInstance(script.ScriptableType, owner) as IScriptable<T>;
        }


        //[Hook(HookType.AresHook, Address = 0x6F9E50, Size = 5)]
        static public unsafe UInt32 TechnoClass_Update_Script(REGISTERS* R)
        {
            Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
            TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
            TechnoTypeExt extType = ext.Type;

            extType.Script?[ScriptEventType.OnUpdate]?.Invoke(ext.Scriptable, null);

            return 0;
        }
    }
}
