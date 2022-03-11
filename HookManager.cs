using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPatcher
{
    /// <summary>The exception that is thrown when a hook has race with other hooks.</summary>
    public class HookRaceException : InvalidOperationException
    {
        internal HookRaceException(HookInfo info, HookTransferStation station)
            : base(string.Format("{0} has race with [{1}]", info.Member.Name,
                    string.Join(", ", station.HookInfos.Select(item => item.Member.Name))))
        {
            Info = info;
            Station = station;
        }
        HookInfo Info { get; }
        HookTransferStation Station { get; }
    }

    class HookManager
    {
        Dictionary<int, HookTransferStation> transferStations = new Dictionary<int, HookTransferStation>();

        int maxHookSize = ASM.Jmp.Length;

        public void CheckHook(HookInfo info)
        {
            HookAttribute hook = info.HookAttribute;
            int targetAddress = hook.Address;

            var module = Helpers.GetProcessModule(hook.Module);
            if (IsJmpOutOfModule(new JumpStruct((int)module.BaseAddress, targetAddress)))
            {
                Logger.LogWarning("{0} jmp out of module - '{1}'.", info.Member.Name, module.ModuleName);
            }

            CheckHookRace(info);
        }

        public void CheckHookRace(HookInfo info)
        {
            HookAttribute hook = info.HookAttribute;
            int targetAddress = hook.Address;
            int size = Math.Max(ASM.Jmp.Length, hook.Size);

            // check range (targetAddress, targetAddress + size)
            for (int i = 1; i < size; i++)
            {
                if (transferStations.TryGetValue(targetAddress + i, out HookTransferStation station))
                {
                    throw new HookRaceException(info, station);
                }
            }

            // check range (targetAddress - maxHookSize, targetAddress)
            for (int i = 1; i < maxHookSize; i++)
            {
                if (transferStations.TryGetValue(targetAddress - i, out HookTransferStation station))
                {
                    if (station.HookInfos.Count > 0
                        && targetAddress - i + Math.Max(station.MaxHookInfo.HookAttribute.Size, ASM.Jmp.Length) - 1 >= targetAddress)
                    {
                        throw new HookRaceException(info, station);
                    }
                }
            }

            // check jmp in range (targetAddress - ASM.Jmp.Length, targetAddress + size)
            int checkAddress = targetAddress - ASM.Jmp.Length + 1;
            byte[] buffer = new byte[targetAddress - checkAddress + size];
            MemoryHelper.Read(checkAddress, buffer, buffer.Length);
            for (int i = 0; i < buffer.Length; i++)
            {
                if(buffer[i] == ASM.Jmp[0])
                {
                    int offset = 0;
                    MemoryHelper.Read(checkAddress + i + 1, ref offset);

                    JumpStruct jmp = new JumpStruct(checkAddress + i, 0) { Offset = offset };
                    if (jmp.From != targetAddress)
                    {
                        Logger.LogWarning("{0} destroy 'JMP 0x{1:X}' at 0x{2:X}.", info.Member.Name, jmp.To, jmp.From);
                    }
                    else if (IsJmpOutOfModule(jmp))
                    {
                        Logger.LogWarning("{0} overwrite 'JMP 0x{1:X}', which may jump to other module.", info.Member.Name, jmp.To);
                    }
                }
            }
        }

        public bool IsJmpOutOfModule(JumpStruct jmp)
        {
            if (Helpers.GetProcessModuleAt(jmp.From, out var module))
            {
                return Helpers.AddressInModule(jmp.To, module) == false;
            }

            return false;
        }

        public void ApplyHook(MemberInfo member)
        {
            HookAttribute[] hooks = HookInfo.GetHookAttributes(member);
            foreach (var hook in hooks)
            {
                var info = new HookInfo(member, hook);
                Logger.Log("appling {3} hook: {0:X}, {1}, {2:X}", hook.Address, member.Name, hook.Size, hook.Type);

                try
                {
                    CheckHook(info);

                    int key = hook.Address;

                    HookTransferStation station = null;

                    // use old station if exist
                    if (transferStations.ContainsKey(key))
                    {
                        station = transferStations[key];
                        if (station.Match(hook.Type))
                        {
                            Logger.Log("insert hook to key '{0:X}'", key);
                            station.SetHook(info);
                        }
                        else if (station.HookInfos.Count <= 0)
                        {
                            Logger.LogWarning("remove key '{0:X}' because of hook type mismatch. ", key);
                            transferStations.Remove(key);
                            station = null;
                        }
                        else
                        {
                            throw new InvalidOperationException("hook type mismatch.");
                        }
                    }

                    // create new station
                    if (station == null)
                    {
                        Logger.Log("add key '{0:X}'", key);
                        switch (hook.Type)
                        {
                            case HookType.AresHook:
                                station = new AresHookTransferStation(info);
                                break;
                            case HookType.SimpleJumpToRet:
                            case HookType.DirectJumpToHook:
                                station = new JumpHookTransferStation(info);
                                break;
                            case HookType.WriteBytesHook:
                                station = new WriteBytesHookTransferStation(info);
                                break;
                            case HookType.ExportTableHook:
                                station = new ExportTableHookTransferStation(info);
                                break;
                            case HookType.ImportTableHook:
                                station = new ImportTableHookTransferStation(info);
                                break;
                            default:
                                Logger.LogError("found unkwnow hook: " + member.Name);
                                return;
                        }

                        transferStations.Add(key, station);
                    }

                    ASMWriter.FlushInstructionCache(hook.Address, Math.Max(hook.Size, ASM.Jmp.Length));
                    maxHookSize = Math.Max(hook.Size, maxHookSize);
                }
                catch (Exception e)
                {
                    Logger.LogError("hook applied error!");
                    Logger.PrintException(e);
                }
            }
        }

        public void RemoveAssemblyHook(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                MemberInfo[] members = type.GetMembers();
                foreach (MemberInfo member in members)
                {
                    if (member.IsDefined(typeof(HookAttribute), false))
                    {
                        HookAttribute[] hooks = HookInfo.GetHookAttributes(member);
                        foreach (var hook in hooks)
                        {
                            var info = new HookInfo(member, hook);
                            int key = info.GetHookAttribute().Address;

                            if (transferStations.ContainsKey(key))
                            {
                                Logger.Log("remove hook: " + info.Member.Name);
                                if (transferStations[key].HookInfos.Count > 0)
                                {
                                    info = transferStations[key].HookInfos.FirstOrDefault(cur => cur.Member == member && cur.TransferStation == transferStations[key]);
                                    if (info != null)
                                    {
                                        transferStations[key].UnHook(info);
                                    }
                                    else
                                    {
                                        Logger.LogError("remove error! TransferStation does not contain hook {0}!", member.Name);
                                    }
                                }
                                else
                                {
                                    Logger.LogError("remove error! TransferStation has no hook!");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
