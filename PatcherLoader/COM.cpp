#include "COM.h"

#include <stdio.h>

#include <combaseapi.h>

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
