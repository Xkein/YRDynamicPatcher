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
		public MethodInfo Method { get; private set; }
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

	class AresHookTransferStation : IDisposable
	{
		static readonly byte INIT = ASM.INIT;
		static readonly byte[] code_call =
			{
				0x60, 0x9C, //PUSHAD, PUSHFD
		        0x68, INIT, INIT, INIT, INIT, //PUSH HookAddress
		        0x83, 0xEC, 0x04,//SUB ESP, 4
		        0x8D, 0x44, 0x24, 0x04,//LEA EAX,[ESP + 4]
		        0x50, //PUSH EAX
		        0xE8, INIT, INIT, INIT, INIT,  //CALL ProcAddress
		        0x83, 0xC4, 0x0C, //ADD ESP, 0Ch
		        0x89, 0x44, 0x24, 0xF8,//MOV ss:[ESP - 8], EAX
		        0x9D, 0x61, //POPFD, POPAD
		        0x83, 0x7C, 0x24, 0xD4, 0x00,//CMP ss:[ESP - 2Ch], 0
		        0x74, 0x04, //JZ .proceed
		        0xFF, 0x64, 0x24, 0xD4 //JMP ss:[ESP - 2Ch]
            };

		MemoryHandle memoryHandle;
		byte[] code_over;

        public HookInfo HookInfo { get; set; }

		public AresHookTransferStation(HookInfo info)
		{
			// alloc bigger space
			memoryHandle = new MemoryHandle(code_call.Length + 0x10 + ASM.Jmp.Length);
			SetHook(info);
		}

		IntPtr GetMemory(int size)
        {
            if (memoryHandle.Size < size)
            {
				memoryHandle = new MemoryHandle(size);
			}

			return (IntPtr)memoryHandle.Memory;
		}

		public void SetHook(HookInfo info)
		{
			UnHook();

			HookInfo = info;
			HookAttribute hook = info.GetHookAttribute();

			var callable = (int)info.GetCallable();
			Logger.Log("ares hook callable: 0x{0:X}", callable);

			int pMemory = (int)GetMemory(code_call.Length + hook.Size + ASM.Jmp.Length);
			Logger.Log("AresHookTransferStation alloc: 0x{0:X}", pMemory);

			if (pMemory != (int)IntPtr.Zero)
			{
				MemoryHelper.Write(pMemory, code_call, code_call.Length);

				MemoryHelper.Write(pMemory + 3, hook.Address);
				ASMWriter.WriteCall(new JumpStruct(pMemory + 0xF, callable));

				var origin_code_offset = pMemory + code_call.Length;

				if (hook.Size > 0)
				{
					code_over = new byte[hook.Size];
					MemoryHelper.Read(hook.Address, code_over, hook.Size);
					MemoryHelper.Write(origin_code_offset, code_over, hook.Size);
				}

				var jmp_back_offset = origin_code_offset + hook.Size;
				ASMWriter.WriteJump(new JumpStruct(jmp_back_offset, hook.Address + hook.Size));

				ASMWriter.WriteJump(new JumpStruct(hook.Address, pMemory));

				ASMWriter.FlushInstructionCache(pMemory, memoryHandle.Size);
			}
		}

		public void UnHook()
        {
			if(code_over != null)
			{
				HookAttribute hook = HookInfo.GetHookAttribute();
				MemoryHelper.Write(hook.Address, code_over, hook.Size);
				ASMWriter.FlushInstructionCache(hook.Address, Math.Max(hook.Size, 5));
				code_over = null;
			}
        }

		private bool disposedValue;
		protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    memoryHandle.Dispose();
                }

				UnHook();
                disposedValue = true;
            }
        }
        ~AresHookTransferStation()
        {
            Dispose(disposing: false);
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
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
