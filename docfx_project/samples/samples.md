---
uid: samples
---


# Sample C# Projects

## Preparing your system
We recommend you read the article @prepare_your_system to make sure that your motherboard and CPU are capable, and that the correct Intel HD Graphics drivers are installed. @prepare_your_system

## Primary Samples

<a name="Transcoder1"></a>
### Transcoder1 C# Sample
This sample shows transcoding of video.
By default it takes H.264 input, and writes H.264 output.
It transcodes Big Buck Bunny to a new bitstream.
The class StreamTranscoder is passed a stream and frames are read back one by one and written to disk.

<a name="Decoder1"></a>
### Decoder1 C# Sample
This sample shows decoding of compressed video bitstreams.
By default it takes H.264 input, and writes NV12 output.
It decodes Big Buck Bunny to uncompressed frames.
The class [StreamDecoder] is passed a stream and frames are read back one by one and written to disk.

<a name="Encoder1"></a>
### Encoder1
This sample shows encoding of uncompressed frames.
By default it takes NV12 input, and writes H.264 output.
A class implementing ILowLevelEncoder is passed individual frames, and compressed frames are returned from the encoder engine, one by one.

<a name="Player1"></a>
### Player1 C# Sample
This sample shows how to decode frames to memory, and display them using Direct 3D.
The frames can be decoded to either system-memory or video-memory, the difference is a complex topic and shall be covered in a separate article.
The system-memory or video-memory behavior is controlled by a bool at the start program. This sample only works on Windows, and with systems supporting DirectX 11. It is unclear if this example will work on Windows 7. Win7 has partial DirectX 11 support. It is known to work on Windows 8 and Windows 10.
The StreamDecoder class is used to produce frames, and SharpDX and Direct3D11 is used display the frames one by one.


## Special Case Samples

### _SystemCapabilities C# Sample
This program simply tests to see if you are ready for either:
1. Hardware Quick Sync encode/decode/transcode. 
2. Software fallback encode/decode/transcode. 

### Decoder50 C# Sample
This is an advanced unusual special case sample.
This sample is paired with Encoder50. It demonstrates UYVY to/from MJPEG.
Newer CPUs/IGPs can do YUY2-> MJPEG, and MJPEG->YUY2.
What these two samples demonstrate is encoding UYVY as if it is YUY2.
This means the following:  
1. No color space conversion is needed to or from UYVY. [normally UYVY <-> NV12 or UYVY <-> YUY2 is needed, which can take CPU time]
2. Because the final .jpeg output has color planes reverse, you cannot display the Jpeg normally.
This sample is suitable for showing do to send/receive to Blackmagic Design UYVY cards and products such as the Decklink series.

### Encoder50 C# Sample
This sample is the counterpart to Decoder50, which is for MJPEG to/from UYVY

### Transcoder3 C# Sample
This shows how to directly use the LowLevelTranscoderCSharp class without streams, but instead byte arrays.




