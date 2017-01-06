
#Converting Frame Formats

## Comparing CPU and GPU format conversion

The Lime Video SDK supports two types of frame format conversion: CPU-based and GPU-based. The differences are explained below.
The primary uncompressed format used internally is NV12.

<a name="cpu.conversion"></a>
## CPU based FourCC frame format conversion
CPU based format conversion can be simpler to use than GPU format conversion, and provides a larger number of FourCC options, but can have the disadvantage of utilizing CPU cycles in situations where you are trying to use the CPU for other things.
Classes for CPU based conversion are found in the [CPUConvertResize] namespace.
Primary classes are: [NV12ToXXXXConverter], and [NV12FromXXXXConverter].
We recommend you stay with CPU conversion unless you are using one of the current formats described at the end of the next section. (RGB4 or YUY2)


<a name="gpu.conversion"></a>
## GPU based FourCC frame format conversion
GPU based format conversion can have the advantage of higher performance, and little or no impact upon CPU utilization. It can be much tricker to setup and use than CPU format conversion, though. Also, the number of formats supported is much less than what is capable with CPU conversion.

The primary FourCC used by the GPU is NV12, the GPU can also convert NV12 to and from a number of other formats.
Other FourCC formats, such as RGB4 and YUY2 are sometimes available for Quick Sync functions. Some of this may depend on your CPU/graphics-generation. Your ability to use this from the LVSDK will depend on the functions you use, and what is supported.

At this time using RGB4 for StreamDecoder output is supported, and YUY2 for MJPEG encode/decode, anything else has not been extensively tested, and may not work without improvements.


## Format conversion by coding type
- For decoding with GPU conversion, you can use the [Player1] sample to see how the sample configures the [StreamDecoder] class to do GPU based conversion from the internal NV12 format to RGB4 format.
- For decoding with CPU conversion, you can explore the [Decoder1] sample to set how the [NV12ToXXXXConverter] class is used to convert from NV12 to a number of different outputs.
- For encoding, at this time, GPU conversion is not supported, but CPU conversion is. The [Encoder1] sample shows how to use the [NV12FromXXXXConverter] class to convert to NV12 when encoding. 
- For transcoding, frames stay in NV12 format, and format conversion is not needed or useful.


[CPUConvertResize]: xref:LimeVideoSDK.CPUConvertResize
[NV12ToXXXXConverter]: xref:LimeVideoSDK.CPUConvertResize.NV12ToXXXXConverter
[NV12FromXXXXConverter]: xref:LimeVideoSDK.CPUConvertResize.NV12FromXXXXConverter
[StreamDecoder]: xref:LimeVideoSDK.QuickSync.StreamDecoder  
[Player1]: xref:samples#Player1
[Decoder1]: xref:samples#Decoder1
[Encoder1]: xref:samples#Encoder1