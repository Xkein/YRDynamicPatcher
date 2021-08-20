// dllmain.cpp : Defines the entry point for the DLL application.

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files
#include <windows.h>
#include <thread>
#include <string>
#include <array>

#include <filesystem>
#include "COM.h"
#include "CLR.h"

auto Action = []() {
    if (AllocConsole()) {
        //COM::Load();
        CLR::Init();
        CLR::Load();
    }
    else {
        MessageBoxW(NULL, TEXT("alloc console error"), TEXT("PatcherLoader"), MB_OK);
    }
};

struct alignas(16) hookdecl {
    unsigned int hookAddr;
    unsigned int hookSize;
    const char* hookName;
};

// do action after window created
#pragma section(".syhks00", read, write)
__declspec(allocate(".syhks00")) hookdecl _hk__PatcherLoader_Action = { 0x6BB9D2, 0x6, "PatcherLoader_Action" };
//__declspec(allocate(".syhks00")) hookdecl _hk__PatcherLoader_WaitAction = { 0x48CCC3, 0x5, "PatcherLoader_WaitAction" };

typedef DWORD REGISTERS;

//#include <mutex>
//#include <future>
//std::mutex action_mutex;

extern "C" __declspec(dllexport) DWORD __cdecl PatcherLoader_Action(REGISTERS * R)
{
   /* std::promise<void> p;
    std::future f = p.get_future();
    std::thread([&]()
        {
            p.set_value();
            std::lock_guard guard(action_mutex);
            Action();
        }
    ).detach();

    f.wait();*/
    Action();

    return 0;
}

//extern "C" __declspec(dllexport) DWORD __cdecl PatcherLoader_WaitAction(REGISTERS * R)
//{
//    std::lock_guard guard(action_mutex);
//    return 0;
//}

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
