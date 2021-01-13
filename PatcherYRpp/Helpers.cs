using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace PatcherYRpp
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Pointer<T>
    {
        public Pointer(int value)
        {
            Value = (IntPtr)value;
        }
        public Pointer(long value)
        {
            Value = (IntPtr)value;
        }
        public unsafe Pointer(void* value)
        {
            Value = (IntPtr)value;
        }
        public unsafe Pointer(IntPtr value)
        {
            Value = value;
        }

        public IntPtr Value;
        public unsafe ref T Ref { get => ref Unsafe.AsRef<T>(Value.ToPointer()); }
        public unsafe T Data { get => Unsafe.Read<T>(Value.ToPointer()); set => Unsafe.Write(Value.ToPointer(), value); }
        public unsafe ref T this[int index] { get => ref Unsafe.Add(ref Unsafe.AsRef<T>(Value.ToPointer()), index); }

        public Pointer<TTo> Convert<TTo>()
        {
            return new Pointer<TTo>(Value);
        }

        public static unsafe Pointer<T> AsPointer(ref T obj)
        {
            return new Pointer<T>(Unsafe.AsPointer(ref obj));
        }

        public static bool operator ==(Pointer<T> value1, Pointer<T> value2) => value1.Value == value2.Value;
        public static bool operator !=(Pointer<T> value1, Pointer<T> value2) => value1.Value != value2.Value;
        public override int GetHashCode() => Value.GetHashCode();
        public override bool Equals(object obj) => Value.Equals(((Pointer<T>)obj).Value);
        public override string ToString() => Value.ToString();

        public static explicit operator int(Pointer<T> value) => (int)value.Value;
        public static unsafe explicit operator void*(Pointer<T> value) => (void*)value.Value;
        public static explicit operator long(Pointer<T> value) => (long)value.Value;
        public static implicit operator IntPtr(Pointer<T> value) => value.Value;

        public static unsafe explicit operator Pointer<T>(void* value) => new Pointer<T>(value);
        public static explicit operator Pointer<T>(int value) => new Pointer<T>(value);
        public static explicit operator Pointer<T>(long value) => new Pointer<T>(value);
        public static implicit operator Pointer<T>(IntPtr value) => new Pointer<T>(value);
    }
    public static class Helpers
    {

        static public unsafe ref T GetUnmanagedRef<T>(IntPtr ptr, int offset = 0)
        {
            return ref new Pointer<T>(ptr)[offset];
        }

        static public unsafe Span<T> GetSpan<T>(IntPtr ptr, int length)
        {
            return new Span<T>(ptr.ToPointer(), length);
        }

        public static TTo ForceConvert<TFrom, TTo>(TFrom obj)
        {
            return Unsafe.As<TFrom, TTo>(ref obj);
        //    var ptr = new Pointer<TTo>(Pointer<TFrom>.AsPointer(ref obj));
        //    return ptr.Ref;
        }
    }
}
