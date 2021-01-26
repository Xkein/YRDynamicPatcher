using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static PatcherYRpp.YRPP;

namespace PatcherYRpp
{
    [StructLayout(LayoutKind.Explicit, Size = 92)]
    public struct LaserDrawClass
    {
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate IntPtr ConstructorFunction(ref LaserDrawClass pThis, CoordStruct source, CoordStruct target, int zAdjust, byte unknown,
            ColorStruct innerColor, ColorStruct outerColor, ColorStruct outerSpread,
            int duration, bool blinks, bool fades, float startIntensity, float endIntensity);

        static ConstructorFunction ConstructorDlg = Marshal.GetDelegateForFunctionPointer<ConstructorFunction>(new IntPtr(0x54FE60));
        static DestructorFunction DestructorDlg = Marshal.GetDelegateForFunctionPointer<DestructorFunction>(new IntPtr(0x54FFB0));

        public static void Constructor(Pointer<LaserDrawClass> pThis, CoordStruct source, CoordStruct target, int zAdjust, byte unknown,
            ColorStruct innerColor, ColorStruct outerColor, ColorStruct outerSpread,
            int duration, bool blinks = false, bool fades = true,
            float startIntensity = 1.0f, float endIntensity = 0.0f)
        {
            ConstructorDlg(ref pThis.Ref, source, target, zAdjust, unknown, innerColor, outerColor, outerSpread, duration, blinks, fades, startIntensity, endIntensity);
        }

        public static void Constructor(Pointer<LaserDrawClass> pThis, CoordStruct source, CoordStruct target, ColorStruct innerColor,
            ColorStruct outerColor, ColorStruct outerSpread, int duration)
        {
            Constructor(pThis, source, target, 0, 1, innerColor, outerColor, outerSpread, duration);
        }

        public void Destructor()
        {
            DestructorDlg(Pointer<LaserDrawClass>.AsPointer(ref this));
        }

        [FieldOffset(28)] public int Thickness; // only respected if IsHouseColor
        [FieldOffset(32)] public bool IsHouseColor;
        [FieldOffset(33)] public bool IsSupported; // this changes the values for InnerColor (false: halve, true: double), HouseColor only


        [FieldOffset(65)] public ColorStruct InnerColor;
        [FieldOffset(68)] public ColorStruct OuterColor;
        [FieldOffset(71)] public ColorStruct OuterSpread;
    }
}
