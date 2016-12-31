# Reference Help Topics

## Intel Quick Sync Video
On most modern Intel consumer CPUs, and specific Xeon server CPUs, there is and integrated graphics processor [IGP], or basically a video card inside the CPU. The IGP is sometimes referred to as a GPU, or graphics processor unit.
Intel Quick Sync Video is a feature usually found with the CPU's with internal IGP's [or GPUs].
In particular Intel Quick Sync Video is the functionality inside the CPU/IGP/GPU which provides the functionality for encoding, decoding, and transcoding of video elementary streams such as: 
- HEVC, H.265, AVC, H.264, MPEG-2, VP9, VC-1, MVC, JPEG, and MJPEG<br>
The Lime Video SDK is a 100% BSD open source license toolkit for .NET languages to bring this functionality to languages like C#, F#, VB.NET on platforms that .NET can run on.

These links can provide some background on Intel Quick Sync Video, and Intel's graphic architecture:
https://en.wikipedia.org/wiki/Intel_Quick_Sync_Video
https://en.wikipedia.org/wiki/Intel_HD_and_Iris_Graphics
https://en.wikipedia.org/wiki/List_of_Intel_graphics_processing_units


## Linux Support [as of 12.29.2016]
We have had all the samples except the Windows/DirectX specific 'Player1' sample running on Linux under Mono. The current state of Linux support at the time of writing this is poor, as the samples should be re-tested, and included in a sample-level unit tests.
If you need Linux support, please contact me, as I would invest some time getting stuff ship-shape [in good condition] with an active end-user providing feedback. 

##Compressed Bitstream Formats
The Quick Sync hardware, and the SDK we use to access it, obstensibly provide support fort he following bitstream/elementary stream formats:
- HEVC, H.265, AVC, H.264, MPEG-2, VP9, VC-1, MVC, JPEG, and MJPEG<br>

But hardware bitstream support really depends upon which generation of Intel HD/Iris Graphics you are running on. If you are running on 9th, or 9.5th generation graphics you can fairly well expect to get hardware support for most formats in the above list. [9th and 9.5th Gen are found in Skylake and Kaby Lake CPUs]
As you go farther back in time you lose some levels of support, for example, Haswell has excellent support for H.264, but there is no or little support for H.265.
This section should be updated with a link to CPU/Intel-Graphics-Generation support by bitstream format if one is ever created or found.
This page talks about Intel Graphics Generations:
https://en.wikipedia.org/wiki/Intel_HD_and_Iris_Graphics


##Uncompressed FOURCC Formats 
"NV12" is the primary internal native format the Quick Sync Hardware uses.
Other FourCC formats, such as UYVY, RGB4, YUY2 are sometimes available for Quick Sync functions. Some of this may depend on your CPU/graphics-generation. Your ability to use this from the LVSDK will depend on the functions you use, and what has been supported.
The @player1_sample provides 

##Resizing Frames 
###Resizing during decoding
###Resizing in system memory
##Converting Frame Formats
###Converting FourCC type during decoding
###Converting FourCC type in system memory

<a name="imsdk"></a>
# Intel Media SDK