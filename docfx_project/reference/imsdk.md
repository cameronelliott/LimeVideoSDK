---
uid: imsdk.docs
---

# Intel Quick Sync Video

## General background on Intel Quick Sync Video
On most modern Intel consumer CPUs, and specific Xeon server CPUs, there is often an integrated GPU inside the CPU. The integrated GPU may act as a video card, and can provide video related functions.

In particular Intel's Quick Sync Video provides functionality for encoding, decoding, and transcoding of video elementary streams such as: 
- HEVC, H.265, AVC, H.264, MPEG-2, VP9, VC-1, MVC, JPEG, and MJPEG

The Lime Video SDK bring Quick Sync Video functions to languages like C#, F#, VB.NET on platforms that .NET can run on.

Throughout this documentation GPU will be used to refer to Intel's integrated GPU inside the CPU chip.

These links can provide some background on Intel Quick Sync Video, and Intel's graphic architecture:

Wiki: [Intel_Quick_Sync_Video]  
Wiki: [Intel_HD_and_Iris_Graphics]  
Wiki: [List_of_Intel_graphics_processing_units]  

[Intel_Quick_Sync_Video]: https://en.wikipedia.org/wiki/Intel_Quick_Sync_Video
[Intel_HD_and_Iris_Graphics]: https://en.wikipedia.org/wiki/Intel_HD_and_Iris_Graphics
[List_of_Intel_graphics_processing_units]: https://en.wikipedia.org/wiki/List_of_Intel_graphics_processing_units



## Intel Media SDK Reference Documentation
The Lime Video SDK uses the Intel Media SDK to get access to the Quick Sync functions inside the CPU and GPU.
Sometimes it can be helpful to review the documentation for the low layer C++ SDK when using the Lime Video SDK.

Our preferred method for viewing the documentation is to download the latest version of the Intel Media SDK, install it,  and to find this file in the distribution: *mediasdk-man.pdf*

You might also find this link useful: https://software.intel.com/en-us/media-sdk/documentation
But our experience is that the online-reference documentation is incomplete, and we believe you will have a better experience with the PDF files from the SDK downloads.



