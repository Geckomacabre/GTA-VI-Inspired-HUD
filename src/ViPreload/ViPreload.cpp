// ViPreload.asi: loads DirectWrite, and three DLLs your own music needs, together with the game, before GTA
// starts its hundreds of threads.
//
// Why (Legacy, 2026-11-12, WER dump GTA5.exe.58120.dmp): about 3 s after the scripts load, ViMinimap's renderer
// creates new FontFamily("Arial"). GDI+ answers that by loading every system font through DirectWrite, so
// DWrite.dll was loaded in the middle of the game. DWrite.dll has static TLS, so Windows' loader rebuilt the TLS
// vector of all 382 threads (LdrpHandleTlsData), and on the 64th it freed an already-free block:
// HEAP_CORRUPTION_ACTIONABLE_BlockNotBusy_DOUBLE_FREE_c0000374_ntdll.dll!LdrpHandleTlsData, the game gone.
//
// This ASI has DWrite.dll as a static import (the reference below), so the ASI loader's LoadLibrary of this file
// brings DWrite.dll in at once, at game start, while there are few threads. When the minimap asks for its font,
// DirectWrite is already there and nothing with static TLS gets loaded mid-game.
//
// On Demand (ViExtras MusicPlayer) does the same when it plays its first song. The moment Media Foundation picks
// the decoder, it looks for codecs installed from the Store, and that loads windows.media.dll,
// windows.applicationmodel.dll and AppXDeploymentClient.dll, all three with static TLS. A running GTA doesn't
// have them (checked against the dump's module list; the rest of Media Foundation and the sound stack are already
// loaded). A player's Legacy closed exactly there, the log ending at "music: station MY MUSIC". So they are
// static imports here too. Each is a WinRT DLL, so each exports DllCanUnloadNow (Windows 8 onwards). Three imports
// of the same name need three local names: defs\*.def give each one with EXPORTAS, and build.cmd makes their
// import libraries.
//
// It runs no code: no hooks, no threads, no game access. DllMain only turns off thread notifications.
// Built without the C runtime: build.cmd (cl /GS- + link /NODEFAULTLIB /ENTRY:DllMain).
#include <windows.h>
#include <dwrite.h>

// DllCanUnloadNow of windows.media.dll, windows.applicationmodel.dll and AppXDeploymentClient.dll (defs\*.def).
extern "C" HRESULT STDAPICALLTYPE ViPreloadMediaUnload(void);
extern "C" HRESULT STDAPICALLTYPE ViPreloadAppModelUnload(void);
extern "C" HRESULT STDAPICALLTYPE ViPreloadAppxUnload(void);

// Never called. Taking each function's address is what puts its DLL in this file's import table.
extern "C" __declspec(dllexport) void *ViPreloadDWrite(void)
{
    return (void *)&DWriteCreateFactory;
}

extern "C" __declspec(dllexport) void *ViPreloadMusic(int which)
{
    return which == 0 ? (void *)&ViPreloadMediaUnload
         : which == 1 ? (void *)&ViPreloadAppModelUnload
                      : (void *)&ViPreloadAppxUnload;
}

extern "C" BOOL WINAPI DllMain(HINSTANCE instance, DWORD reason, LPVOID reserved)
{
    (void)reserved;
    if (reason == DLL_PROCESS_ATTACH)
    {
        DisableThreadLibraryCalls(instance);
    }
    return TRUE;
}
