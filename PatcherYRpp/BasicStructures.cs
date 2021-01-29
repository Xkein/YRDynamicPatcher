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
        public ColorStruct(int r, int g, int b)
        {
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
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

        public static CoordStruct operator -(CoordStruct coord1)
        {
            return new CoordStruct(-coord1.X, -coord1.Y, -coord1.Z);
        }
        public static CoordStruct operator +(CoordStruct coord1, CoordStruct coord2)
        {
            return new CoordStruct(
                 coord1.X + coord2.X,
                 coord1.Y + coord2.Y,
                 coord1.Z + coord2.Z);
        }
        public static CoordStruct operator -(CoordStruct coord1, CoordStruct coord2)
        {
            return new CoordStruct(
                 coord1.X - coord2.X,
                 coord1.Y - coord2.Y,
                 coord1.Z - coord2.Z);
        }
        public static CoordStruct operator *(CoordStruct coord1, double r)
        {
            return new CoordStruct(
                 (int)(coord1.X * r),
                 (int)(coord1.Y * r),
                 (int)(coord1.Z * r));
        }

        public static double operator *(CoordStruct coord1, CoordStruct coord2)
        {
            return coord1.X * coord2.X
                 + coord1.Y * coord2.Y
                 + coord1.Z * coord2.Z;
        }
        //magnitude
        public double Magnitude()
        {
            return Math.Sqrt(MagnitudeSquared());
        }
        //magnitude squared
        public double MagnitudeSquared()
        {
            return this * this;

        }

        public  double DistanceFrom(CoordStruct other)
        {
            return (other - this).Magnitude();
        }

        public static bool operator ==(CoordStruct coord1, CoordStruct coord2)
        {
            return coord1.X == coord2.X && coord1.Y == coord2.Y && coord1.Z == coord2.Z;
        }
        public static bool operator !=(CoordStruct coord1, CoordStruct coord2) => !(coord1 == coord2);

        public override bool Equals(object obj) => this == (CoordStruct)obj;

        public int X;
        public int Y;
        public int Z;
    }
}
