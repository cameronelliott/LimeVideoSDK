---
uid: home
title: Lime Video SDK
---

# Introduction to the Lime Video SDK  
####[UNDER CONSTRUCTION 1.1.2017]
## What is the Lime Video SDK?
- This SDK enables **transcoding**, **encoding**, and **decoding** of video bitstreams.
- Intel CPU+GPU hardware accelerated support.
- Software-only fallback support.
- 100% BSD open source software without limitations or binary blobs.
- Compressed formats: HEVC, H.265, AVC, H.264, MPEG-2, VP9, VC-1, MVC, JPEG, and MJPEG.
- Uncompressed formats: RGB3, RGB4, BGR4, BGR3, NV12, I420, IYUV, YUY2, UYVY, YV12, P411, P422


## Major Features
### GPU based coding
The LVSDK supports Intel CPU+GPU based acceleration on Intel CPUs supporting HD Graphics. By using the hardware GPU features encode/decode/transcode can run much faster than it does on the CPU. The SDK utilizes the feature known as Quick Sync quick sync.  
 [Wiki Intel Quick Sync Video](https://en.wikipedia.org/wiki/Intel_Quick_Sync_Video)

### CPU based coding
For non-Intel CPUs, and Intel CPUs not supporting HD Graphics, software fallback is available. Please see @software_fallback for more information about enabling software fallback.

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




