
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
    public class GiantNukeUpScript : BulletScriptable
    {
        public GiantNukeUpScript(BulletExt owner) : base(owner) { }

        double factor = 1.0;
        static Pointer<AnimTypeClass> pAnimType => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("TWLT070");
        Random random = new Random(114514);
        public override void OnUpdate()
        {
            Pointer<BulletClass> pBullet = Owner.OwnerObject;
            BulletTypeExt extType = Owner.Type;

            CoordStruct location = pBullet.Ref.Base.Base.GetCoords();
            location += new CoordStruct(random.Next(100, 500), random.Next(100, 500), random.Next(100, 500));

            Pointer<AnimClass> pLaser = YRMemory.Create<AnimClass>(pAnimType, location);

            pBullet.Ref.Velocity.Z = 70 * factor;
            factor = factor - 0.02;
        }

        static void reset_time()
        {
            if (HouseClass.Player.IsNull == false)
            {
                Pointer<SuperClass> pSuper = HouseClass.Player.Ref.FindSuperWeapon(SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("NukeSpecial"));
                pSuper.Ref.IsCharged = 1;
            }
        }

        [Hook(HookType.WriteBytesHook, Address = 0x7E03E8, Size = 5)]
        static public byte[] set_time()
        {
            reset_time();
            return new byte[] { 0 };
        }
    }
}