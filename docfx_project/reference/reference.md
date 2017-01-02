[StreamTranscoder]: xref:LimeVideoSDK.QuickSync.StreamTranscoder
[StreamDecoder]: xref:LimeVideoSDK.QuickSync.StreamDecoder  
[LowLevelEncoderCSharp]: LimeVideoSDK.QuickSync.LowLevelEncoderCSharp
[mfxVideoParam]: xref:LimeVideoSDK.QuickSyncTypes.mfxVideoParam
[Player1]: xref:samples
[Intel_Quick_Sync_Video]: https://en.wikipedia.org/wiki/Intel_Quick_Sync_Video
[Intel_HD_and_Iris_Graphics]: https://en.wikipedia.org/wiki/Intel_HD_and_Iris_Graphics
[List_of_Intel_graphics_processing_units]: https://en.wikipedia.org/wiki/List_of_Intel_graphics_processing_units





# Reference Help Topics

## Intel Quick Sync Video
On most modern Intel consumer CPUs, and specific Xeon server CPUs, there is often an integrated GPU inside the CPU. The integrated GPU may act as a video card, and can provide video related functions.

In particular Intel's Quick Sync Video provides functionality for encoding, decoding, and transcoding of video elementary streams such as: 
- HEVC, H.265, AVC, H.264, MPEG-2, VP9, VC-1, MVC, JPEG, and MJPEG

The Lime Video SDK bring Quick Sync Video functions to languages like C#, F#, VB.NET on platforms that .NET can run on.

Throughout this documentation GPU will be used to refer to Intel's integrated GPU inside the CPU chip.

These links can provide some background on Intel Quick Sync Video, and Intel's graphic architecture:

Wiki: [Intel_Quick_Sync_Video]  
Wiki: [Intel_HD_and_Iris_Graphics]  
Wiki: [List_of_Intel_graphics_processing_units]  


## Linux Support [as of 12.29.2016]
We have had all the samples except the Windows/DirectX specific [Player1] sample running on Linux under Mono. The current state of Linux support at the time of writing this is poor, as the samples should be re-tested, and included in a sample-level unit tests.
If you need Linux support, please contact me, as I would invest some time getting stuff ship-shape [in good condition] with an active end-user providing feedback. 




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

You can use the [Player1 Sample] to see how the sample configures the [StreamDecoder] class to do GPU based conversion from the internal NV12 format to RGB4 format. This con
The @ provides 


<a name="imsdk"></a>




