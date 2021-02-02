
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using PatcherSample;
using System.Threading.Tasks;

namespace Test
{
    public class MTNK : TechnoScriptable
    {
        public MTNK(TechnoExt owner) : base(owner) {}

        static MTNK()
        {
            // Task.Run(() =>
            // {
            //     while (true)
            //     {
            //         Logger.Log("Ticked.");
            //         Thread.Sleep(1000);
            //     }
            // });
        }
        
        Random random = new Random();
        static ColorStruct innerColor = new ColorStruct(208,10,203);
        static ColorStruct outerColor = new ColorStruct(88, 0, 88);
        static ColorStruct outerSpread = new ColorStruct(10, 10, 10);


        [DllImport("Ares0A.dll")]
        static public extern IntPtr DrawLaser(CoordStruct sourceCoords, CoordStruct targetCoords,
            ColorStruct innerColor, ColorStruct outerColor, ColorStruct outerSpread, int duration, int thinkness);

        CoordStruct lastLocation;

        public unsafe void OnUpdate()
        {
            Pointer<TechnoClass> pTechno = Owner.OwnerObject;
            TechnoTypeExt extType = Owner.Type;

            CoordStruct nextLocation = pTechno.Ref.Base.Base.GetCoords();
            nextLocation.Z += 50;
            if (lastLocation.DistanceFrom(nextLocation) > 100)
            {
                //var pLaser = YRMemory.Create<LaserDrawClass>(lastLocation, nextLocation, innerColor, outerColor, outerSpread, duration);
                LaserDrawClass* pLaser = (LaserDrawClass*)DrawLaser(lastLocation, nextLocation, innerColor, outerColor, outerSpread, 200, 10);
                pLaser->IsHouseColor = true;
                //Logger.Log("laser [({0}, {1}, {2}) -> ({3}, {4}, {5})]", lastLocation.X, lastLocation.Y, lastLocation.Z, nextLocation.X, nextLocation.Y, nextLocation.Z);

                lastLocation = nextLocation;
            }
        }
    }
}