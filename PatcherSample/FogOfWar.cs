using DynamicPatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherSample
{
    //class FogOfWar
    //{
    //    //6B8E7A = ScenarioClass_LoadSpecialFlags, 6
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x6B8E7A, Size = 6)]
    //    static public extern UInt32 ScenarioClass_LoadSpecialFlags(ref REGISTERS R);
    //    //686C03 = SetScenarioFlags_FogOfWar, 5
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x686C03, Size = 5)]
    //    static public extern UInt32 SetScenarioFlags_FogOfWar(ref REGISTERS R);

    //    //4ADFF0 = MapClass_RevealShroud, 5
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x4ADFF0, Size = 5)]
    //    static public extern UInt32 MapClass_RevealShroud(ref REGISTERS R);
    //    //577EBF = MapClass_Reveal, 6
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x577EBF, Size = 6)]
    //    static public extern UInt32 MapClass_Reveal(ref REGISTERS R);
    //    //586683 = CellClass_DiscoverTechno, 5
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x586683, Size = 5)]
    //    static public extern UInt32 CellClass_DiscoverTechno(ref REGISTERS R);
    //    //4FC1FF = HouseClass_AcceptDefeat_CleanShroudFog, 6
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x4FC1FF, Size = 6)]
    //    static public extern UInt32 HouseClass_AcceptDefeat_CleanShroudFog(ref REGISTERS R);

    //    //4ACE3C = MapClass_TryReshroudCell_SetCopyFlag, 6
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x4ACE3C, Size = 6)]
    //    static public extern UInt32 MapClass_TryReshroudCell_SetCopyFlag(ref REGISTERS R);
    //    //4A9CA0 = MapClass_RevealFogShroud, 7
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x4A9CA0, Size = 7)]
    //    static public extern UInt32 MapClass_RevealFogShroud(ref REGISTERS R);
    //    //486BF0 = CellClass_CleanFog, 9
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x486BF0, Size = 9)]
    //    static public extern UInt32 CellClass_CleanFog(ref REGISTERS R);
    //    //486A70 = CellClass_FogCell, 5
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x486A70, Size = 5)]
    //    static public extern UInt32 CellClass_FogCell(ref REGISTERS R);
    //    //440B8D = BuildingClass_Put_CheckFog, 6
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x440B8D, Size = 6)]
    //    static public extern UInt32 BuildingClass_Put_CheckFog(ref REGISTERS R);
    //    //486C50 = CellClass_ClearFoggedObjects, 6
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x486C50, Size = 6)]
    //    static public extern UInt32 CellClass_ClearFoggedObjects(ref REGISTERS R);

    //    //700779 = TechnoClass_GetCursorOverCell_OverFog, 7
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x700779, Size = 7)]
    //    static public extern UInt32 TechnoClass_GetCursorOverCell_OverFog(ref REGISTERS R);
    //    //6D3470 = TacticalClass_DrawFoggedObject, 8
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x6D3470, Size = 8)]
    //    static public extern UInt32 TacticalClass_DrawFoggedObject(ref REGISTERS R);
    //    //51F983 = InfantryClass_MouseOverCell_OverFog, 5
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x51F983, Size = 5)]
    //    static public extern UInt32 InfantryClass_MouseOverCell_OverFog(ref REGISTERS R);


    //    //5F4B3E = ObjectClass_DrawIfVisible, 6
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x5F4B3E, Size = 6)]
    //    static public extern UInt32 ObjectClass_DrawIfVisible(ref REGISTERS R);
    //    //6FA2B7 = TechnoClass_Update_DrawHidden_CheckFog, 6
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x6FA2B7, Size = 6)]
    //    static public extern UInt32 TechnoClass_Update_DrawHidden_CheckFog(ref REGISTERS R);
    //    //6F5190 = TechnoClass_DrawExtras_CheckFog, 6
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x6F5190, Size = 6)]
    //    static public extern UInt32 TechnoClass_DrawExtras_CheckFog(ref REGISTERS R);
    //    //6924C0 = DisplayClass_ProcessClickCoords_SetFogged, 7
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x6924C0, Size = 7)]
    //    static public extern UInt32 DisplayClass_ProcessClickCoords_SetFogged(ref REGISTERS R);
    //    //48049E = CellClass_DrawTileAndSmudge_CheckFog, 6
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x48049E, Size = 6)]
    //    static public extern UInt32 CellClass_DrawTileAndSmudge_CheckFog(ref REGISTERS R);
    //    //6D6EDA = TacticalClass_Overlay_CheckFog1, A
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x6D6EDA, Size = 0xA)]
    //    static public extern UInt32 TacticalClass_Overlay_CheckFog1(ref REGISTERS R);
    //    //6D70BC = TacticalClass_Overlay_CheckFog2, A
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x6D70BC, Size = 0xA)]
    //    static public extern UInt32 TacticalClass_Overlay_CheckFog2(ref REGISTERS R);
    //    //71CC8C = TerrainClass_DrawIfVisible, 6
    //    [DllImport("Ares0A.dll")]
    //    [Hook(HookType.AresHook, Address = 0x71CC8C, Size = 6)]
    //    static public extern UInt32 TerrainClass_DrawIfVisible(ref REGISTERS R);

    //}
}
