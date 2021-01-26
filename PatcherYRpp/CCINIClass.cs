using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherYRpp
{
    [StructLayout(LayoutKind.Explicit, Size = 88)]
    public struct INIClass
    {

    }

    [StructLayout(LayoutKind.Explicit, Size = 88)]
    public struct CCINIClass
    {
        private static IntPtr pINI_Rules = new IntPtr(0x887048);
        public static Pointer<CCINIClass> INI_Rules { get => ((Pointer<Pointer<CCINIClass>>)pINI_Rules).Data; set => ((Pointer<Pointer<CCINIClass>>)pINI_Rules).Ref = value; }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int ReadStringFunction(ref CCINIClass pThis, [MarshalAs(UnmanagedType.LPStr)]string pSection,
            [MarshalAs(UnmanagedType.LPStr)] string pKey, [MarshalAs(UnmanagedType.LPStr)] string pDefault, byte[] buffer, int bufferSize);
        static ReadStringFunction ReadStringDlg = Marshal.GetDelegateForFunctionPointer<ReadStringFunction>(new IntPtr(0x528A10));

        public int ReadString(string section, string key, string def, byte[] buffer, int bufferSize)
        {
            return ReadStringDlg(ref this, section, key, def, buffer, bufferSize);
        }

        [FieldOffset(0)]
        public INIClass Base;
    }
}
