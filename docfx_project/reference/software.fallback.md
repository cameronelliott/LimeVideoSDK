---
uid: software.fallback
---

[Intel Media SDK]: https://software.intel.com/en-us/media-sdk
[Media Server Studio SDK]: https://software.intel.com/en-us/intel-media-server-studio


# Software Fallback

## What is Software Fallback?

Sometimes your need to use the SDK when hardware acceleration is not possible. Reasons for this include: You computer doesn't support Quick Sync, you need to do testing in a virtual machine, etc.

In these cases, you need software fallback mode. If you use the mfxIMPL.MFX_IMPL_AUTO flag inside the SDK, this can occur automatically if there is no integrated GPU, or you don't have your drivers correctly configured. If you use the flag 'mfxIMPL.MFX_IMPL_SOFTWARE' at the appropriate place, you can force the use of software mode to occur.

## Configuring The Availability of Software Fallback

For software fallback to work, a software fallback library from Intel needs to be able on the PATH, this file is named "libmfxsw64.dll".
This library does not currently ship with the LVSDK due to licensing concerns. It might in the future.
The best way to make sure that "libmfxsw64.dll" is available when needed is to follow these steps:

1. Make sure "libmfxsw64.dll" is not already available: 
	1. Open a new Command Window
	2. Run the command: "C:\> where libmfxsw64.dll"
	3. If there "where" command shows paths where the DLL can be found you are ready to go.
2. If is not found on your, system, install the free [Intel Media SDK], or the [Media Server Studio SDK].
3. Open a new Cmd.exe window
    1. Open a new Command Window
    2. Run the command: "C:\> where libmfxsw64.dll"
    3. The output should confirm the path to the DLL, and then, you are ready to use software fallback.
