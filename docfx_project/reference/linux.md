[Player1]: xref:samples#Player1


# Linux Support

## Linux Support [as of 12.29.2016]
We have had all the samples running on Linux under Mono, with the exception of [Player1] which requires DirectX.

But, currently Linux testing and maintenance is not up to snuff.

Really, the next step is to switch from Mono to .NET core, and build a set of unit tests around the samples to verify operation at build time on an appropriate system.
[Sadly both Windows and Linux CI cloud solutions will not be able to run unit tests with hardware mode, because the CI cloud providers don't provide virtual environments capable of  Quick Sync operation. Software fallback will work on Windows, and may work on Linux too]


If you need Linux support, please contact me, as I would invest some time getting stuff ship-shape [in good condition] with an active end-user providing feedback. 