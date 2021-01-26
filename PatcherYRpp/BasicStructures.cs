using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherYRpp
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ColorStruct
    {
        public ColorStruct(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public byte R;
        public byte G;
        public byte B;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CoordStruct
    {
        public CoordStruct(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }


        public static bool operator ==(CoordStruct value1, CoordStruct value2) => (value1 != value2) == false;
        public static bool operator !=(CoordStruct value1, CoordStruct value2)
        {
            return value1.X != value2.X && value1.Y != value2.Y && value1.Z != value2.Z;
        }

        public int X;
        public int Y;
        public int Z;
    }
}
