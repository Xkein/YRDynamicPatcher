#include "COM.h"

#include <stdio.h>

#include <combaseapi.h>

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
    ptr->Activate();
    //CoUninitialize();
}

#include "Registration.h"

bool COM::Load()
{
    std::filesystem::path current_path = std::filesystem::current_path();
    std::filesystem::path DynamicPatcherDLL = current_path / L"DynamicPatcher.dll";
    Registration::Register(DynamicPatcherDLL);
    ActiveByCOM2();
    Registration::Unregister(DynamicPatcherDLL);
    return true;
}
