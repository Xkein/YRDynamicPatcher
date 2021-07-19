
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
    public class DESO : TechnoScriptable
    {
        public DESO(TechnoExt owner) : base(owner) { }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 1)
            {
                var id = new DecoratorId(1919810);
                if (Owner.Get(id) == null)
                {
                    Owner.CreateDecorator<NuclearLeakage>(id, "Nuclear Leakage Decorator", this);
                }
            }
        }
    }

    [Serializable]
    public class NuclearLeakage : EventDecorator
    {
        public NuclearLeakage(DESO deso)
        {
            Owner.Set(deso.Owner);
        }

        int times = 100;
        ExtensionReference<TechnoExt> Owner;

        static Pointer<WeaponTypeClass> Weapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("RadEruptionWeapon");
        static Pointer<WarheadTypeClass> Warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RadEruptionWarhead");
        
        static Pointer<WeaponTypeClass> Weapon2 => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("TerrorBomb");
        static Pointer<WarheadTypeClass> Warhead2 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("TerrorBombWH");

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (Owner.Get() == null || times-- <= 0)
            {
                Decorative.Remove(this);
                return;
            }

            TechnoExt owner = Owner.Get();

            Pointer<WeaponTypeClass> pWeapon = Weapon;
            Pointer<WarheadTypeClass> pWarhead = Warhead;

            CoordStruct curLocation = owner.OwnerObject.Ref.Base.Base.GetCoords();

            Pointer<BulletClass> pBullet = pWeapon.Ref.Projectile.Ref.
                CreateBullet(owner.OwnerObject.Convert<AbstractClass>(), owner.OwnerObject,
                1, pWarhead, pWeapon.Ref.Speed, pWeapon.Ref.Bright);
            pBullet.Ref.WeaponType = pWeapon;

            pBullet.Ref.MoveTo(curLocation, new BulletVelocity(0, 0, 0));
            pBullet.Ref.Detonate(curLocation);
            
            pWeapon = Weapon2;
            pWarhead = Warhead2;

            pBullet = pWeapon.Ref.Projectile.Ref.
                CreateBullet(owner.OwnerObject.Convert<AbstractClass>(), owner.OwnerObject,
                pWeapon.Ref.Damage, pWarhead, pWeapon.Ref.Speed, pWeapon.Ref.Bright);
            pBullet.Ref.WeaponType = pWeapon;

            pBullet.Ref.MoveTo(curLocation, new BulletVelocity(0, 0, 0));
            pBullet.Ref.Detonate(curLocation);
        }
    }

}