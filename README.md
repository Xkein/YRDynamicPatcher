
# YRDynamicPatcher

[![license](https://www.gnu.org/graphics/gplv3-or-later.png)](https://www.gnu.org/licenses/gpl-3.0.en.html)

**Dynamic Patcher** work differently from [Ares](https://github.com/Ares-Developers/Ares). It can dynamic patch Yuri's Revenge by dynamic compiling the C# code and syringe the binary code to Yuri's Revenge.

It give a completely different way to write our extension features.
We can use it to do something below:
- use the very nice reflection technique in C#
- use "\*.cs" file as a script, which mean it can be a weapon script, map script and so on.
- coding when the game is running, without restarting the game.
- without building a dll file. instead, we can use "\*.cs" file as hotfix.

**Features**
============
- **Hook Type**
  - Ares style hook
  - Direct jump to hook function
  - Direct jump to address
- Recoverable Hook
- Recoverable Hook from Exception
- Dynamic Compile & Syringe Technique


Examples
--------
**you can put the cs file of `PatcherSample` to directory `DynamicPatcher`**

![dynamic_change](Sample/dynamic_change.gif)

![recovery_hook](Sample/recovery_hook.gif)

![try-catch](Sample/try-catch.gif)

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

Configuration
--------
the file `DynamicPatcher\config.json` explanation:

`try_catch_callable` : try-catch invoke a hook function.

`references` : the assemblies it referenced.

`compiler_options` : the command line to compiler


YRPP
--------
The c# style YRPP is WIP.


Legal
-----
This project has no direct affiliation with Electronic Arts Inc. Command & Conquer, Command & Conquer Red Alert 2, Command & Conquer Yuri's Revenge are registered trademarks of Electronic Arts Inc. All Rights Reserved.
