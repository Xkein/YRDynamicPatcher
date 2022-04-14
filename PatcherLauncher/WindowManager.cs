using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherLauncher
{
    public class WindowManager
    {

        public const int HWND_TOP = 0;
        public const int HWND_BOTTOM = 1;
        public const int HWND_TOPMOST = -1;
        public const int HWND_NOTOPMOST = -2;


        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint wFlags);


        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out WindowRect lpRect);

        /// <summary>
        　　/// 设置窗体为TopMost
        　　/// </summary>
        　　/// <param name="hWnd"></param>
        public static void SetTopomost(IntPtr hWnd)
        {
            WindowRect rect = new WindowRect();
            GetWindowRect(hWnd, out rect);
            SetWindowPos(hWnd, (IntPtr)HWND_TOPMOST, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, 0);
        }
    }

    public struct WindowRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}


