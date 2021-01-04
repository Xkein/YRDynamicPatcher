# YRDynamicPatcher

**Dynamic Patcher** work differently from [Ares](https://github.com/Ares-Developers/Ares). It can dynamic patch Yuri's Revenge by dynamic compiling the C# code and syringe the binary code to Yuri's Revenge.

**Features**
============
- **Hook Type**
  - Ares style hook
  - Direct jump to hook function
  - Direct jump to address
- Recoverable Ares style hook


Examples
--------
**you can put the cs file of `PatcherSample` to directory `DynamicPatcher`**

![dynamic compile and patch](https://i0.hdslb.com/bfs/album/5e67e364a667de6ceaf9522cecbe6bcd7a575a70.gif@518w.gif "dynamic compile and patch")

Usage
--------
Put `PatcherLoader.dll` and `DynamicPatcher.dll` on your YR directory and launch Syringe targeting your YR executable (usually `gamemd.exe`).

Create the directory `DynamicPatcher`, and put `config.json` on it.

`config.json` could be gain from released files.

The patcher will search exist files at first or detected every file changes later. Next compile the file and syringe.

Hook
--------
Each hook are writed like the below

``` csharp
namespace PatcherSample
{
    public class HookTest
    {
        [Hook(HookType.AresHook, Address = 0x6FCFA0, Size = 5)]
        static public UInt32 ShowFirer(ref REGISTERS R)
        {
            var pTechno = (IntPtr)R.ESI;
            var pType = YRPP.GetTechnoType(pTechno);
            IntPtr IDPtr = Marshal.ReadIntPtr(pType, 96);
            string ID = Marshal.PtrToStringUni(IDPtr);
            Console.WriteLine(ID + " fired");
            var rof = new Random().Next(10, 50);
            Console.WriteLine("next ROF: " + rof);
            R.EAX = (uint)rof;
            Console.WriteLine();

            return 0x6FCFBE;
        }

    }
}
```

YRPP
--------
The c# style YRPP is WIP.


Legal
-----

This project has no direct affiliation with Electronic Arts Inc. Command & Conquer, Command & Conquer Red Alert 2, Command & Conquer Yuri's Revenge are registered trademarks of Electronic Arts Inc. All Rights Reserved.
