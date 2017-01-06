
##Converting Frame Formats

<a name="fourcc.conversion.cpu"></a>
### CPU based FourCC frame format conversion
GPU based format conversion can be simpler to use than GPU format conversion, but can have the disadvantage of utilizing CPU cycles in situations where you are trying to use the CPU for other things.


<a name="fourcc.conversion.gpu"></a>
### GPU based FourCC frame format conversion
GPU based format conversion can have the advantage of higher performance, and little or no impact upon CPU utilization. It can be much tricker to use than CPU format conversion also.

The primary FourCC used by the GPU is NV12, the GPU can also convert NV12 to and from a number of other formats
Other FourCC formats, such as UYVY, RGB4, YUY2 are sometimes available for Quick Sync functions. Some of this may depend on your CPU/graphics-generation. Your ability to use this from the LVSDK will depend on the functions you use, and what has been supported.

You can use the [Player1 Sample] to see how the sample configures the [StreamDecoder] class to do GPU based conversion from the internal NV12 format to RGB4 format.
