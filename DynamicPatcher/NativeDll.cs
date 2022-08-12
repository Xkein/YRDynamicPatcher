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
        [DllImport("kernel32.dll", EntryPoint = "SetDefaultDllDirectories")]
        private static extern bool _SetDefaultDllDirectories(uint DirectoryFlags);

        [DllImport("kernel32.dll")]
        private extern static IntPtr LoadLibrary(string lpLibFileName);
        [DllImport("kernel32.dll")]
        private extern static IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        [DllImport("kernel32.dll")]
        private extern static bool FreeLibrary(IntPtr hLibModule);

        private const uint LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x1000;
        private const uint LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x100;


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

        /// <summary>
        /// enable additional dll directories
        /// </summary>
        /// <returns></returns>
        public static bool EnableDllDirectories()
        {
            return _SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_DEFAULT_DIRS | LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR);
        }
    }
}
