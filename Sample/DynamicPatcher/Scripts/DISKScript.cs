
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using System.Threading.Tasks;

namespace Test
{
    public class Disk : TechnoScriptable
    {
        public Disk(TechnoExt owner) : base(owner) {}
        
        Random random = new Random();
        static ColorStruct innerColor = new ColorStruct(11,45,14);
        static ColorStruct outerColor = new ColorStruct(19, 19, 810);
        static ColorStruct outerSpread = new ColorStruct(10, 10, 10);


        [DllImport("Ares0A.dll")]
        static public extern IntPtr DrawLaser(CoordStruct sourceCoords, CoordStruct targetCoords,
            ColorStruct innerColor, ColorStruct outerColor, ColorStruct outerSpread, int duration, int thinkness);
        [DllImport("Ares0A.dll")]
        static public extern DamageAreaResult DamageArea(CoordStruct Coords, int Damage, /*Pointer<TechnoClass>*/IntPtr SourceObject, IntPtr WH,
            bool AffectsTiberium, IntPtr SourceHouse);
        [DllImport("Ares0A.dll")]
        static public extern void FlashbangWarheadAt(int Damage, IntPtr WH, CoordStruct coords, bool Force = false, SpotlightFlags CLDisableFlags = SpotlightFlags.None);

        int angle;
        int frames;
        static Pointer<WarheadTypeClass> pWH = WarheadTypeClass.Find("BlimpHEEffect");

        public void OnUpdate()
        {
            Pointer<TechnoClass> pTechno = Owner.OwnerObject;
            TechnoTypeExt extType = Owner.Type;

            CoordStruct curLocation = pTechno.Ref.Base.Base.GetCoords();
            
            int height = pTechno.Ref.Base.GetHeight();

            Action<int, int> Draw = (int start, int count) => {
                const double radius = 1145.14;
                int increasement = 360 / count;
                CoordStruct from = curLocation;
                for (int i = 0; i < count; i++) {
                    double x = radius * Math.Cos((start + i * increasement) * Math.PI / 180);
                    double y = radius * Math.Sin((start + i * increasement) * Math.PI / 180);
                    CoordStruct to = curLocation + new CoordStruct((int)x, (int)y, -height);
                    Pointer<LaserDrawClass> pLaser = DrawLaser(from, to, innerColor, outerColor, outerSpread, 8, 10);
                    pLaser.Ref.IsHouseColor = true;
                    
                    if(frames > 300) {
                        int damage = 11;
                        DamageArea(to, damage, pTechno, pWH, false, pTechno.Ref.Owner);
                        FlashbangWarheadAt(damage, pWH, to);
                    }
                    else {
                        frames++;
                    }
                }
            };

            Draw(angle, 3);
            angle = (angle + 4) % 360;
        }
    }
}