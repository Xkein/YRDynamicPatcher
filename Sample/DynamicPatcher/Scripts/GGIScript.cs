
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using Extension.Decorators;
using Extension.Utilities;
using System.Threading.Tasks;

namespace Scripts
{
    [Serializable]
    public class GGI : TechnoScriptable
    {
        public GGI(TechnoExt owner) : base(owner) {}

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex) 
        {
            if (weaponIndex == 0)
            {
                if (pTarget.CastToTechno(out Pointer<TechnoClass> pTechno))
                {
                    TechnoExt pTargetExt = TechnoExt.ExtMap.Find(pTechno);
                    if (pTargetExt.Get(MissileFall.ID) == null) {
                        pTargetExt.CreateDecorator<MissileFall>(MissileFall.ID, "Missile Fall Decorator", this);
                    }
                }
            }
            
        }
    }

    [Serializable]
    public class MissileFall : EventDecorator
    {
        public static DecoratorId ID = new DecoratorId(114514);
        public MissileFall(GGI ggi)
        {
            Owner.Set(ggi.Owner);
        }
        int lifetime = 15;
        ExtensionReference<TechnoExt> Owner;
        
        static Random random = new Random(1919810);
        static Pointer<WeaponTypeClass> Weapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("RedEye2");

        int rof = 5;
        public override void OnUpdate()
        {
            if (Owner.Get() == null || lifetime-- <= 0)
            {
                Decorative.Remove(this);
                return;
            }

            if (rof-- > 0) {
                return;
            }
            rof = 5;

            TechnoExt target = Decorative as TechnoExt;
            
            Pointer<WeaponTypeClass> pWeapon = Weapon;
            Pointer<BulletClass> pBullet = pWeapon.Ref.Projectile.Ref.
                CreateBullet(target.OwnerObject.Convert<AbstractClass>(), Owner.Get().OwnerObject,
                 /*pWeapon.Ref.Damage*/10, pWeapon.Ref.Warhead, pWeapon.Ref.Speed, pWeapon.Ref.Bright);
                 
            CoordStruct curLocation = target.OwnerObject.Ref.Base.Base.GetCoords();
            const int radius = 600;
            CoordStruct where = curLocation + new CoordStruct(random.Next(-radius, radius), random.Next(-radius, radius), 2000);
            BulletVelocity velocity = new BulletVelocity(0,0,0);
            pBullet.Ref.MoveTo(where, velocity);
        }
    }

}