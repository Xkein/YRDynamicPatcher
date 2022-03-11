using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPatcher
{
    /// <summary>Represents the dll.</summary>
    public class NativeDll
    {
        private static Dictionary<string, IntPtr> _Cookie = new();

        [DllImport("kernel32.dll", EntryPoint = "AddDllDirectory")]
        private static extern IntPtr _AddDllDirectory([MarshalAs(UnmanagedType.LPWStr)] string newDirectory);
        [DllImport("kernel32.dll", EntryPoint = "RemoveDllDirectory")]
        private static extern bool _RemoveDllDirectory(IntPtr cookie);

        [DllImport("kernel32.dll")]
        private extern static IntPtr LoadLibrary(string lpLibFileName);
        [DllImport("kernel32.dll")]
        private extern static IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        [DllImport("kernel32.dll")]
        private extern static bool FreeLibrary(IntPtr hLibModule);


        /// <summary>
        /// add dll search directory.
        /// </summary>
        /// <param name="newDirectory"></param>
        /// <returns></returns>
        public static bool AddDllDirectory(string newDirectory)
        {
            IntPtr cookie = _AddDllDirectory(newDirectory);

            if (cookie == IntPtr.Zero)
                return false;

            _Cookie[newDirectory] = cookie;
            return true;
        }

        /// <summary>
        /// remove dll search directory.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static bool RemoveDllDirectory(string directory)
        {
            if (_Cookie.TryGetValue(directory, out IntPtr cookie))
            {
                return _RemoveDllDirectory(cookie);
            }

            return false;
        }

    }
}
