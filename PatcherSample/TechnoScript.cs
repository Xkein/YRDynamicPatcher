using DynamicPatcher;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherSample
{
    public class TechnoScript
    {
        //[Hook(HookType.AresHook, Address = 0x6F9E50, Size = 5)]
        static public unsafe UInt32 TechnoClass_Update(REGISTERS* R)
        {
            ref TechnoClass rTechno = ref ((Pointer<TechnoClass>)R->ESI).Ref;
            ref TechnoTypeClass rType = ref rTechno.Type.Ref;

            TechnoExt ext = TechnoExt.ExtMap.Find((Pointer<TechnoClass>)R->ESI);
            TechnoTypeExt extType = TechnoTypeExt.ExtMap.Find(rTechno.Type);

            if (extType.Script_Update != null)
            {
                var pair = Program.Patcher.FileAssembly.First((pair) => Path.GetFileNameWithoutExtension(pair.Key) == extType.Script_Update);
                Assembly assembly = pair.Value;
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    MethodInfo method = type.GetMethod("Update", new Type[] { typeof(Pointer<TechnoClass>) });
                    if (method != null)
                    {
                        method.Invoke(null, new object[] { (Pointer<TechnoClass>)R->ESI });
                    }
                }
            }

            return 0;
        }
    }
}
