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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public word Get16()
		{
			return (word)data;
		}

		/// <summary>Get data by dword.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public dword Get()
		{
			return data;
		}

		/// <summary>Get data by T.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get<T>()
		{
			return Unsafe.As<dword, T>(ref data);
		}

		/// <summary>Set data by dword.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public dword Set(dword value)
		{
			return data = value;
		}

		/// <summary>Set data by T.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set<T>(T value)
		{
			data = Unsafe.As<T, dword>(ref value);
		}

		/// <summary>Set data by word.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set16(word value)
		{
			data = data | value;
		}

		/// <summary>Get high byte of data.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte Get8Hi()
		{
			return (byte)(data >> 8);
		}

		/// <summary>Get low byte of data.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte Get8Lo()
		{
			return (byte)data;
		}

		/// <summary>Set high byte of data.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set8Hi(byte value)
		{
			data = data | ((dword)value << 8);
		}

		/// <summary>Set low byte of data.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set8Lo(byte value)
		{
			data = data | value;
		}
		/// <summary>Get T* by register + offset</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* lea<T>(int byteOffset) where T : unmanaged
		{
			return (T*)lea(byteOffset);
		}

		/// <summary>Get dword by register + offset</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public dword lea(int byteOffset)
		{
			return (dword)(data + byteOffset);
		}

		/// <summary>Get T by [register + offset]</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T At<T>(int byteOffset)
		{
			return Unsafe.Read<T>((void*)lea(byteOffset));
		}

		/// <summary>Set [register + offset] by T</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => origin;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set => origin = value;
		}

		/// <summary>Data of flag register </summary>
		public dword EFLAGS
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => flags;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => flags = value;
		}

		/// <summary>Data of EAX</summary>
		public dword EAX
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _EAX.Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _EAX.Set(value);
		}

		/// <summary>Data of ECX</summary>
		public dword ECX
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _ECX.Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _ECX.Set(value);
		}

		/// <summary>Data of EDX</summary>
		public dword EDX
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _EDX.Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _EDX.Set(value);
		}

		/// <summary>Data of EBX</summary>
		public dword EBX
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _EBX.Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _EBX.Set(value);
		}

		/// <summary>Data of ESP</summary>
		public dword ESP
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _ESP.Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _ESP.Set(value);
		}

		/// <summary>Data of EBP</summary>
		public dword EBP
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _EBP.Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _EBP.Set(value);
		}

		/// <summary>Data of EDI</summary>
		public dword EDI
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _EDI.Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _EDI.Set(value);
		}

		/// <summary>Data of ESI</summary>
		public dword ESI
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _ESI.Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _ESI.Set(value);
		}
		#region REG_SHORTCUTS_XHL(A)

		/// <summary>Data of AX</summary>
		public word AX
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _EAX.Get16();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _EAX.Set16(value);
		}

		/// <summary>Data of AH</summary>
		public byte AH
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _EAX.Get8Hi();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _EAX.Set8Hi(value);
		}

		/// <summary>Data of AL</summary>
		public byte AL
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _EAX.Get8Lo();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _EAX.Set8Lo(value);
		}
		#endregion
		#region REG_SHORTCUTS_XHL(B)

		/// <summary>Data of BX</summary>
		public word BX
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _EBX.Get16();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _EBX.Set16(value);
		}

		/// <summary>Data of BH</summary>
		public byte BH
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _EBX.Get8Hi();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _EBX.Set8Hi(value);
		}

		/// <summary>Data of BL</summary>
		public byte BL
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _EBX.Get8Lo();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _EBX.Set8Lo(value);
		}
		#endregion
		#region REG_SHORTCUTS_XHL(C)

		/// <summary>Data of CX</summary>
		public word CX
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _ECX.Get16();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _ECX.Set16(value);
		}

		/// <summary>Data of CH</summary>
		public byte CH
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _ECX.Get8Hi();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _ECX.Set8Hi(value);
		}

		/// <summary>Data of CL</summary>
		public byte CL
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _ECX.Get8Lo();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _ECX.Set8Lo(value);
		}
		#endregion
		#region REG_SHORTCUTS_XHL(D)

		/// <summary>Data of DX</summary>
		public word DX
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _EDX.Get16();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _EDX.Set16(value);
		}

		/// <summary>Data of DH</summary>
		public byte DH
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _EDX.Get8Hi();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _EDX.Set8Hi(value);
		}

		/// <summary>Data of DL</summary>
		public byte DL
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _EDX.Get8Lo();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _EDX.Set8Lo(value);
		}
		#endregion

		/// <summary>Get T by ESP + offset</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T lea_Stack<T>(int offset)
		{
			dword tmp = _ESP.lea(offset);
			return Unsafe.As<dword, T>(ref tmp);
		}

		/// <summary>Get dword by ESP + offset</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public dword lea_Stack(int offset)
		{
			return _ESP.lea(offset);
		}

		/// <summary>Get T&amp; by ESP + offset</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T ref_Stack<T>(int offset)
		{
			return ref Unsafe.AsRef<T>((void*)lea_Stack(offset));
		}

		/// <summary>Get T by [ESP + offset]</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Stack<T>(int offset)
		{
			return _ESP.At<T>(offset);
		}

		/// <summary>Get dword by [ESP + offset]</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public dword Stack32(int offset)
		{
			return _ESP.At<dword>(offset);
		}

		/// <summary>Get word by [ESP + offset]</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public word Stack16(int offset)
		{
			return _ESP.At<word>(offset);
		}

		/// <summary>Get byte by [ESP + offset]</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte Stack8(int offset)
		{
			return _ESP.At<byte>(offset);
		}

		/// <summary>Get T by [EBP + offset]</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Base<T>(int offset)
		{
			return _EBP.At<T>(offset);
		}

		/// <summary>Set [ESP + offset] by T</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Stack<T>(int offset, T value)
		{
			_ESP.At(offset, value);
		}

		/// <summary>Set [ESP + offset] by word</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Stack16(int offset, word value)
		{
			_ESP.At(offset, value);
		}

		/// <summary>Set [ESP + offset] by byte</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Stack8(int offset, byte value)
		{
			_ESP.At(offset, value);
		}

		/// <summary>Set [EBP + offset] by T</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Base<T>(int offset, T value)
		{
			_EBP.At(offset, value);
		}
	};
}
