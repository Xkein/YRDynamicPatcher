
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

        CoordStruct lastLocation;

        public override void OnUpdate()
        {
            Pointer<TechnoClass> pTechno = Owner.OwnerObject;
            TechnoTypeExt extType = Owner.Type;

            CoordStruct nextLocation = pTechno.Ref.Base.Base.GetCoords();
            nextLocation.Z += 50;
            if (lastLocation.DistanceFrom(nextLocation) > 100)
            {
                Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(lastLocation, nextLocation, innerColor, outerColor, outerSpread, 30);
                pLaser.Ref.Thickness = 10;
                pLaser.Ref.IsHouseColor = true;
                //Logger.Log("laser [({0}, {1}, {2}) -> ({3}, {4}, {5})]", lastLocation.X, lastLocation.Y, lastLocation.Z, nextLocation.X, nextLocation.Y, nextLocation.Z);

                lastLocation = nextLocation;
            }
        }
        
        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex) 
        {
            TechnoTypeExt extType = Owner.Type;
            Pointer<SuperWeaponTypeClass> pSWType = extType.FireSuperWeapon;

            if (pSWType.IsNull == false) {
                Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                Pointer<HouseClass> pOwner = pTechno.Ref.Owner;
                Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(pSWType);

                CellStruct targetCell = MapClass.Coord2Cell(pTarget.Ref.GetCoords());
                //Logger.Log("FireSuperWeapon({2}):0x({3:X}) -> ({0}, {1})", targetCell.X, targetCell.Y, pSWType.Ref.Base.GetID(), (int)pSuper);
                pSuper.Ref.IsCharged = 1;
                pSuper.Ref.Launch(targetCell, true);
                pSuper.Ref.IsCharged = 0;
            }
        }
    }
}