using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherYRpp
{
    [StructLayout(LayoutKind.Explicit, Size = 92)]
    public struct LaserDrawClass
    {
        [FieldOffset(28)] public int Thickness; // only respected if IsHouseColor
        [FieldOffset(32)] public bool IsHouseColor;
        [FieldOffset(33)] public bool IsSupported; // this changes the values for InnerColor (false: halve, true: double), HouseColor only


        [FieldOffset(65)] public ColorStruct InnerColor;
        [FieldOffset(68)] public ColorStruct OuterColor;
        [FieldOffset(71)] public ColorStruct OuterSpread;
    }
}
