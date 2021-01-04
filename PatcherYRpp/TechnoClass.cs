using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherYRpp
{
    [StructLayout(LayoutKind.Explicit, Size = 1312)]
    public struct TechnoClass
    {
        static public readonly IntPtr ArrayPointer = new IntPtr(0xA8EC78);
        static public ref DynamicVectorClass<Pointer<TechnoClass>> Array { get => ref DynamicVectorClass<Pointer<TechnoClass>>.GetDynamicVector(ArrayPointer); }

        public Pointer<TechnoTypeClass> Type { get => YRPP.GetTechnoType(this.GetThisPointer()); }

        [FieldOffset(540)]
        Pointer<HouseClass> Owner;
    }


    [StructLayout(LayoutKind.Explicit, Size = 3576)]
    public struct TechnoTypeClass
    {
        [FieldOffset(0)]
        AbstractTypeClass Base;
    }
}
