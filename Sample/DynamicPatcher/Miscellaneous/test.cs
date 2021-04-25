
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension;

namespace Test
{
    public class HTest
    {
        //[Hook(HookType.AresHook, Address = 0x6FCFA0, Size = 5)]
        static public unsafe UInt32 ShowFirer_work(REGISTERS* R)
        {
            ref TechnoClass rTechno = ref ((Pointer<TechnoClass>)R->ESI).Ref;
            ref TechnoTypeClass rType = ref rTechno.Type.Ref;
            ref HouseClass rHouse = ref rTechno.Owner.Ref;
            unsafe
            {
                string ID = rType.Base.Base.UIName;
                string HouseID = rHouse.Type.Ref.Base.UIName;
                Logger.Log("{0}({1}) fired", ID, HouseID);
            };
            int rof = 1919810;
            if (rTechno.Owner == HouseClass.Player)
            {
                rof = new Random().Next(0, 50);
            }
            else
            {
                rof = new Random().Next(114, 514);
            }
            Logger.Log("next ROF: " + rof);
            R->EAX = (uint)rof;
            Logger.Log("");

            return 0x6FCFBE;
        }

        [Hook(HookType.AresHook, Address = 0x550F6A, Size = 8)]
        static public unsafe UInt32 LaserDrawClass_Fade(REGISTERS* R)
        {
            ref LaserDrawClass pThis = ref ((Pointer<LaserDrawClass>)R->EBX).Ref;
            int thickness = pThis.Thickness;

            var curColor = ((Pointer<ColorStruct>)R->lea_Stack(0x14)).Data;

            bool doNot_quickDraw = R->Stack<bool>(0x13);
            R->ESI = doNot_quickDraw ? 8u : 64u;

            // faster
            if (thickness <= 5)
            {
                R->EAX = (uint)(curColor.R >> 1);
                R->ECX = (uint)(curColor.G >> 1);
                R->EDX = (uint)(curColor.B >> 1);
                return 0x550F9D;
            }

            int layer = R->Stack<int>(0x5C);

            ColorStruct innerColor = pThis.InnerColor;
            ColorStruct maxColor;
            if (pThis.IsSupported)
            {
                maxColor = innerColor;
                thickness--;
            }
            else
            {
                maxColor = new ColorStruct((byte)(innerColor.R >> 1), (byte)(innerColor.G >> 1), (byte)(innerColor.B >> 1));
            }
            byte max = Math.Max(maxColor.R, Math.Max(maxColor.G, maxColor.B));

            double w = Math.PI * ((double)(max - 8) / (double)(2 * thickness * max));
            double mul = Math.Cos(w * layer);

            R->EAX = (uint)(maxColor.R * mul);
            R->ECX = (uint)(maxColor.G * mul);
            R->EDX = (uint)(maxColor.B * mul);
            //Logger.Log("LaserDrawClass_Fade::RGB:{0},{1},{2}, layer:{3}\n", curColor.R, curColor.G, curColor.B, layer);

            return 0x550F9D;
        }
        
        static public object writebytesfunc() => new byte[]{0x11,0x45,0x14,0x19,0x19,0x81};
		public delegate object writebytesdlg();

        [Hook(HookType.WriteBytesHook, Address = 0x7E03E0, Size = 8)]
        static public writebytesdlg writebytestest = writebytesfunc;
        
        static public object errorlogtestfunc() => throw new InvalidOperationException("you can't call this function.");
		public delegate object errorlogtestdlg();

        [Hook(HookType.WriteBytesHook, Address = 0x7E03F0, Size = 5)]
        static public errorlogtestdlg errorlogtest = errorlogtestfunc;
    }
}
