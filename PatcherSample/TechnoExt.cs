using DynamicPatcher;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatcherSample
{
    [Serializable]
    public class TechnoExt : Extension<TechnoClass>
    {
        public static Container<TechnoExt, TechnoClass> ExtMap = new Container<TechnoExt, TechnoClass>("TechnoClass");

        internal TechnoScriptable scriptable;
        public TechnoScriptable Scriptable
        { 
            get
            {
                if(scriptable == null)
                {
                    if(Type.Script != null)
                    {
                        scriptable = ScriptManager.GetScriptable(Type.Script, this) as TechnoScriptable;
                    }
                }
                return scriptable;
            }
        }

        public TechnoTypeExt Type { get => TechnoTypeExt.ExtMap.Find(OwnerObject.Ref.Type); }

        public TechnoExt(Pointer<TechnoClass> OwnerObject) : base(OwnerObject)
        {
        }

        //[Hook(HookType.AresHook, Address = 0x6F3260, Size = 5)]
        static public unsafe UInt32 TechnoClass_CTOR(REGISTERS* R)
        {
            var pItem = (Pointer<TechnoClass>)R->ESI;

            TechnoExt.ExtMap.FindOrAllocate(pItem);
            return 0;
        }

        //[Hook(HookType.AresHook, Address = 0x6F4500, Size = 5)]
        static public unsafe UInt32 TechnoClass_DTOR(REGISTERS* R)
        {
            var pItem = (Pointer<TechnoClass>)R->ECX;

            TechnoExt.ExtMap.Remove(pItem);
            return 0;
        }
    }

}
