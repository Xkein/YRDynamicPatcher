
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
  - Write bytes to address
- Recoverable Hook
- Recoverable Hook from Exception (if caught)
- Specified Module Hook
- Dynamic Compile & Syringe Technique
- Hook Conflict Detection


Examples
--------

![dynamic_change](https://github.com/Xkein/Images/blob/master/DynamicPatcher/dynamic_change.gif?raw=true)

![recovery_hook](https://github.com/Xkein/Images/blob/master/DynamicPatcher/recovery_hook.gif?raw=true)

![try-catch](https://github.com/Xkein/Images/blob/master/DynamicPatcher/try-catch.gif?raw=true)

![fire_update_script](https://github.com/Xkein/Images/blob/master/DynamicPatcher/fire_update_script.gif?raw=true)

![event_decorator](https://github.com/Xkein/Images/blob/master/DynamicPatcher/event_decorator.gif?raw=true)

![runtime_ini_edit](https://github.com/Xkein/Images/blob/master/DynamicPatcher/runtime_ini_edit.gif?raw=true)

Quick Use
--------
1. Download newest DynamicPatcher from [Releases (Recommended)](https://github.com/Xkein/YRDynamicPatcher/releases) or [Actions](https://github.com/Xkein/YRDynamicPatcher/actions).
2. Unzip to game folder.
3. Open config file `DynamicPatcher\dynamicpatcher.config.json` and set `hide_console` to false, in order to check if DynamicPatcher work.
   - You can try something mentioned in Release.
4. Run game by Ares's `Syringe`.

If DynamicPatcher not work, check the below:
- Runtime [VC++ Redistributable 2015 - 2022 x86](https://aka.ms/vs/17/release/VC_redist.x86.exe) and [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48)
- Run as Administrator

Usage
--------
Put `PatcherLoader.dll` and `DynamicPatcher.dll` on your YR directory and launch Syringe targeting your YR executable (usually `gamemd.exe`).

Create the directory `DynamicPatcher` and put `dynamicpatcher.config.json` & `compiler.config.json` on it.

Create the directory `DynamicPatcher\Libraries` and put necessary assembly on it.

Everythings could be gained from released files.(recommend)

Start-up
--------
DP includes some function of syringe, but at present it needs syringe to activate. In the future, we can activate DP through PatcherLauncher.

The patcher will search exist files at first or detected every file changes later. Next compile the file and syringe.

**Patcher will check the file time between code files and compiled assemblies and dependencies to skip unnecessary compile.

** If you meet some stranger problem, try deleting the directory `DynamicPatcher\Build`.

Configuration
--------
the file `DynamicPatcher\dynamicpatcher.config.json` explanation:

`try_catch_callable` : try-catch invoke a hook function.

`force_gc_collect` : forces garbage collection per 10s.

`hide_console` : hide the console during start-up.

`copy_logs` : backup log files.


the file `DynamicPatcher\compiler.config.json` explanation:

`references` : the assemblies it referenced.

`preprocessor_symbols` : define preprocessor symbols.

`show_hidden` : show hidden message of compiler

`load_temp_file_in_memory` : load temp file into memory. set false if want to Save & Load. set true if want to modify code dynamically.

`emit_pdb` : emit pdb message

`force_compile` : force compile all files at start-up.

`pack_assembly` : pack/unpack builded assemblies for release.


Release Mode
--------
1. Set `pack_assembly` to true and `load_temp_file_in_memory` to false (`DynamicPatcher\Packages` needed).
2. Run game with Debug mode(DynamicPatcher.dll).
3. Cleaning: Remove `DynamicPatcher\Build`, `DynamicPatcher\Logs` and `DynamicPatcher\patcher.log`
4. Use DynamicPatcher_RELEASE.dll instead of DynamicPatcher.dll in release version of your mod


Hook
--------
Each hook are writed like the below

``` csharp
namespace PatcherSample
{
    public class HookTest
    {
        [Hook(HookType.AresHook, Address = 0x6FCFA0, Size = 5)]
        static public unsafe UInt32 ShowFirer(REGISTERS* R)
        {
            ref TechnoClass rTechno = ref ((Pointer<TechnoClass>)R->ESI).Ref;
            ref TechnoTypeClass rType = ref rTechno.Type.Ref;
            ref HouseClass rHouse = ref rTechno.Owner.Ref;
            unsafe
            {
                string ID = rType.Base.GetUIName();
                string HouseID = rHouse.Type.Ref.Base.GetUIName();
                Logger.Log("{0}({1}) fired", ID, HouseID);
            };
            int rof = 1919810;
            if (rTechno.Owner == HouseClass.Player)
            {
                rof = new Random().Next(0, 50);
            }
            else
            {
                rof = new Random().Next(114, 514);
            }
            Logger.Log("next ROF: " + rof);
            R->EAX = (uint)rof;
            Logger.Log("");

            return 0x6FCFBE;
        }
        
        static public object writebytesfunc() => new byte[]{0x11,0x45,0x14,0x19,0x19,0x81};
		public delegate object writebytesdlg();

        [Hook(HookType.WriteBytesHook, Address = 0x7E03E0, Size = 8)]
        static public writebytesdlg writebytestest = writebytesfunc;
        
        static public object errorlogtestfunc() => throw new InvalidOperationException("you can't call this function.");
		public delegate object errorlogtestdlg();

        [Hook(HookType.WriteBytesHook, Address = 0x7E03F0, Size = 5)]
        static public errorlogtestdlg errorlogtest = errorlogtestfunc;
    }
}
```
- HookAttribute Target
  - Method
  - Field
  - Property

[DynamicPatcher based Extensions](https://github.com/Xkein/DPExtension-Dionysus)
--------
The extension is divided into 2 parts —— dynamic and static.

Dynamic means that you can edit when game running.

- Dynamic
  1. Hooks
  2. ...
   
- Static (Projects included in solution file named by '.sln')
  1. APIs (Many helpers)
  2. Structure Definitions
  3. Managers
  4. ...

[YRPP](https://github.com/Xkein/PatcherYRpp)
--------
YRPP is a static part of Extension.

It give some game structure and helpers in C# style.

Legal
-----
This project has no direct affiliation with Electronic Arts Inc. Command & Conquer, Command & Conquer Red Alert 2, Command & Conquer Yuri's Revenge are registered trademarks of Electronic Arts Inc. All Rights Reserved.

Support Me
-----
[Patreon](https://www.patreon.com/Xkein)

[Alipay](https://github.com/Xkein/Images/blob/master/SupportMe/alipay.jpg?raw=true)

[WeChat](https://github.com/Xkein/Images/blob/master/SupportMe/wechat.png?raw=true)