// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

#pragma once

// from api 1.0
#define MFXInit                          prefix_MFXInit
#define MFXClose                         prefix_MFXClose
#define MFXQueryIMPL                     prefix_MFXQueryIMPL
#define MFXQueryVersion                  prefix_MFXQueryVersion

#define MFXJoinSession                   prefix_MFXJoinSession
#define MFXDisjoinSession                prefix_MFXDisjoinSession
#define MFXCloneSession                  prefix_MFXCloneSession
#define MFXSetPriority                   prefix_MFXSetPriority
#define MFXGetPriority                   prefix_MFXGetPriority

#define MFXVideoCORE_SetBufferAllocator  prefix_MFXVideoCORE_SetBufferAllocator
#define MFXVideoCORE_SetFrameAllocator   prefix_MFXVideoCORE_SetFrameAllocator
#define MFXVideoCORE_SetHandle           prefix_MFXVideoCORE_SetHandle
#define MFXVideoCORE_GetHandle           prefix_MFXVideoCORE_GetHandle
#define MFXVideoCORE_SyncOperation       prefix_MFXVideoCORE_SyncOperation

#define MFXVideoENCODE_Query             prefix_MFXVideoENCODE_Query
#define MFXVideoENCODE_QueryIOSurf       prefix_MFXVideoENCODE_QueryIOSurf
#define MFXVideoENCODE_Init              prefix_MFXVideoENCODE_Init
#define MFXVideoENCODE_Reset             prefix_MFXVideoENCODE_Reset
#define MFXVideoENCODE_Close             prefix_MFXVideoENCODE_Close
#define MFXVideoENCODE_GetVideoParam     prefix_MFXVideoENCODE_GetVideoParam
#define MFXVideoENCODE_GetEncodeStat     prefix_MFXVideoENCODE_GetEncodeStat
#define MFXVideoENCODE_EncodeFrameAsync  prefix_MFXVideoENCODE_EncodeFrameAsync

#define MFXVideoDECODE_Query             prefix_MFXVideoDECODE_Query
#define MFXVideoDECODE_DecodeHeader      prefix_MFXVideoDECODE_DecodeHeader
#define MFXVideoDECODE_QueryIOSurf       prefix_MFXVideoDECODE_QueryIOSurf
#define MFXVideoDECODE_Init              prefix_MFXVideoDECODE_Init
#define MFXVideoDECODE_Reset             prefix_MFXVideoDECODE_Reset
#define MFXVideoDECODE_Close             prefix_MFXVideoDECODE_Close
#define MFXVideoDECODE_GetVideoParam     prefix_MFXVideoDECODE_GetVideoParam
#define MFXVideoDECODE_GetDecodeStat     prefix_MFXVideoDECODE_GetDecodeStat
#define MFXVideoDECODE_SetSkipMode       prefix_MFXVideoDECODE_SetSkipMode
#define MFXVideoDECODE_GetPayload        prefix_MFXVideoDECODE_GetPayload
#define MFXVideoDECODE_DecodeFrameAsync  prefix_MFXVideoDECODE_DecodeFrameAsync

#define MFXVideoVPP_Query                prefix_MFXVideoVPP_Query
#define MFXVideoVPP_QueryIOSurf          prefix_MFXVideoVPP_QueryIOSurf
#define MFXVideoVPP_Init                 prefix_MFXVideoVPP_Init
#define MFXVideoVPP_Reset                prefix_MFXVideoVPP_Reset
#define MFXVideoVPP_Close                prefix_MFXVideoVPP_Close

#define MFXVideoVPP_GetVideoParam        prefix_MFXVideoVPP_GetVideoParam
#define MFXVideoVPP_GetVPPStat           prefix_MFXVideoVPP_GetVPPStat
#define MFXVideoVPP_RunFrameVPPAsync     prefix_MFXVideoVPP_RunFrameVPPAsync

// from api 1.1
#define MFXVideoUSER_Register            prefix_MFXVideoUSER_Register
#define MFXVideoUSER_Unregister          prefix_MFXVideoUSER_Unregister
#define MFXVideoUSER_ProcessFrameAsync   prefix_MFXVideoUSER_ProcessFrameAsync

// from api 1.10

#define MFXVideoENC_Query                prefix_MFXVideoENC_Query
#define MFXVideoENC_QueryIOSurf          prefix_MFXVideoENC_QueryIOSurf
#define MFXVideoENC_Init                 prefix_MFXVideoENC_Init
#define MFXVideoENC_Reset                prefix_MFXVideoENC_Reset
#define MFXVideoENC_Close                prefix_MFXVideoENC_Close
#define MFXVideoENC_ProcessFrameAsync    prefix_MFXVideoENC_ProcessFrameAsync
#define MFXVideoVPP_RunFrameVPPAsyncEx   prefix_MFXVideoVPP_RunFrameVPPAsyncEx
#define MFXVideoUSER_Load                prefix_MFXVideoUSER_Load
#define MFXVideoUSER_UnLoad              prefix_MFXVideoUSER_UnLoad

// from api 1.11

#define MFXVideoPAK_Query                prefix_MFXVideoPAK_Query
#define MFXVideoPAK_QueryIOSurf          prefix_MFXVideoPAK_QueryIOSurf
#define MFXVideoPAK_Init                 prefix_MFXVideoPAK_Init
#define MFXVideoPAK_Reset                prefix_MFXVideoPAK_Reset
#define MFXVideoPAK_Close                prefix_MFXVideoPAK_Close
#define MFXVideoPAK_ProcessFrameAsync    prefix_MFXVideoPAK_ProcessFrameAsync

// from api 1.13

#define MFXVideoUSER_LoadByPath          prefix_MFXVideoUSER_LoadByPath

// from api 1.14
#define MFXInitEx                        prefix_MFXInitEx
#define MFXDoWork                        prefix_MFXDoWork

// related to audio

// from api 1.8

#define MFXAudioCORE_SyncOperation       prefix_MFXAudioCORE_SyncOperation
#define MFXAudioENCODE_Query             prefix_MFXAudioENCODE_Query
#define MFXAudioENCODE_QueryIOSize       prefix_MFXAudioENCODE_QueryIOSize
#define MFXAudioENCODE_Init              prefix_MFXAudioENCODE_Init
#define MFXAudioENCODE_Reset             prefix_MFXAudioENCODE_Reset
#define MFXAudioENCODE_Close             prefix_MFXAudioENCODE_Close
#define MFXAudioENCODE_GetAudioParam     prefix_MFXAudioENCODE_GetAudioParam
#define MFXAudioENCODE_EncodeFrameAsync  prefix_MFXAudioENCODE_EncodeFrameAsync

#define MFXAudioDECODE_Query             prefix_MFXAudioDECODE_Query
#define MFXAudioDECODE_DecodeHeader      prefix_MFXAudioDECODE_DecodeHeader
#define MFXAudioDECODE_Init              prefix_MFXAudioDECODE_Init
#define MFXAudioDECODE_Reset             prefix_MFXAudioDECODE_Reset
#define MFXAudioDECODE_Close             prefix_MFXAudioDECODE_Close
#define MFXAudioDECODE_QueryIOSize       prefix_MFXAudioDECODE_QueryIOSize
#define MFXAudioDECODE_GetAudioParam     prefix_MFXAudioDECODE_GetAudioParam
#define MFXAudioDECODE_DecodeFrameAsync  prefix_MFXAudioDECODE_DecodeFrameAsync

// from api 1.9

#define MFXAudioUSER_Register            prefix_MFXAudioUSER_Register
#define MFXAudioUSER_Unregister          prefix_MFXAudioUSER_Unregister
#define MFXAudioUSER_ProcessFrameAsync   prefix_MFXAudioUSER_ProcessFrameAsync
#define MFXAudioUSER_Load                prefix_MFXAudioUSER_Load
#define MFXAudioUSER_UnLoad              prefix_MFXAudioUSER_UnLoad

