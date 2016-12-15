# See Readme.txt for help building

if(WIN32)
    cmake_minimum_required (VERSION 3.6.1)
else()
    cmake_minimum_required (VERSION 2.8.12)
endif()

project (LimeVideoSDKNative)

message("MSVC_C_ARCHITECTURE_ID = ${MSVC_C_ARCHITECTURE_ID}")
message("MSVC_CXX_ARCHITECTURE_ID = ${MSVC_CXX_ARCHITECTURE_ID}")
message("CMAKE_CL_64 = ${CMAKE_CL_64}")
message("CMAKE_CXX_FLAGS_INIT = ${CMAKE_CXX_FLAGS_INIT}")
message("CMAKE_CXX_FLAGS_DEBUG = ${CMAKE_CXX_FLAGS_DEBUG}")



set(CMAKE_SUPPRESS_REGENERATION TRUE)
set(__ARCH intel64)
set(X86X64 x64)



# rather than build the dispatcher, we could link against the pre-build distributed
# dispatcher libraries, but there are two issues with this:
# a) I think it would more, not less complex. b) Intel does not distribute a shared-lib ready dispatcher on Linux

add_subdirectory (imsdk.dispatcher/opensource/mfx_dispatch) 
#add_subdirectory (imsdk.samples/samples) 
# It would be nice to use 'add_subdirectory' to build the samples, but there are two issues:
# 1) The sample Cmake files do not support Windows yet, 2) They don't output a lib, which we need.









#list(APPEND SOURCES  src/tutorials.common/common_utils_windows.cpp )
#list(APPEND SOURCES  src/tutorials.common/common_utils.cpp )
#list(APPEND SOURCES  src/tutorials.common/cmd_options.cpp )
#list(APPEND SOURCES  src/tutorials.common/common_directx11.cpp )
#list(APPEND SOURCES  src/tutorials.common/common_directx.cpp )

# I put underscores in front of the non-Intel stuff for clarity in VS Solution Explorer
#list(APPEND SOURCES  src/_ColorConversions.cpp )
#list(APPEND SOURCES  src/_Resampler.cpp )
list(APPEND SOURCES  src/_VideoAccelerationSupport.cpp )
# old list(APPEND SOURCES  src/_DeviceSetup.cpp )
list(APPEND SOURCES  src/_CPPCLI.cpp )
list(APPEND SOURCES  src/exports_limevideosdk.def )
# these need to wait until I rewrite them to be free of 3rd party code
#list(APPEND SOURCES  src/_Decoder.cpp )
#list(APPEND SOURCES  src/_Encoder.cpp )
#list(APPEND SOURCES  src/_Transcoder.cpp )
#list(APPEND SOURCES  src/_Helpers.cpp )

# Intel stuff
list(APPEND SOURCES  imsdk.samples/samples/sample_common/src/base_allocator.cpp )
list(APPEND SOURCES  imsdk.samples/samples/sample_common/src/d3d_device.cpp )
list(APPEND SOURCES  imsdk.samples/samples/sample_common/src/d3d_allocator.cpp )
list(APPEND SOURCES  imsdk.samples/samples/sample_common/src/d3d11_device.cpp )
list(APPEND SOURCES  imsdk.samples/samples/sample_common/src/d3d11_allocator.cpp )
list(APPEND SOURCES  imsdk.samples/samples/sample_common/src/decode_render.cpp )
list(APPEND SOURCES  imsdk.samples/samples/sample_common/src/general_allocator.cpp )
list(APPEND SOURCES  imsdk.samples/samples/sample_common/src/vaapi_allocator.cpp )
list(APPEND SOURCES  imsdk.samples/samples/sample_common/src/vaapi_device.cpp )
list(APPEND SOURCES  imsdk.samples/samples/sample_common/src/vaapi_utils.cpp )
list(APPEND SOURCES  imsdk.samples/samples/sample_common/src/vaapi_utils_android.cpp )
list(APPEND SOURCES  imsdk.samples/samples/sample_common/src/vaapi_utils_drm.cpp )
list(APPEND SOURCES  imsdk.samples/samples/sample_common/src/vaapi_utils_x11.cpp )
list(APPEND SOURCES  imsdk.samples/samples/sample_common/src/sysmem_allocator.cpp )






## no no no, old way.
##if(WIN32)
##    set (OUTPUT_PREFIX lib)
##endif()



 

set( LIBNAME libLimeVideoSDKNative${X86X64} )


add_library( ${LIBNAME}  SHARED  ${SOURCES} )

# add dummymain until we add back in native: encoder decoder transcoder helpers
list(APPEND DECSRC ${SOURCES} )
list(APPEND DECSRC  src/_dummymain.cpp )
add_executable( decodetest ${DECSRC} )

 

#set_target_properties(decodetest PROPERTIES LINKER_LANGUAGE CXX)

##set_target_properties(${LIBNAME} PROPERTIES PREFIX  "lib")


if(WIN32)
    # this help build a combined solution with the C# and C++ projects
    # we only recommend using this .sln when you need to C++ work
    include_external_msproject(LimeVideoSDKQuickSync "../../../dotnet/Class Libraries/LimeVideoSDKQuickSync/LimeVideoSDKQuickSync.csproj" PLATFORM "x64")
    include_external_msproject(Decoder1 "../../../dotnet/Sample Programs/Decoder Samples/Decoder1/Decoder1.csproj" PLATFORM "x64")
    add_dependencies(LimeVideoSDKQuickSync ${LIBNAME})      # we need an explicit build dependancy between these two.   
    add_dependencies(Decoder1 LimeVideoSDKQuickSync)      # we need an explicit build dependancy between these two.   
    # does not work set_property(DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR} PROPERTY VS_STARTUP_PROJECT Decoder1)

    #on windows, we create a .SLN dependancy so libmfxsw64 gets built before ${LIBNAME}
    add_dependencies(${LIBNAME} libmfxsw64)                  
    add_library( libmfxsw64  SHARED  src/libmfxsw64stub.cpp ) # we also need to build libmfxsw64 on windows
    set_target_properties(libmfxsw64 PROPERTIES RUNTIME_OUTPUT_DIRECTORY ../../../lib/bin )

    #on windows, we create a .SLN dependancy so libLimeVideoSDK_ipp gets built before ${LIBNAME}
    add_dependencies(${LIBNAME} libLimeVideoSDK_ipp )   
    list(APPEND SRC_IPPALT src/_resampler.cpp )
    list(APPEND SRC_IPPALT src/_ColorConversions.cpp )
     list(APPEND SRC_IPPALT src/exports_FourCCResizeConvert.def )
    add_library( libLimeVideoSDK_ipp  SHARED    ${SRC_IPPALT} ) 
    set_target_properties(libLimeVideoSDK_ipp PROPERTIES RUNTIME_OUTPUT_DIRECTORY ../../../lib/bin )
    set_target_properties(libLimeVideoSDK_ipp PROPERTIES LINK_FLAGS " /ignore:4099 /def:../src/exports_FourCCResizeConvert.def ") 
  
    # we dont need a post-build copy when we can just set the output path.
    set_target_properties(${LIBNAME} PROPERTIES RUNTIME_OUTPUT_DIRECTORY ../../../lib/bin )
    include( ${CMAKE_SOURCE_DIR}/cmake/FindDirectX.cmake )
    find_path(DXVA2DIR dxva2.lib
    "C:/Program Files (x86)/Windows Kits/10/Lib/*/um/${X86X64}"
    "C:/Program Files (x86)/Windows Kits/8.1/Lib/*/um/${X86X64}"
    NO_DEFAULT_PATH
    )
    message("DXVA2DIR=${DXVA2DIR}")
    include_directories("C:/Program Files (x86)/IntelSWTools/Intel(R)_Media_SDK_2016.0.1/igfx_s3dcontrol/include")
    add_definitions(-DDX9_D3D)
    add_definitions(-DDX11_D3D)
    add_definitions(-DWIN64)

#target_compile_options(${LIBNAME} "/W4 /WX") 
target_compile_options(${LIBNAME} PUBLIC /W4 /WX)
target_compile_options(${LIBNAME} PUBLIC /W4 /WX)
      
    #add_compile_options("/W4 /WX") 
     
    string( REPLACE "/RTC1" "" CMAKE_CXX_FLAGS_DEBUG ${CMAKE_CXX_FLAGS_DEBUG} )
    set_target_properties(${LIBNAME} PROPERTIES COMPILE_FLAGS  "/EHa")
    set_target_properties(decodetest PROPERTIES COMPILE_FLAGS  "/EHa")
   
    set_source_files_properties(src/_CPPCLI.cpp PROPERTIES COMPILE_FLAGS "/clr")
    set_target_properties(${LIBNAME} PROPERTIES LINK_FLAGS " /ignore:4099 /def:../src/exports_limevideosdk.def ") 


    # These lines do the following:
    # They create a special relationship between the files ${LIBNAME} and LibmfxSW64.dll and libLimeVideoSDK_ipp
    #
    # LibmfxSW64.dll is Intel's software fallback library for Quick Sync encode, decode, etc...
    # Having LibmfxSW64.dll as a 'assemblylinkresource' means when Visual Studio
    # copies ${LIBNAME} to an output folder, then LibmfxSW64.dll will also be copied to the output
    # folder. 
    # This means C# sample projects automatically get LibmfxSW64.dll dumped into their output folders.
    # Which means the Intel dispatcher can easily find the LibmfxSW64.dll library if software fallback
    # is needed!
    # This also means Visual Studio will complain when it cannot find libmfxsw64.dll
    #
    get_target_property(tmp_link_flags ${LIBNAME} LINK_FLAGS)
    set_target_properties(${LIBNAME} PROPERTIES LINK_FLAGS_DEBUG " ${tmp_link_flags} /assemblylinkresource:../../../lib/bin/debug/libmfxsw64.dll") 
      set_target_properties(${LIBNAME} PROPERTIES LINK_FLAGS_RELEASE " ${tmp_link_flags} /assemblylinkresource:../../../lib/bin/release/libmfxsw64.dll") 
    
    get_target_property(tmp_link_flags ${LIBNAME} LINK_FLAGS)
    set_target_properties(${LIBNAME} PROPERTIES LINK_FLAGS_DEBUG " ${tmp_link_flags} /assemblylinkresource:../../../lib/bin/debug/libLimeVideoSDK_ipp.dll") 
        set_target_properties(${LIBNAME} PROPERTIES LINK_FLAGS_RELEASE " ${tmp_link_flags} /assemblylinkresource:../../../lib/bin/release/libLimeVideoSDK_ipp.dll") 
   
   

    ##add_custom_command(TARGET ${LIBNAME} POST_BUILD COMMAND ${CMAKE_COMMAND} -E copy libmfxsw64.dll $<TARGET_FILE_DIR:${LIBNAME}> )

    # this gets executed before ${LIBNAME} is built
   # add_custom_command(TARGET libmfxsw64 POST_BUILD COMMAND ${CMAKE_COMMAND} -E copy $<TARGET_FILE:libmfxsw64> . )
   # add_custom_command(TARGET libLimeVideoSDK_ipp POST_BUILD COMMAND ${CMAKE_COMMAND} -E copy $<TARGET_FILE:libLimeVideoSDK_ipp> . )

    target_link_libraries( ${LIBNAME}  ${DXVA2DIR}/dxva2.lib)
    target_link_libraries( ${LIBNAME}  ${DirectX_ROOT_DIR}/lib/${MSVC_CXX_ARCHITECTURE_ID}/dxgi.lib)
    target_link_libraries( ${LIBNAME}  ${DirectX_ROOT_DIR}/lib/${MSVC_CXX_ARCHITECTURE_ID}/d3d11.lib)
    target_link_libraries( ${LIBNAME}  ${DirectX_ROOT_DIR}/lib/${MSVC_CXX_ARCHITECTURE_ID}/d3d9.lib)
    target_link_libraries( ${LIBNAME}  ${DirectX_ROOT_DIR}/lib/${MSVC_CXX_ARCHITECTURE_ID}/dxguid.lib)


    target_link_libraries( decodetest  mfx )  
    target_link_libraries( decodetest  ${DXVA2DIR}/dxva2.lib)
    target_link_libraries( decodetest  ${DirectX_ROOT_DIR}/lib/${MSVC_CXX_ARCHITECTURE_ID}/dxgi.lib)
    target_link_libraries( decodetest  ${DirectX_ROOT_DIR}/lib/${MSVC_CXX_ARCHITECTURE_ID}/d3d11.lib)
    target_link_libraries( decodetest  ${DirectX_ROOT_DIR}/lib/${MSVC_CXX_ARCHITECTURE_ID}/d3d9.lib)
    target_link_libraries( decodetest  ${DirectX_ROOT_DIR}/lib/${MSVC_CXX_ARCHITECTURE_ID}/dxguid.lib)

    

    target_link_libraries( ${LIBNAME}  mfx )   
else()
    include_directories( /usr/include/libdrm )
    add_definitions(-DLIBVA_SUPPORT -DLIBVA_DRM_SUPPORT)
    set_target_properties(${LIBNAME} PROPERTIES COMPILE_FLAGS  "-std=c++11 ")
    target_link_libraries( ${LIBNAME}  -Wl,-whole-archive dispatch_shared -Wl,-no-whole-archive)
endif()

include_directories(src/tutorials.common)
include_directories(imsdk.samples/samples/sample_common/include)
include_directories(imsdk.dispatcher/include)
include_directories(imsdk.dispatcher/opensource/mfx_dispatch/include)
#include_directories(nv12.convert.and.resize)


