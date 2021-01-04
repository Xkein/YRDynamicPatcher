using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherYRpp
{
    [StructLayout(LayoutKind.Explicit, Size = 90296)]
    public struct HouseClass
    {
        [FieldOffset(48)]
        int ArrayIndex;

        [FieldOffset(52)]
        Pointer<HouseTypeClass> Type;
    }

    [StructLayout(LayoutKind.Explicit, Size = 432)]
    public struct HouseTypeClass
    {
        [FieldOffset(0)]
        AbstractTypeClass Base;

    }
}
