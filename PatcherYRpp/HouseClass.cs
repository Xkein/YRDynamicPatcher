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
        public static Pointer<HouseClass> Player { get => ((Pointer<Pointer<HouseClass>>)player).Data; set => ((Pointer<Pointer<HouseClass>>)player).Ref = value; }
        private static IntPtr player = new IntPtr(0xA83D4C);
        public static Pointer<HouseClass> Observer { get => ((Pointer<Pointer<HouseClass>>)observer).Data; set => ((Pointer<Pointer<HouseClass>>)observer).Ref = value; }
        private static IntPtr observer = new IntPtr(0xAC1198);

        [FieldOffset(48)]
        public int ArrayIndex;

        [FieldOffset(52)]
        public Pointer<HouseTypeClass> Type;
    }

    [StructLayout(LayoutKind.Explicit, Size = 432)]
    public struct HouseTypeClass
    {
        [FieldOffset(0)]
        public AbstractTypeClass Base;

    }
}
