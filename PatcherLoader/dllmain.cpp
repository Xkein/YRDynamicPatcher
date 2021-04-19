// dllmain.cpp : Defines the entry point for the DLL application.

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files
#include <windows.h>
#include <thread>
#include <string>
#include <array>

#include <mscoree.h>
#include <metahost.h>

#import "mscorlib.tlb" raw_interfaces_only \
    high_property_prefixes("_get","_put","_putref")		\
    rename("ReportEvent", "InteropServices_ReportEvent")  rename("or", "or_")
using namespace mscorlib;

// http://blog.chinaunix.net/uid-26349264-id-3283439.html
// not work good
void ActiveByCLR()
{
    HRESULT hr;
    ICLRMetaHost* pMetaHost = NULL;
    ICLRRuntimeInfo* pRuntimeInfo = NULL;

    auto Cleanup = [&]() {
        if (pMetaHost) {
            pMetaHost->Release();
            pMetaHost = NULL;
        }

        if (pRuntimeInfo) {
            pRuntimeInfo->Release();
            pRuntimeInfo = NULL;
        }
    };

    hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&pMetaHost));
    if (FAILED(hr)) {
        Cleanup();
        return;
    }

    //HANDLE hProcess = GetCurrentProcess();
    hr = pMetaHost->GetRuntime(L"v4.0.30319", IID_PPV_ARGS(&pRuntimeInfo));
    if (FAILED(hr)) {
        Cleanup();
        return;
    }

    BOOL fLoadable;

    hr = pRuntimeInfo->IsLoadable(&fLoadable);
    if (FAILED(hr) || !fLoadable) {
        Cleanup();
        return;
    }

    ICLRRuntimeHost* pClrRuntimeHost = NULL;

    hr = pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_PPV_ARGS(&pClrRuntimeHost));
    if (FAILED(hr)) {
        wprintf(L"ICLRRuntimeInfo::GetInterface failed w/hr 0x%08lx\n", hr);
        Cleanup();
        return;
    }

    hr = pClrRuntimeHost->Start();
    if (FAILED(hr)) {
        wprintf(L"CLR failed to start w/hr 0x%08lx\n", hr);
        Cleanup();
        return;
    }

    /*IUnknownPtr pAppDomainThunk = NULL;
    DWORD domainId;
    hr = pClrRuntimeHost->GetCurrentAppDomainId(&domainId);
    if (FAILED(hr)) {
        Cleanup();
        return;
    }
    _AppDomainPtr pDefaultAppDomain = NULL;
    hr = pAppDomainThunk->QueryInterface(__uuidof(_AppDomain), (VOID**)&pDefaultAppDomain);
    _AssemblyPtr pAssembly = NULL;
    pDefaultAppDomain->Load_2(L"DynamicPatcher", &pAssembly);
    pAssembly->CreateInstance();*/

    DWORD retVal;
    hr = pClrRuntimeHost->ExecuteInDefaultAppDomain(L"DynamicPatcher.dll",
        L"DynamicPatcher.Program",
        L"ActivateFromCLR",
        L"",
        &retVal);

    pClrRuntimeHost->Stop();

    if (FAILED(hr)) {
        Cleanup();
        return;
    }

}


#define DPGUID L"{4BC759CC-5BB6-4E10-A14E-C813C869CE2F}"

// https://docs.microsoft.com/en-us/dotnet/framework/deployment/in-process-side-by-side-execution?redirectedfrom=MSDN
void ActiveByCOM() {
    CoInitialize(NULL);
    CLSID clsid;
    HRESULT hr = -1;
    HRESULT clsidhr = CLSIDFromString(DPGUID, &clsid);
    if (FAILED(clsidhr))
    {
        printf("Failed to construct CLSID from String\n");
    }

    IUnknown* pUnk = NULL;
    hr = CoCreateInstance(clsid, NULL, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&pUnk));
    if (FAILED(hr))
    {
        printf("Failed CoCreateInstance\n");
    }
    else
    {
        pUnk->AddRef();
        printf("Succeeded\n");
    }

    DISPID dispid;
    IDispatch* pActive;
    pUnk->QueryInterface(IID_IDispatch, (void**)&pActive);
    OLECHAR method_name[]{ OLESTR("Activate") };
    hr = pActive->GetIDsOfNames(IID_NULL, (LPOLESTR*)&method_name, 1, LOCALE_SYSTEM_DEFAULT, &dispid);
    DISPPARAMS dispparams;
    dispparams.cNamedArgs = 0;
    dispparams.cArgs = 0;
    VARIANTARG* pvarg = NULL;
    EXCEPINFO* pexcepinfo = NULL;
    WORD wFlags = DISPATCH_METHOD;

    LPVARIANT pvRet = NULL;
    UINT* pnArgErr = NULL;
    hr = pActive->Invoke(dispid, IID_NULL, LOCALE_USER_DEFAULT, wFlags,
        &dispparams, pvRet, pexcepinfo, pnArgErr);
    CoUninitialize();
}

#import "DynamicPatcher.tlb" named_guids raw_interface_only
using namespace DynamicPatcher;

void ActiveByCOM2() {
    //CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
    IPatcherPtr ptr;
    ptr.CreateInstance(CLSID_Program);
    //CoUninitialize();
}

#include <filesystem>
#include "Registration.h"

auto Action = []() {
    if (AllocConsole()) {
        //using System::Reflection::Assembly;
        //using System::Activator;
        //using System::Console;

        //Console::WriteLine("AllocConsole() succeed");
        ////Assembly^ assembly = Assembly::Load("DynamicPatcher");
        ////Activator::CreateInstance(assembly->GetType("Program"));
        ////auto program = gcnew DynamicPatcher::Program();
        // use clr will trigger many int3 breakpoint, which make syringe work bad
        //DynamicPatcher::Program::Activate();
        //Console::WriteLine("load succeed");

        //ActiveByCLR();

        std::filesystem::path current_path = std::filesystem::current_path();
        std::filesystem::path DynamicPatcherDLL = current_path / L"DynamicPatcher.dll";
        Registration::Register(DynamicPatcherDLL);
        //ActiveByCOM();
        ActiveByCOM2();
        Registration::Unregister(DynamicPatcherDLL);
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
__declspec(allocate(".syhks00")) hookdecl _hk__PatcherLoader_Action = { 0x6BDA21, 0x6, "PatcherLoader_Action" };


typedef DWORD REGISTERS;
extern "C" __declspec(dllexport) DWORD __cdecl PatcherLoader_Action(REGISTERS * R)
{
    /*std::thread([]()
        {
            std::this_thread::sleep_for(std::chrono::seconds(2));
            Action();
        }
    ).detach();*/
    Action();

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
