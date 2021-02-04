using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

using dword = System.UInt32;
using word = System.UInt16;

namespace DynamicPatcher
{
	/// <summary>Register helper.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Register
	{
		dword data;

		/// <summary>Get data by word.</summary>
		public word Get16()
		{
			return (word)data;
		}

		/// <summary>Get data by dword.</summary>
		public dword Get()
		{
			return data;
		}

		/// <summary>Get data by T.</summary>
		public T Get<T>()
		{
			return Unsafe.As<dword, T>(ref data);
		}

		/// <summary>Set data by dword.</summary>
		public dword Set(dword value)
		{
			return data = value;
		}

		/// <summary>Set data by T.</summary>
		public void Set<T>(T value)
		{
			data = Unsafe.As<T, dword>(ref value);
		}

		/// <summary>Set data by word.</summary>
		public void Set16(word value)
		{
			data = data | value;
		}

		/// <summary>Get high byte of data.</summary>
		public byte Get8Hi()
		{
			return (byte)(data >> 8);
		}

		/// <summary>Get low byte of data.</summary>
		public byte Get8Lo()
		{
			return (byte)data;
		}

		/// <summary>Set high byte of data.</summary>
		public void Set8Hi(byte value)
		{
			data = data | ((dword)value << 8);
		}

		/// <summary>Set low byte of data.</summary>
		public void Set8Lo(byte value)
		{
			data = data | value;
		}
		/// <summary>Get T* by register + offset</summary>
		public T* lea<T>(int byteOffset) where T : unmanaged
		{
			return (T*)lea(byteOffset);
		}

		/// <summary>Get dword by register + offset</summary>
		public dword lea(int byteOffset)
		{
			return (dword)(data + byteOffset);
		}

		/// <summary>Get T by [register + offset]</summary>
		public T At<T>(int byteOffset)
		{
			return Unsafe.Read<T>((void*)lea(byteOffset));
		}

		/// <summary>Set [register + offset] by T</summary>
		public void At<T>(int byteOffset, T value)
		{
			Unsafe.Write((void*)lea(byteOffset), value);
		}
	};

	/// <summary>Represent the context of ares hook</summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct REGISTERS
	{
		private dword origin;
		private dword flags;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public Register _EDI;
        public Register _ESI;
		public Register _EBP;
		public Register _ESP;
		public Register _EBX;
		public Register _EDX;
		public Register _ECX;
		public Register _EAX;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

		/// <summary>Current Hook Address</summary>
		public dword Origin
		{
			get => origin;
			private set => origin = value;
		}

		/// <summary>Data of flag register </summary>
		public dword EFLAGS
		{
			get => flags;
			set => flags = value;
		}

		/// <summary>Data of EAX</summary>
		public dword EAX
		{
			get => _EAX.Get();
			set => _EAX.Set(value);
		}

		/// <summary>Data of ECX</summary>
		public dword ECX
		{
			get => _ECX.Get();
			set => _ECX.Set(value);
		}

		/// <summary>Data of EDX</summary>
		public dword EDX
		{
			get => _EDX.Get();
			set => _EDX.Set(value);
		}

		/// <summary>Data of EBX</summary>
		public dword EBX
		{
			get => _EBX.Get();
			set => _EBX.Set(value);
		}

		/// <summary>Data of ESP</summary>
		public dword ESP
		{
			get => _ESP.Get();
			set => _ESP.Set(value);
		}

		/// <summary>Data of EBP</summary>
		public dword EBP
		{
			get => _EBP.Get();
			set => _EBP.Set(value);
		}

		/// <summary>Data of EDI</summary>
		public dword EDI
		{
			get => _EDI.Get();
			set => _EDI.Set(value);
		}

		/// <summary>Data of ESI</summary>
		public dword ESI
		{
			get => _ESI.Get();
			set => _ESI.Set(value);
		}
		#region REG_SHORTCUTS_XHL(A)

		/// <summary>Data of AX</summary>
		public word AX
		{
			get => _EAX.Get16();
			set => _EAX.Set16(value);
		}

		/// <summary>Data of AH</summary>
		public byte AH
		{
			get => _EAX.Get8Hi();
			set => _EAX.Set8Hi(value);
		}

		/// <summary>Data of AL</summary>
		public byte AL
		{
			get => _EAX.Get8Lo();
			set => _EAX.Set8Lo(value);
		}
		#endregion
		#region REG_SHORTCUTS_XHL(B)

		/// <summary>Data of BX</summary>
		public word BX
		{
			get => _EBX.Get16();
			set => _EBX.Set16(value);
		}

		/// <summary>Data of BH</summary>
		public byte BH
		{
			get => _EBX.Get8Hi();
			set => _EBX.Set8Hi(value);
		}

		/// <summary>Data of BL</summary>
		public byte BL
		{
			get => _EBX.Get8Lo();
			set => _EBX.Set8Lo(value);
		}
		#endregion
		#region REG_SHORTCUTS_XHL(C)

		/// <summary>Data of CX</summary>
		public word CX
		{
			get => _ECX.Get16();
			set => _ECX.Set16(value);
		}

		/// <summary>Data of CH</summary>
		public byte CH
		{
			get => _ECX.Get8Hi();
			set => _ECX.Set8Hi(value);
		}

		/// <summary>Data of CL</summary>
		public byte CL
		{
			get => _ECX.Get8Lo();
			set => _ECX.Set8Lo(value);
		}
		#endregion
		#region REG_SHORTCUTS_XHL(D)

		/// <summary>Data of DX</summary>
		public word DX
		{
			get => _EDX.Get16();
			set => _EDX.Set16(value);
		}

		/// <summary>Data of DH</summary>
		public byte DH
		{
			get => _EDX.Get8Hi();
			set => _EDX.Set8Hi(value);
		}

		/// <summary>Data of DL</summary>
		public byte DL
		{
			get => _EDX.Get8Lo();
			set => _EDX.Set8Lo(value);
		}
		#endregion

		/// <summary>Get T by ESP + offset</summary>
		public T lea_Stack<T>(int offset)
		{
			dword tmp = _ESP.lea(offset);
			return Unsafe.As<dword, T>(ref tmp);
		}

		/// <summary>Get dword by ESP + offset</summary>
		public dword lea_Stack(int offset)
		{
			return _ESP.lea(offset);
		}

		/// <summary>Get T&amp; by ESP + offset</summary>
		public ref T ref_Stack<T>(int offset)
		{
			return ref Unsafe.AsRef<T>((void*)lea_Stack(offset));
		}

		/// <summary>Get T by [ESP + offset]</summary>
		public T Stack<T>(int offset)
		{
			return _ESP.At<T>(offset);
		}

		/// <summary>Get dword by [ESP + offset]</summary>
		public dword Stack32(int offset)
		{
			return _ESP.At<dword>(offset);
		}

		/// <summary>Get word by [ESP + offset]</summary>
		public word Stack16(int offset)
		{
			return _ESP.At<word>(offset);
		}

		/// <summary>Get byte by [ESP + offset]</summary>
		public byte Stack8(int offset)
		{
			return _ESP.At<byte>(offset);
		}

		/// <summary>Get T by [EBP + offset]</summary>
		public T Base<T>(int offset)
		{
			return _EBP.At<T>(offset);
		}

		/// <summary>Set [ESP + offset] by T</summary>
		public void Stack<T>(int offset, T value)
		{
			_ESP.At(offset, value);
		}

		/// <summary>Set [ESP + offset] by word</summary>
		public void Stack16(int offset, word value)
		{
			_ESP.At(offset, value);
		}

		/// <summary>Set [ESP + offset] by byte</summary>
		public void Stack8(int offset, byte value)
		{
			_ESP.At(offset, value);
		}

		/// <summary>Set [EBP + offset] by T</summary>
		public void Base<T>(int offset, T value)
		{
			_EBP.At(offset, value);
		}
	};
}
