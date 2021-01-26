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

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate IntPtr GetTechnoTypeDelegate(ref TechnoClass techno);
        static public GetTechnoTypeDelegate GetTechnoTypeDlg = Marshal.GetDelegateForFunctionPointer<GetTechnoTypeDelegate>(new IntPtr(0x6F3270));

        public Pointer<TechnoTypeClass> Type  { get => GetTechnoTypeDlg(ref this); }

        [FieldOffset(0)]
        public ObjectClass Base;

        [FieldOffset(540)]
        public Pointer<HouseClass> Owner;
    }


    [StructLayout(LayoutKind.Explicit, Size = 3576)]
    public struct TechnoTypeClass
    {
        [FieldOffset(0)]
        public AbstractTypeClass Base;
    }
}
