---
uid: home
title: Lime Video SDK
---
[BSD open source]: https://en.wikipedia.org/wiki/BSD_licenses


# Introduction to the Lime Video SDK  

#### [UNDER CONSTRUCTION 1.5.2017]
## What is the Lime Video SDK?

- **C# & .NET SDK enables transcoding, encoding, and decoding of video bitstreams.**
- **Hardware accelerated transcoding, encoding, and decoding on most Intel CPUs.**
- **100% BSD open source software without limitations or binary blobs.** 
- **Ready to run C# samples for transcoding, encoding, decoding, and screen-playback.**
- Software-only fallback support for CPUs without hardware acceleration.
- Windows enabled now, Linux enabled soon.
- Compressed formats: HEVC, H.265, AVC, H.264, MPEG-2, VP9, VC-1, MVC, JPEG, and MJPEG.
- Uncompressed formats: RGB3, RGB4, BGR4, BGR3, NV12, I420, IYUV, YUY2, UYVY, YV12, P411, P422
- .NET assemblies enables C#, F#, VB.NET usage out of the box.


## Major Features
### GPU based coding
The LVSDK supports Intel CPU+GPU based acceleration on Intel CPUs supporting HD Graphics. By using the hardware GPU features encode/decode/transcode can run much faster than it does on the CPU. CPU utilization is lower also. The SDK utilizes the feature known as Intel Quick Sync Video.  
 [Wiki Intel Quick Sync Video](https://en.wikipedia.org/wiki/Intel_Quick_Sync_Video)

### CPU based coding
For non-Intel CPUs, and Intel CPUs not supporting HD Graphics, software fallback is available. Please see @software.fallback for more information about enabling software fallback.

### BSD Open Source License
All C# and C++ code in the libraries is part of the SDK, there are no hidden bits, nothing you cannot see.
The [BSD open source] license is one of the most permission, friendly, flexible open source licenses, period.

### Transcoding Video
Transcoding converts compressed bitstreams to compressed bitstreams

Input formats: HEVC, H.265, AVC, H.264, MPEG-2, VP9, VC-1, MVC, JPEG, and MJPEG<br>
Output formats: HEVC, H.265, AVC, H.264, MPEG-2, VP9, VC-1, MVC, JPEG, and MJPEG<br>

### Encoding Video
Encoding converts uncompressed frames to compressed bitstreams or elementary streams.

Input formats: RGB3, RGB4, BGR4, BGR3, NV12, I420, IYUV, YUY2, UYVY, YV12, P411, P422<br>
Output formats: HEVC, H.265, AVC, H.264, MPEG-2, VP9, VC-1, MVC, JPEG, and MJPEG<br>

### Decoding Video
Decoding converts compressed bitstreams to uncompressed frames.

Input formats: HEVC, H.265, AVC, H.264, MPEG-2, VP9, VC-1, MVC, JPEG, and MJPEG<br>
Output formats: RGB3, RGB4, BGR4, BGR3, NV12, I420, IYUV, YUY2, UYVY, YV12, P411, P422<br>

Some redundant terms have been repeated such as HEVC for H.265 to make help searching easier.

### Playing Video
Playing of video is currently supported on Windows, and may be in Linux at some point. [contact us for more info]

Input formats: HEVC, H.265, AVC, H.264, MPEG-2, VP9, VC-1, MVC, JPEG, and MJPEG<br>


## Getting Started With Samples
There are samples for transcoding, encoding, decoding, and playing back of video, among other things.
Please seek the @samples page to get started with the samples.

## Future Work
Currently the SDK provides only the codec building blocks of typical video and media handling systems.
We hope one day to includes functions such as MP4, RTMP, RTSP/RTP, and possible others. Contact us if your company is looking to sponsor projects like these @contact




