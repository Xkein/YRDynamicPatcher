#pragma once
#include <string>
#include <WinUser.h>

void ShowErrorAndExit(std::wstring error)
{
    MessageBox(NULL, error.c_str(), L"Patcher Loader", MB_OK);
    ExitProcess(0);
}

