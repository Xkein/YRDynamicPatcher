// dllmain.cpp : Defines the entry point for the DLL application.

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
#define _CRT_SECURE_NO_WARNINGS
// Windows Header Files
#include <windows.h>
#include <windef.h>
#include <libloaderapi.h>
#include <shellapi.h>

#include <thread>
#include <string>
#include <array>
#include <filesystem>
#include <format>

#include <hostfxr.h>
#include <coreclr_delegates.h>
#include <nethost.h>

#define STATUS_CODE_SUCCEEDED(status_code) ((static_cast<int>(status_code)) >= 0)


void ShowErrorAndExit(std::wstring error)
{
    MessageBox(NULL, error.c_str(), L"Patcher Loader", MB_OK);
    ExitProcess(0);
}

#define CHECK_STATUS_CODE(code, error_msg) \
    if (auto ret = code; !STATUS_CODE_SUCCEEDED(ret)) ShowErrorAndExit(error_msg)

#define DEBUG_CLR_HOST

void trace(std::string str) {
#ifdef DEBUG_CLR_HOST
    printf(str.c_str());
    putchar('\n');
#endif
}
void trace(std::wstring str) {
#ifdef DEBUG_CLR_HOST
    wprintf(str.c_str());
    putchar('\n');
#endif
}

namespace hostfxr
{
    HMODULE hostfxr_lib;
    hostfxr_initialize_parameters initialize_parameters { sizeof(hostfxr_initialize_parameters), };
    hostfxr_handle handle;

#define DECLARE_HOSTFXR_FN(func) func##_fn func
    DECLARE_HOSTFXR_FN(hostfxr_close);
    DECLARE_HOSTFXR_FN(hostfxr_get_runtime_delegate);
    DECLARE_HOSTFXR_FN(hostfxr_get_runtime_properties);
    DECLARE_HOSTFXR_FN(hostfxr_get_runtime_property_value);
    DECLARE_HOSTFXR_FN(hostfxr_initialize_for_dotnet_command_line);
    DECLARE_HOSTFXR_FN(hostfxr_initialize_for_runtime_config);
    DECLARE_HOSTFXR_FN(hostfxr_main);
    DECLARE_HOSTFXR_FN(hostfxr_main_bundle_startupinfo);
    DECLARE_HOSTFXR_FN(hostfxr_main_startupinfo);
    DECLARE_HOSTFXR_FN(hostfxr_run_app);
    DECLARE_HOSTFXR_FN(hostfxr_set_error_writer);
    DECLARE_HOSTFXR_FN(hostfxr_set_runtime_property_value);

    typedef int32_t(HOSTFXR_CALLTYPE* hostfxr_get_dotnet_environment_info_fn)(
        const char_t* dotnet_root, void* reserved,
        hostfxr_get_dotnet_environment_info_result_fn result,
        void* result_context);
    DECLARE_HOSTFXR_FN(hostfxr_get_dotnet_environment_info);

    void load(HMODULE hostfxr_lib) {
        hostfxr::hostfxr_lib = hostfxr_lib;

#define GET_HOSTFXR_FN(func) \
        func = (func##_fn)GetProcAddress(hostfxr_lib, #func); \
        if (func == nullptr) { \
            ShowErrorAndExit(TEXT("could not get "#func)); return;\
        }

        GET_HOSTFXR_FN(hostfxr_close);
        GET_HOSTFXR_FN(hostfxr_get_runtime_delegate);
        GET_HOSTFXR_FN(hostfxr_get_runtime_properties);
        GET_HOSTFXR_FN(hostfxr_get_runtime_property_value);
        GET_HOSTFXR_FN(hostfxr_initialize_for_dotnet_command_line);
        GET_HOSTFXR_FN(hostfxr_initialize_for_runtime_config);
        GET_HOSTFXR_FN(hostfxr_main);
        GET_HOSTFXR_FN(hostfxr_main_bundle_startupinfo);
        GET_HOSTFXR_FN(hostfxr_main_startupinfo);
        GET_HOSTFXR_FN(hostfxr_run_app);
        GET_HOSTFXR_FN(hostfxr_set_error_writer);
        GET_HOSTFXR_FN(hostfxr_set_runtime_property_value);

        GET_HOSTFXR_FN(hostfxr_get_dotnet_environment_info);
    }

    void load() {
        char_t hostfxr_path[MAX_PATH];
        size_t hostfxr_path_length;
        if (get_hostfxr_path(hostfxr_path, &hostfxr_path_length, nullptr)) {
            ShowErrorAndExit(TEXT("could not find hostfxr.dll"));
        }
        trace(std::wstring(TEXT("hostfxr_path: ")) + hostfxr_path);

        HMODULE hostfxr_lib = LoadLibrary(hostfxr_path);
        if (hostfxr_lib == nullptr) {
            ShowErrorAndExit(std::format(TEXT("could not load {}"), hostfxr_path));
            return;
        }
        hostfxr::load(hostfxr_lib);

        hostfxr::hostfxr_set_error_writer([](const char_t* message) { trace(message); });
    }
}

namespace hostpolicy
{
    typedef hostfxr_set_error_writer_fn corehost_set_error_writer_fn;
    typedef hostfxr_error_writer_fn corehost_error_writer_fn;

    corehost_set_error_writer_fn corehost_set_error_writer;

    void load(HMODULE hostpolicy_lib) {
#define GET_HOSTPOLICY_FN(func) \
        func = (func##_fn)GetProcAddress(hostpolicy_lib, #func); \
        if (func == nullptr) { \
            ShowErrorAndExit(TEXT("could not get "#func)); return;\
        }

        GET_HOSTPOLICY_FN(corehost_set_error_writer);
    }

    void load() {
        HMODULE hostpolicy_lib = GetModuleHandle(TEXT("hostpolicy.dll"));
        if (hostpolicy_lib == nullptr) {
            ShowErrorAndExit(TEXT("could not find hostpolicy.dll"));
            return;
        }

        hostpolicy::load(hostpolicy_lib);

        hostpolicy::corehost_set_error_writer([](const char_t* message) { trace(message); });
    }
}

namespace coreclr {
    load_assembly_and_get_function_pointer_fn load_assembly_and_get_function_pointer;
    get_function_pointer_fn get_function_pointer;
}


namespace boot_env {
    std::filesystem::path host_root;
    std::filesystem::path dotnet_root;
    std::filesystem::path app_path;

    void load() {
        auto current_path = std::filesystem::current_path();
        host_root = current_path / "gamemd.exe";
        dotnet_root = TEXT(R"(C:\Program Files (x86)\dotnet)");
        app_path = current_path / "PatcherActivator.dll";

        hostfxr::initialize_parameters.dotnet_root = dotnet_root.c_str();
        hostfxr::initialize_parameters.host_path = host_root.c_str();

        trace(std::wstring(TEXT("host_root: ")) + boot_env::host_root.c_str());
        trace(std::wstring(TEXT("dotnet_root: ")) + boot_env::dotnet_root.c_str());
        trace(std::wstring(TEXT("app_path: ")) + boot_env::app_path.c_str());
    }
}


void prepare_dotnet_env() {
    auto runtime_config = boot_env::app_path.parent_path() / boot_env::app_path.stem() += ".runtimeconfig.json";
    auto deps = boot_env::app_path.parent_path() / boot_env::app_path.stem() += ".deps.json";

    CHECK_STATUS_CODE(hostfxr::hostfxr_initialize_for_runtime_config(runtime_config.c_str(), &hostfxr::initialize_parameters, &hostfxr::handle),
        std::format(TEXT("hostfxr_initialize_for_runtime_config failed: {:#x}"), (unsigned int)ret));

    hostpolicy::load();

    CHECK_STATUS_CODE(hostfxr::hostfxr_get_runtime_delegate(hostfxr::handle, hostfxr_delegate_type::hdt_load_assembly_and_get_function_pointer, (void**)&coreclr::load_assembly_and_get_function_pointer),
        std::format(TEXT("hostfxr_get_runtime_delegate failed: {:#x}"), (unsigned int)ret));

    CHECK_STATUS_CODE(hostfxr::hostfxr_get_runtime_delegate(hostfxr::handle, hostfxr_delegate_type::hdt_get_function_pointer, (void**)&coreclr::get_function_pointer),
        std::format(TEXT("hostfxr_get_runtime_delegate failed: {:#x}"), (unsigned int)ret));
    
#ifdef DEBUG_CLR_HOST
    size_t prop_count = 114;
    auto keys = new const char_t* [prop_count];
    auto values = new const char_t* [prop_count];
    CHECK_STATUS_CODE(hostfxr::hostfxr_get_runtime_properties(hostfxr::handle, &prop_count, keys, values),
        std::format(TEXT("hostfxr_get_runtime_properties failed: {:#x}"), (unsigned int)ret));

    trace("runtime properties:");
    for (size_t i = 0; i < prop_count; i++)
    {
        trace(std::format(TEXT("{}: {}"), keys[i], values[i]));
    }
#endif
}

void start_program() {
    int argCount;
    const char_t** argList = (const char_t**)CommandLineToArgvW(GetCommandLine(), &argCount);

    //CHECK_STATUS_CODE(hostfxr::hostfxr_run_app(hostfxr::handle), std::format(TEXT("hostfxr_run_app: {:#x}"), (unsigned int)ret));

    MessageBox(NULL, TEXT("Attach Me"), TEXT("PatcherLoader"), MB_OK);
    int (*dlg)() = nullptr;
    //auto typeName = TEXT("DynamicPatcher.Program, DynamicPatcher, Version=2.0.0.0, Culture=neutral, PublicKeyToken=1a18ce02bf7a1a48");
    auto typeName = TEXT("PatcherActivator.Activator, PatcherActivator");
    auto methodName = TEXT("ActivateUnmanaged");
    CHECK_STATUS_CODE(coreclr::load_assembly_and_get_function_pointer(boot_env::app_path.c_str(), typeName, methodName, UNMANAGEDCALLERSONLY_METHOD, nullptr, (void**)&dlg),
        std::format(TEXT("coreclr::load_assembly_and_get_function_pointer failed: {:#x}"), (unsigned int)ret));

    //CHECK_STATUS_CODE(dlg(), TEXT("Activate error"));
    dlg();

    //CHECK_STATUS_CODE(hostfxr::hostfxr_main_startupinfo(argCount, argList, boot_env::host_root.c_str(), boot_env::dotnet_root.c_str(), boot_env::app_path.c_str()), 
    //    std::format(TEXT("run DynamicPatcher error with code: {:#x}"), (unsigned int)ret));

    LocalFree(argList);
}

auto Action = []() {
#ifdef DEBUG_CLR_HOST
    AllocConsole();
    freopen("CONOUT$", "w+t", stdout);
    freopen("CONIN$", "r+t", stdin);
#endif
    boot_env::load();
    hostfxr::load();

    prepare_dotnet_env();

    start_program();

    //FreeLibrary(hostfxr::hostfxr_lib);
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
