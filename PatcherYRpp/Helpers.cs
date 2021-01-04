using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
        public ref T Ref { get => ref GetRef(); }
        public ref T GetRef()
        {
            return ref Helpers.GetUnmanagedRef<T>(Value);
        }
        public T Data { get => Ref; }


        //public static Pointer<T> GetObjectPtr(T obj)
        //{
        //    return (Pointer<T>)YRPP.GetObjectPointer(obj);
        //}

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

        static public ref T GetUnmanagedRef<T>(IntPtr ptr, int offset = 0)
        {
            var data = GetSpan<T>(ptr, offset + 1);
            return ref data[offset];
        }

        static public unsafe Span<T> GetSpan<T>(IntPtr ptr, int length)
        {
            return new Span<T>(ptr.ToPointer(), length);
        }

        public static unsafe Pointer<T> GetThisPointer<T>(this T obj) where T : unmanaged
        {
            void* ptr = &obj;
            return (Pointer<T>)(ptr);
        }


        //[DllImport("kernel32.dll", EntryPoint = "MulDiv")]
        //public static extern IntPtr GetObjectPointerExBase(ref object obj, int _1 = 1, int _2 = 1);
        //public static IntPtr GetObjectPointerEx(object obj)
        //{
        //    const int magic_offset = 0x54;
        //    Pointer<IntPtr> tmp = GetObjectPointerExBase(ref obj) + magic_offset;
        //    return tmp.Data;
        //}

        //[UnmanagedFunctionPointer(CallingConvention.Winapi)]
        //public delegate IntPtr GetObjectPointerDelegate<T>(ref T obj, int _1 = 1, int _2 = 2);
        //[UnmanagedFunctionPointer(CallingConvention.Winapi)]
        //public delegate IntPtr StdCall_C(int _0, int _1 = 1, int _2 = 1);

        //[DllImport("kernel32.dll")]
        //public extern static IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        //[DllImport("kernel32.dll")]
        //public static extern IntPtr GetModuleHandle(string lpModuleName);

        //private static IntPtr GetObjectPointerBase = GetProcAddress(GetModuleHandle("kernel32.dll"), "MulDiv");

        //public static unsafe T ForceConvert<T>(object obj)
        //{
        //    var buffer = GetSpan<int>(GetObjectPointerEx(obj), 1);
        //    object sample = Activator.CreateInstance<T>();
        //    Pointer<int> sampleId = GetObjectPointerEx(sample);
        //    buffer[0] = sampleId.Ref;
        //    return (T)obj;
        //}


        //public static Pointer<T> GetObjectPointer<T>(ref T obj)
        //{
        //    var dlg = Marshal.GetDelegateForFunctionPointer<StdCall_C>(GetObjectPointerBase);
        //    Pointer<GetObjectPointerDelegate<T>> dlgPtr = Marshal.GetFunctionPointerForDelegate((Delegate)dlg);
        //    GetObjectPointerDelegate<T> tmp = new GetObjectPointerDelegate<T>((ref T _0, int _1, int _2) => { return IntPtr.Zero; });
        //    var dlgT = ForceConvert<GetObjectPointerDelegate<T>>(dlgPtr);
        //    IntPtr ptr = dlgT.Invoke(ref obj);
        //    return GetUnmanagedRef<Pointer<T>>(ptr);
        //}
    }
}
