using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherYRpp
{
    [StructLayout(LayoutKind.Explicit, Size = 152, CharSet = CharSet.Ansi)]
	public struct AbstractTypeClass
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x18)]
		[FieldOffset(36)] string ID;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
		[FieldOffset(61)] string UINameLabel;

		[MarshalAs(UnmanagedType.LPWStr, SizeConst = 0x20)]
		[FieldOffset(96)] string UIName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x31)]
		[FieldOffset(100)] char Name;
	}
}
