
using System;
using System.IO;
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

namespace Miscellaneous
{
    public class DynamicLoadINI
    {
        static Semaphore semaphore = new Semaphore(0, 1);
        [Hook(HookType.AresHook, Address = 0x48CE9C, Size = 5)]
        static public unsafe UInt32 Synchronize(REGISTERS* R)
        {
            semaphore.Release();
            semaphore.WaitOne();
            return 0;
        }

        static CodeWatcher iniWatcher;

        [Hook(HookType.WriteBytesHook, Address = 0x7E03E8, Size = 5)]
        static public byte[] Watch()
        {
            iniWatcher = new CodeWatcher(AppDomain.CurrentDomain.BaseDirectory, "*.INI");
            Logger.LogWarning("watching INI files at {0}", AppDomain.CurrentDomain.BaseDirectory);
            Logger.LogWarning("remove the this hook if unneed.");
            iniWatcher.StartWatchPath();

            iniWatcher.OnCodeChanged += (object sender, FileSystemEventArgs e) =>
            {
                string path = e.FullPath;

                if (Path.GetFileNameWithoutExtension(path).ToLower() == "ra2md")
                {
                    return;
                }

                Logger.Log("");
                Logger.Log("detected file {0}: {1}", e.ChangeType, path);
                // wait for editor releasing
                var time = TimeSpan.FromSeconds(1.0);
                Logger.Log("sleep: {0}s", time.TotalSeconds);
                Thread.Sleep(time);

                Pointer<CCINIClass> pINI = IntPtr.Zero;
                Pointer<CCFileClass> pFile = IntPtr.Zero;
                try
                {
                    pINI = YRMemory.Create<CCINIClass>();
                    pFile = YRMemory.Create<CCFileClass>(path);
                    pINI.Ref.ReadCCFile(pFile);

                    ref var typeArray = ref AbstractTypeClass.ABSTRACTTYPE_ARRAY.Array;

                    Logger.Log("waiting for the end of game frame.");
                    semaphore.WaitOne();
                    Logger.Log("reloading.");
                    for (int i = 0; i < typeArray.Count; i++)
                    {
                        var pItem = typeArray[i].Convert<AbstractTypeClass>();
                        switch (pItem.Ref.Base.WhatAmI())
                        {
                            case AbstractType.AircraftType:
                            case AbstractType.AnimType:
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
                                //Logger.Log("{0} is reloading.", pItem.Ref.GetID());
                                pItem.Ref.LoadFromINI(pINI);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.PrintException(ex);
                }
                finally
                {
                    YRMemory.Delete(pFile);
                    YRMemory.Delete(pINI);
                    semaphore.Release();
                }

                Logger.Log("{0} reloaded.", path);
            };

            return new byte[] { 0 };
        }
    }
}