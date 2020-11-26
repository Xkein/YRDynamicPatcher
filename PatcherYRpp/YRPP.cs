using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherYRpp
{
    public class YRPP
    {

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate IntPtr ThisCall_0(IntPtr pThis);

        static public ThisCall_0 GetTechnoType = Marshal.GetDelegateForFunctionPointer<ThisCall_0>(new IntPtr(0x6F3270));
    }
}
