
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using Extension.Decorators;
using Extension.Utilities;
using System.Threading.Tasks;

namespace Miscellaneous
{
    public class DynamicLoadINI
    {
#if REALTIME_INI
        static Semaphore semaphore = new Semaphore(0, 1);
        [Hook(HookType.AresHook, Address = 0x48CE9C, Size = 5)]
        static public unsafe UInt32 Synchronize(REGISTERS* R)
        {
            semaphore.Release();
            semaphore.WaitOne();
            return 0;
        }

        static string FindMainINI(string name)
        {
            Func<Pointer<CCINIClass>, bool> IsMainINI = (Pointer<CCINIClass> pINI) => 
            {
	            string section = "#include";
	            int length = pINI.Ref.GetKeyCount(section);
                INIReader reader = new INIReader(pINI);
                for (int i = 0; i < length; i++)
                {
		            string key = pINI.Ref.GetKeyName(section, i);
                    string sub_name = null;
                    if (reader.ReadNormal(section, key, ref sub_name))
                    {
                        //Logger.Log("sub_name {0}", sub_name);
                        if (string.Compare(sub_name, name, true) == 0)
                        {
                            return true;
                        }
                    }
                }
                return false;
            };

            Dictionary<string, Pointer<CCINIClass>> inis = new() {
                { "Rulesmd.ini", CCINIClass.INI_Rules },
                { "Artmd.ini", CCINIClass.INI_Art },
                { "AImd.ini", CCINIClass.INI_AI }
            };
            
            foreach (var ini in inis)
            {
                if (string.Compare(ini.Key, name, true) == 0 || IsMainINI(ini.Value))
                {
                    return ini.Key;
                }
            }

            return name;
        }

        static CodeWatcher iniWatcher;

        [Hook(HookType.WriteBytesHook, Address = 0x7E03E8, Size = 5)]
        static public unsafe byte[] Watch()
        {
            iniWatcher = new CodeWatcher(AppDomain.CurrentDomain.BaseDirectory, "*.INI");
            iniWatcher.FirstAction = (string path) => {
                Logger.LogWarning("watching INI files at {0}", path);
                Logger.LogWarning("remove preprocessor_symbols->REALTIME_INI if unneed.");
            };
            iniWatcher.StartWatchPath();

            iniWatcher.OnCodeChanged += (object sender, FileSystemEventArgs e) =>
            {
                string path = e.FullPath;

                if (Path.GetFileNameWithoutExtension(path).ToLower() == "ra2md")
                {
                    return;
                }
 
                Logger.Log("");
                Logger.LogWarning("experimental feature REALTIME_INI is working.");
                Logger.Log("detected file {0}: {1}", e.ChangeType, path);
                Thread.Sleep(TimeSpan.FromSeconds(1.0));

                try
                {
                    Pointer<CCINIClass> pINI = IntPtr.Zero;
                    Pointer<CCFileClass> pFile = IntPtr.Zero;
                    Pointer<CCFileClass> pMap = IntPtr.Zero;

                    pINI = YRMemory.Create<CCINIClass>();

                    string ini_name = FindMainINI(Path.GetFileName(path));
                    pFile = YRMemory.Create<CCFileClass>(ini_name);
                    Logger.Log("reloading {0}.", ini_name);
                    pINI.Ref.ReadCCFile(pFile);
                    YRMemory.Delete(pFile);

                    Logger.Log("waiting for the end of game frame.");
                    semaphore.WaitOne();

                    Action ReloadRules = () => {
                        string map_name = ScenarioClass.Instance.FileName;
                        pMap = YRMemory.Create<CCFileClass>(map_name);
                        Logger.Log("reloading {0}.", map_name);
                        pINI.Ref.ReadCCFile(pMap);
                        YRMemory.Delete(pMap);

                        Logger.Log("reloading Types.");
                        ref var typeArray = ref AbstractTypeClass.ABSTRACTTYPE_ARRAY.Array;
                        for (int i = 0; i < typeArray.Count; i++)
                        {
                            var pItem = typeArray[i].Convert<AbstractTypeClass>();
                            switch (pItem.Ref.Base.WhatAmI())
                            {
                                case AbstractType.AircraftType:
                                case AbstractType.BuildingType:
                                case AbstractType.BulletType:
                                case AbstractType.HouseType:
                                case AbstractType.InfantryType:
                                case AbstractType.IsotileType:
                                case AbstractType.OverlayType:
                                case AbstractType.ParticleType:
                                case AbstractType.ParticleSystemType:
                                case AbstractType.Side:
                                case AbstractType.SmudgeType:
                                case AbstractType.SuperWeaponType:
                                //case AbstractType.TerrainType: // which will get crash
                                case AbstractType.UnitType:
                                case AbstractType.VoxelAnimType:
                                case AbstractType.Tiberium:
                                case AbstractType.WeaponType:
                                case AbstractType.WarheadType:
                                    //Logger.Log("{0} is reloading.", pItem.Ref.ID);
                                    pItem.Ref.LoadFromINI(pINI);
                                    break;
                            }
                        }
                    };

                    Action ReloadArt = () => {
                        using var memory = new MemoryHandle(Marshal.SizeOf<CCINIClass>());
                        Logger.Log("storing art data.");
                        Unsafe.CopyBlock((void*)memory.Memory, (void*)CCINIClass.INI_Art, (uint)memory.Size);
                        Logger.Log("writing new art data.");
                        Unsafe.CopyBlock((void*)CCINIClass.INI_Art, (void*)pINI, (uint)memory.Size);

                        Logger.Log("reloading Types.");
                        ref var typeArray = ref AbstractTypeClass.ABSTRACTTYPE_ARRAY.Array;
                        for (int i = 0; i < typeArray.Count; i++)
                        {
                            var pItem = typeArray[i].Convert<AbstractTypeClass>();
                            switch (pItem.Ref.Base.WhatAmI())
                            {
                                case AbstractType.AnimType:
                                    pItem.Ref.LoadFromINI(pINI);
                                    break;
                                case AbstractType.AircraftType:
                                case AbstractType.BuildingType:
                                case AbstractType.BulletType:
                                case AbstractType.HouseType:
                                case AbstractType.InfantryType:
                                case AbstractType.IsotileType:
                                case AbstractType.OverlayType:
                                case AbstractType.ParticleType:
                                case AbstractType.ParticleSystemType:
                                case AbstractType.Side:
                                case AbstractType.SmudgeType:
                                case AbstractType.SuperWeaponType:
                                case AbstractType.UnitType:
                                case AbstractType.VoxelAnimType:
                                case AbstractType.Tiberium:
                                case AbstractType.WeaponType:
                                case AbstractType.WarheadType:
                                    pItem.Ref.LoadFromINI(CCINIClass.INI_Rules);
                                    break;
                            }
                        }
                        Logger.Log("writing old art data back.");
                        Unsafe.CopyBlock((void*)CCINIClass.INI_Art, (void*)memory.Memory, (uint)memory.Size);
                    };

                    if (string.Compare("Artmd.ini", ini_name, true) == 0)
                    {
                        ReloadArt();
                    }
                    else
                    {
                        ReloadRules();
                    }

                    YRMemory.Delete(pINI);
                }
                catch (Exception ex)
                {
                    Logger.PrintException(ex);
                }
                finally
                {
                    semaphore.Release();
                }

                Logger.Log("{0} reloaded.", path);
                Logger.LogWarning("new changes are only be applied to current game.");
            };

            return new byte[] { 0 };
        }
#endif
    }
}