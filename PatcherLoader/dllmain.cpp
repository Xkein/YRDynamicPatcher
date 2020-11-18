// dllmain.cpp : Defines the entry point for the DLL application.

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files
#include <windows.h>


struct alignas(16) hookdecl {
    unsigned int hookAddr;
    unsigned int hookSize;
    const char* hookName;
};

#pragma section(".syhks00", read, write)
__declspec(allocate(".syhks00")) hookdecl _hk__PatcherLoader_Action = { 0x7CD810, 0x9, "PatcherLoader_Action" };

auto Action = []() {
    using System::Reflection::Assembly;
    using System::Activator;
    using System::Console;
    if (AllocConsole()) {
        Console::WriteLine("AllocConsole() succeed");
        //Assembly^ assembly = Assembly::Load("DynamicPatcher");
        //Activator::CreateInstance(assembly->GetType("Program"));
        auto _ = gcnew DynamicPatcher::Program();
        Console::WriteLine("load succeed");
    }
    else {
        MessageBoxW(NULL, TEXT("alloc console error"), TEXT("PatcherLoader"), MB_OK);
    }
};

typedef DWORD REGISTERS;
extern "C" __declspec(dllexport) DWORD __cdecl PatcherLoader_Action(REGISTERS * R)
{
    Action();
    return 0;
}


BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }

    return TRUE;
}

