
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using DynamicPatcher;
using PatcherYRpp;

namespace PatcherSample
{
    public class HookTest
    {
        //[Hook(HookType.AresHook, Address = 0x6FCFA0, Size = 5)]
        static public UInt32 ShowFirer(ref REGISTERS R)
        {
            try
            {
                ref TechnoClass pTechno = ref ((Pointer<TechnoClass>)R.ESI).Ref;
                ref TechnoTypeClass pType = ref pTechno.Type.Ref;
                unsafe
                {
                    string ID = Marshal.PtrToStringUni(pType.Base.UIName);
                    Logger.Log(ID + " fired");
                };
                var rof = new Random().Next(10, 50);
                Logger.Log("next ROF: " + rof);
                R.EAX = (uint)rof;
                Logger.Log("");
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }
            return 0x6FCFBE;
        }

        //[Hook(HookType.AresHook, Address = 0x550F6A, Size = 8)]
        static public unsafe UInt32 LaserDrawClass_Fade(ref REGISTERS R)
        {
            try
            {
                ref LaserDrawClass pThis = ref ((Pointer<LaserDrawClass>)R.EBX).Ref;
                int thickness = pThis.Thickness;

                var curColor = ((Pointer<ColorStruct>)R.lea_Stack(0x14)).Data;

                bool doNot_quickDraw = R.Stack<bool>(0x13);
                R.ESI = doNot_quickDraw ? 8 : 64;

                // faster
                if (thickness <= 5)
                {
                    R.EAX = (uint)(curColor.R >> 1);
                    R.ECX = (uint)(curColor.G >> 1);
                    R.EDX = (uint)(curColor.B >> 1);
                    return 0x550F9D;
                }

                int layer = R.Stack<int>(0x5C);

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

                R.EAX = (uint)(maxColor.R * mul);
                R.ECX = (uint)(maxColor.G * mul);
                R.EDX = (uint)(maxColor.B * mul);
                //Logger.Log("LaserDrawClass_Fade::RGB:{0},{1},{2}, layer:{3}\n", curColor.R, curColor.G, curColor.B, layer);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }
            return 0x550F9D;
        }

    }
}

