using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPatcher
{
	public enum HookType
	{
		AresHook, SimpleJumpToRet, DirectJumpToHook
	};

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Delegate,
		Inherited = false, AllowMultiple = true)]
	public sealed class HookAttribute : Attribute
	{

		public HookType Type { get; }
		public int Address { get; set; }
		public int Size { get; set; }

		// not necessary
		// public string Name { get; set; }

		public HookAttribute(HookType type)
		{
			Type = type;
		}
	}

	class HookInfo
	{
		static readonly int InvalidAddress = 114514;
		MethodInfo Method { get; set; }
		public HookInfo(MethodInfo method)
		{
			Method = method;
		}

		public HookAttribute GetHookAttribute()
		{
			return Method.GetCustomAttribute(typeof(HookAttribute), false) as HookAttribute;
		}

		delegate int SimpleJumpFunction();
		delegate void HookFunction(params object[] paramters);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate UInt32 AresHookFunction(ref REGISTERS R);

		public int GetReturnValue()
		{
			if (GetHookAttribute().Type == HookType.SimpleJumpToRet)
			{
				SimpleJumpFunction function = Method.CreateDelegate(typeof(SimpleJumpFunction)) as SimpleJumpFunction;
				return function.Invoke();
			}
			return InvalidAddress;
		}

		public IntPtr GetCallable()
		{
			Type dlgType = null;
			switch (GetHookAttribute().Type)
			{
				case HookType.AresHook:
					dlgType = typeof(AresHookFunction);
					break;
				case HookType.SimpleJumpToRet:
					dlgType = typeof(SimpleJumpFunction);
					break;
				case HookType.DirectJumpToHook:
					dlgType = typeof(HookFunction);
					break;
			}
			Delegate dlg = Method.CreateDelegate(dlgType);

			return Marshal.GetFunctionPointerForDelegate(dlg);
		}
	}

    
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Register
	{
		UInt32 data;

		public UInt16 Get16()
		{
			return (UInt16)data;
		}

		public UInt32 Get()
		{
			return data;
		}

		public T Get<T>() where T : unmanaged
		{
			return (T)Convert.ChangeType(data, typeof(T));
		}

		public UInt32 Set(UInt32 value)
		{
			return data = value;
		}

		public void Set<T>(T value) where T : unmanaged
		{
			data = Convert.ToUInt32(value);
		}

		public void Set16(UInt16 value)
		{
			data = data | value;
		}

		public byte Get8Hi()
		{
			return (byte)(data >> 8);
		}

		public byte Get8Lo()
		{
			return (byte)data;
		}

		public void Set8Hi(byte value)
		{
			data = data | ((UInt32)value << 8);
		}

		public void Set8Lo(byte value)
		{
			data = data | value;
		}
		public T* lea<T>(int byteOffset) where T : unmanaged
		{
			return (T*)lea(byteOffset);
		}

		public UInt32 lea(int byteOffset)
		{
			return (UInt32)(data + byteOffset);
		}

		public T At<T>(int byteOffset) where T : unmanaged
		{
			return *lea<T>(byteOffset);
		}

		public void At<T>(int byteOffset, T value) where T : unmanaged
		{
			*lea<T>(byteOffset) = value;
		}
	};

	//A pointer to this class is passed as an argument to EXPORT functions
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct REGISTERS
	{
		private UInt32 origin;
		private UInt32 flags;

		private Register _EDI;
		private Register _ESI;
		private Register _EBP;
		private Register _ESP;
		private Register _EBX;
		private Register _EDX;
		private Register _ECX;
		private Register _EAX;

		public UInt32 Origin
        {
			get => origin;
			private set => origin = value;
		}

		public UInt32 EFLAGS
		{
			get => flags;
			set => flags = value;
		}

		public UInt32 EAX
		{
			get => _EAX.Get();
			set => _EAX.Set(value);
		}
		public UInt32 ECX
		{
			get => _ECX.Get();
			set => _ECX.Set(value);
		}
		public UInt32 EDX
		{
			get => _EDX.Get();
			set => _EDX.Set(value);
		}
		public UInt32 EBX
		{
			get => _EBX.Get();
			set => _EBX.Set(value);
		}
		public UInt32 ESP
		{
			get => _ESP.Get();
			set => _ESP.Set(value);
		}
		public UInt32 EBP
		{
			get => _EBP.Get();
			set => _EBP.Set(value);
		}
		public UInt32 EDI
		{
			get => _EDI.Get();
			set => _EDI.Set(value);
		}
		public UInt32 ESI
		{
			get => _ESI.Get();
			set => _ESI.Set(value);
		}
		#region REG_SHORTCUTS_XHL(A)
		public UInt16 AX
		{
			get => _EAX.Get16();
			set => _EAX.Set16(value);
		}
		public byte AH
		{
			get => _EAX.Get8Hi();
			set => _EAX.Set8Hi(value);
		}
		public byte AL
		{
			get => _EAX.Get8Lo();
			set => _EAX.Set8Lo(value);
		}
		#endregion
		#region REG_SHORTCUTS_XHL(B)
		public UInt16 BX
		{
			get => _EBX.Get16();
			set => _EBX.Set16(value);
		}
		public byte BH
		{
			get => _EBX.Get8Hi();
			set => _EBX.Set8Hi(value);
		}
		public byte BL
		{
			get => _EBX.Get8Lo();
			set => _EBX.Set8Lo(value);
		}
		#endregion
		#region REG_SHORTCUTS_XHL(C)
		public UInt16 CX
		{
			get => _ECX.Get16();
			set => _ECX.Set16(value);
		}
		public byte CH
		{
			get => _ECX.Get8Hi();
			set => _ECX.Set8Hi(value);
		}
		public byte CL
		{
			get => _ECX.Get8Lo();
			set => _ECX.Set8Lo(value);
		}
		#endregion
		#region REG_SHORTCUTS_XHL(D)
		public UInt16 DX
		{
			get => _EDX.Get16();
			set => _EDX.Set16(value);
		}
		public byte DH
		{
			get => _EDX.Get8Hi();
			set => _EDX.Set8Hi(value);
		}
		public byte DL
		{
			get => _EDX.Get8Lo();
			set => _EDX.Set8Lo(value);
		}
		#endregion

		public T lea_Stack<T>(int offset) where T : unmanaged
		{
			return (T)Convert.ChangeType(_ESP.lea(offset), typeof(T));
		}

		public UInt32 lea_Stack(int offset)
		{
			return _ESP.lea(offset);
		}

		public ref T ref_Stack<T>(int offset) where T : unmanaged
		{
			return ref *(T*)lea_Stack(offset);
		}

		public T Stack<T>(int offset) where T : unmanaged
		{
			return _ESP.At<T>(offset);
		}

		public UInt32 Stack32(int offset)
		{
			return _ESP.At<UInt32>(offset);
		}

		public UInt16 Stack16(int offset)
		{
			return _ESP.At<UInt16>(offset);
		}

		public byte Stack8(int offset)
		{
			return _ESP.At<byte>(offset);
		}

		public T Base<T>(int offset) where T : unmanaged
		{
			return _EBP.At<T>(offset);
		}

		public void Stack<T>(int offset, T value) where T : unmanaged
		{
			_ESP.At(offset, value);
		}

		public void Stack16(int offset, UInt16 value)
		{
			_ESP.At(offset, value);
		}

		public void Stack8(int offset, byte value)
		{
			_ESP.At(offset, value);
		}

		public void Base<T>(int offset, T value) where T : unmanaged
		{
			_EBP.At(offset, value);
		}
	};

}
