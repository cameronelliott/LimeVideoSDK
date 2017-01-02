---
uid: Bitstreams
---

#Bitstreams or Elementary Streams

In order to keep the documentation simple, the term 'bitstream' is used interchangeably with the term 'elementary stream'.



##Compressed Bitstream Formats
The very latest versions of Intel CPUs have hardware support for the following bitstreams:
- H.265, H.264, MPEG-2, VP9, VC-1, MVC, and MJPEG

- H.265 is also known as HEVC, H.264 is also known as AVC, and sometimes MJPEG is referred to as JPEG

What hardware bitstream support you have in your CPU really depends upon which generation of internal GPU core your CPU contains. If you are running on 9th, or 9.5th generation graphics you can fairly well expect to get hardware support for all the formats in the above list. [9th and 9.5th Gen are found in Skylake and Kaby Lake CPUs]

As you go farther back in time you lose some levels of support, for example, Haswell has excellent support for H.264, but there is only partial support for H.265.  If you are on Ivy Bridge or Sandy Bridge, there is H.264 support, but no H.265 support whatsoever.

In the future, this section should be updated with a table showing bitstream support by Intel Core generation.

This page talks about Intel Graphics Generations:
https://en.wikipedia.org/wiki/Intel_HD_and_Iris_Graphics