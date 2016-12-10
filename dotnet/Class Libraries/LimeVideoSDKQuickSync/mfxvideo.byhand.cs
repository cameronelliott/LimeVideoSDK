// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
//using System.Drawing;
//using System.Drawing.Imaging;
using System.Diagnostics;


namespace LimeVideoSDKQuickSync
{
    public enum FrameMemoryType : UInt16
    {
        MFX_MEMTYPE_DXVA2_DECODER_TARGET = 0x0010,
        MFX_MEMTYPE_DXVA2_PROCESSOR_TARGET = 0x0020,
        MFX_MEMTYPE_VIDEO_MEMORY_DECODER_TARGET = MFX_MEMTYPE_DXVA2_DECODER_TARGET,
        MFX_MEMTYPE_VIDEO_MEMORY_PROCESSOR_TARGET = MFX_MEMTYPE_DXVA2_PROCESSOR_TARGET,
        MFX_MEMTYPE_SYSTEM_MEMORY = 0x0040,
        MFX_MEMTYPE_RESERVED1 = 0x0080,

        MFX_MEMTYPE_FROM_ENCODE = 0x0100,
        MFX_MEMTYPE_FROM_DECODE = 0x0200,
        MFX_MEMTYPE_FROM_VPPIN = 0x0400,
        MFX_MEMTYPE_FROM_VPPOUT = 0x0800,

        MFX_MEMTYPE_INTERNAL_FRAME = 0x0001,
        MFX_MEMTYPE_EXTERNAL_FRAME = 0x0002,
        MFX_MEMTYPE_OPAQUE_FRAME = 0x0004,

        WILL_READ = 0x1000,
        WILL_WRITE = 0x2000

    }



    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct mfxFrameAllocRequest
    {
        //WARN: array reserved of type UInt32 not included;
        [FieldOffset(16)]
        public mfxFrameInfo Info;
        [FieldOffset(84)]
        public FrameMemoryType Type;
        [FieldOffset(86)]
        public UInt16 NumFrameMin;
        [FieldOffset(88)]
        public UInt16 NumFrameSuggested;
        [FieldOffset(90)]
        public UInt16 reserved2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)] //xxx pack value suspect
    public unsafe struct CustomMemId
    {
        public IntPtr memId;
        public IntPtr memIdStage;
        public UInt16 rw;
    }

    public enum mfxHandleType : Int32
    {
        MFX_HANDLE_DIRECT3D_DEVICE_MANAGER9 = 1,      /* IDirect3DDeviceManager9      */
        MFX_HANDLE_D3D9_DEVICE_MANAGER = MFX_HANDLE_DIRECT3D_DEVICE_MANAGER9,
        MFX_HANDLE_RESERVED1 = 2,
        MFX_HANDLE_D3D11_DEVICE = 3,
        MFX_HANDLE_VA_DISPLAY = 4,
        MFX_HANDLE_RESERVED3 = 5
    }



    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe partial struct mfxFrameSurface1
    {
        //WARN: array reserved of type UInt32 not included;
        [FieldOffset(16)]
        public mfxFrameInfo Info;
        [FieldOffset(88)]
        public mfxFrameData Data;
    }



    [StructLayout(LayoutKind.Explicit, Size = 24, Pack = 1)]
    public unsafe struct mfxExtVPPDoNotUse
    {
        [FieldOffset(0)]
        public mfxExtBuffer Header;
        [FieldOffset(8)]
        public UInt32 NumAlg;
        [FieldOffset(16)]
        public UInt32* AlgList;
    }


    [StructLayout(LayoutKind.Explicit, Size = 24, Pack = 1)]
    public unsafe struct mfxExtVPPDoUse
    {
        [FieldOffset(0)]
        public mfxExtBuffer Header;
        [FieldOffset(8)]
        public UInt32 NumAlg;
        [FieldOffset(16)]
        public UInt32* AlgList;
    }

    [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 1)]
    public unsafe struct mfxFrameId
    {
        [FieldOffset(0)]
        public UInt16 TemporalId;
        [FieldOffset(2)]
        public UInt16 PriorityId;
        [FieldOffset(4)]
        public UInt16 DependencyId;
        [FieldOffset(6)]
        public UInt16 QualityId;
        [FieldOffset(4)]
        public UInt16 ViewId;
    }



    [StructLayout(LayoutKind.Explicit, Size = 64, Pack = 1)]
    public unsafe struct mfxFrameAllocator
    {
        //mfxU32      reserved[4];
        [FieldOffset(16)]
        public IntPtr pthis;
        [FieldOffset(0 * 8 + 24)]
        public IntPtr Alloc;
        [FieldOffset(1 * 8 + 24)]
        public IntPtr Lock;
        [FieldOffset(2 * 8 + 24)]
        public IntPtr Unlock;
        [FieldOffset(3 * 8 + 24)]
        public IntPtr GetHDL;
        [FieldOffset(4 * 8 + 24)]
        public IntPtr Free;

    };

    [StructLayout(LayoutKind.Explicit, Size = 32, Pack = 1)]
    public unsafe struct mfxFrameAllocResponse
    {
        [FieldOffset(16)]
        public IntPtr mids;
        [FieldOffset(16)]
        public IntPtr* mids_ptr;
        [FieldOffset(24)]
        public UInt16 NumFrameActual;
        [FieldOffset(26)]
        public UInt16 reserved2;
    }

    [StructLayout(LayoutKind.Explicit, Size = 208, Pack = 1)]
    public unsafe struct mfxVideoParam
    {
        [FieldOffset(14)]
        public UInt16 AsyncDepth;
        [FieldOffset(16)]
        public mfxInfoMFX mfx;
        [FieldOffset(16)]
        public mfxInfoVPP vpp;
        [FieldOffset(184)]
        public UInt16 Protected;
        [FieldOffset(186)]
        public IOPattern IOPattern;
        [FieldOffset(192)]
        public mfxExtBuffer** ExtParam;
        [FieldOffset(200)]
        public UInt16 NumExtParam;
    }

    [StructLayout(LayoutKind.Explicit, Size = 96, Pack = 1)]
    public unsafe partial struct mfxFrameData
    {
        [FieldOffset(80)]
        public IntPtr MemId;
        [FieldOffset(80)]
        public byte* MemId_ptr;
    }

    [StructLayout(LayoutKind.Sequential, Size = 4)]
    public struct mfxSession
    {
        public IntPtr session;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct mfxSyncPoint
    {
        [FieldOffset(0)]
        public IntPtr sync;
        [FieldOffset(0)]
        public void* sync_ptr;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct mfxExtOpaqueSurfaceAllocHelper
    {
        [FieldOffset(0)]
        public mfxFrameSurface1** Surfaces;
        [FieldOffset(28)]
        public UInt16 Type;
        [FieldOffset(30)]
        public UInt16 NumSurface;
    }

    [StructLayout(LayoutKind.Explicit, Size = 80, Pack = 1)]
    public unsafe struct mfxExtOpaqueSurfaceAlloc
    {
        [FieldOffset(0)]
        public mfxExtBuffer Header;
        [FieldOffset(16)]
        public mfxExtOpaqueSurfaceAllocHelper In;
        [FieldOffset(48)]
        public mfxExtOpaqueSurfaceAllocHelper Out;
    }
}
