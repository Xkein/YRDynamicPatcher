
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using System.Threading.Tasks;

namespace Scripts
{
    [Serializable]
    public class AAHeatSeeker2 : BulletScriptable
    {
        public AAHeatSeeker2(BulletExt owner) : base(owner) {}
        
        Random random = new Random();
        static ColorStruct innerColor = new ColorStruct(208,10,20);
        static ColorStruct outerColor = new ColorStruct(88, 0, 20);
        static ColorStruct outerSpread = new ColorStruct(10, 10, 10);

        CoordStruct lastLocation;
        int angle;

        public override void OnUpdate()
        {
            Pointer<BulletClass> pBullet = Owner.OwnerObject;
            BulletTypeExt extType = Owner.Type;

            CoordStruct nextLocation = pBullet.Ref.Base.Base.GetCoords();
            nextLocation.Z += 0;
            if (lastLocation == default(CoordStruct)) {
                lastLocation = nextLocation;
            }
            if (lastLocation.DistanceFrom(nextLocation) > 50)
            {
                Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(lastLocation, nextLocation, innerColor, outerColor, outerSpread, 50);
                pLaser.Ref.Thickness = 6;
                pLaser.Ref.IsHouseColor = true;
                //Logger.Log("laser [({0}, {1}, {2}) -> ({3}, {4}, {5})]", lastLocation.X, lastLocation.Y, lastLocation.Z, nextLocation.X, nextLocation.Y, nextLocation.Z);

                lastLocation = nextLocation;
            }

            const int radius = 100;
            pBullet.Ref.Base.Location +=
                new CoordStruct((int)(Math.Cos(angle * Math.PI / 180) * radius), (int)(Math.Sin(angle * Math.PI / 180) * radius), 100)
                 * (pBullet.Ref.Velocity.Z > -20 ? 1 : -1);
            angle = (angle + 25) % 360;
        }
    }
}