// dllmain.cpp : Defines the entry point for the DLL application.

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files
#include <windows.h>
#include <thread>
#include <string>


struct alignas(16) hookdecl {
    unsigned int hookAddr;
    unsigned int hookSize;
    const char* hookName;
};

#pragma section(".syhks00", read, write)
__declspec(allocate(".syhks00")) hookdecl _hk__PatcherLoader_Action = { 0x7CD810, 0x9, "PatcherLoader_Action" };


auto Action = []() {
    if (AllocConsole()) {
        using System::Reflection::Assembly;
        using System::Activator;
        using System::Console;

        Console::WriteLine("AllocConsole() succeed");
        //Assembly^ assembly = Assembly::Load("DynamicPatcher");
        //Activator::CreateInstance(assembly->GetType("Program"));
        //auto program = gcnew DynamicPatcher::Program();
        DynamicPatcher::Program::Active();
        Console::WriteLine("load succeed");
    }
    else {
        MessageBoxW(NULL, TEXT("alloc console error"), TEXT("PatcherLoader"), MB_OK);
    }
};

typedef DWORD REGISTERS;
extern "C" __declspec(dllexport) DWORD __cdecl PatcherLoader_Action(REGISTERS * R)
{
    std::thread([]()
        {
            std::this_thread::sleep_for(std::chrono::seconds(2));
            Action();
        }
    ).detach();

    return 0;
}

//Handshake definitions
struct SyringeHandshakeInfo
{
    int cbSize;
    int num_hooks;
    unsigned int checksum;
    DWORD exeFilesize;
    DWORD exeTimestamp;
    unsigned int exeCRC;
    int cchMessage;
    char* Message;
};

extern "C" __declspec(dllexport) HRESULT __cdecl SyringeHandshake(SyringeHandshakeInfo * pInfo)
{
    if (pInfo) {
        std::string message = "Patcher Loader Handshake";
        std::copy(message.begin(), message.end(), pInfo->Message);
        return S_OK;
    }
    return E_POINTER;
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

