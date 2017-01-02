# Reference Help Topics

## Intel Quick Sync Video
On most modern Intel consumer CPUs, and specific Xeon server CPUs, there is often an integrated GPU inside the CPU. The integrated GPU may act as a video card, and can provide video related functions.

In particular Intel's Quick Sync Video provides functionality for encoding, decoding, and transcoding of video elementary streams such as: 
- HEVC, H.265, AVC, H.264, MPEG-2, VP9, VC-1, MVC, JPEG, and MJPEG

The Lime Video SDK bring Quick Sync Video functions to languages like C#, F#, VB.NET on platforms that .NET can run on.

Throughout this documentation GPU will be used to refer to Intel's integrated GPU inside the CPU chip.

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
FourCC formats are a system of naming and characterizing uncompressed video frame formats. Link: [Wiki-FourCC](https://en.wikipedia.org/wiki/FourCC)

Normally, "NV12" is the primary internal native format the Quick Sync Hardware uses.

There are two ways that FourCC formats may be converted:
- CPU based FourCC frame format conversion. [Link](#fourcc.conversion.cpu)
- GPU frame format conversion. [Link](#fourcc.conversion.gpu)





##Converting Frame Formats

<a name="fourcc.conversion.cpu"></a>
### CPU based FourCC frame format conversion
GPU based format conversion can be simpler to use than GPU format conversion, but can have the disadvantage of utilizing CPU cycles in situations where you are trying to use the CPU for other things.


<a name="fourcc.conversion.gpu"></a>
### GPU based FourCC frame format conversion
GPU based format conversion can have the advantage of higher performance, and little or no impact upon CPU utilization. It can be much tricker to use than CPU format conversion also.

The primary FourCC used by the GPU is NV12, the GPU can also convert NV12 to and from a number of other formats
Other FourCC formats, such as UYVY, RGB4, YUY2 are sometimes available for Quick Sync functions. Some of this may depend on your CPU/graphics-generation. Your ability to use this from the LVSDK will depend on the functions you use, and what has been supported.

You can use the [Player1 Sample](xref:samples#player1) to see how the sample configures the [StreamDecoder class](xref:LimeVideoSDKQuickSync.StreamDecoder) class to do GPU based conversion from the internal NV12 format to RGB4 format. This con
The @ provides 


<a name="imsdk"></a>
# Intel Media SDK Reference Documentation
The Lime Video SDK uses the Intel Media SDK to get access to the Quick Sync functions inside the CPU and GPU.
Sometimes it can be helpful to review the documentation for the low layer C++ SDK when using the Lime Video SDK.

Our preferred method for viewing the documentation is to download the latest version of the Intel Media SDK, install it,  and to find this file in the distribution: *mediasdk-man.pdf*

You might also find this link useful: https://software.intel.com/en-us/media-sdk/documentation
But our experience is that the online-reference documentation is incomplete, and we believe you will have a better experience with the PDF files from the SDK downloads.

