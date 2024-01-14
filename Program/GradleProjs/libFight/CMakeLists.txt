# Sets the minimum version of CMake required to build the native
# library. You should either keep the default value or only pass a
# value of 3.6.0 or lower.

cmake_minimum_required(VERSION 3.6.0)

file(STRINGS ../buildConfig.txt BuildType)
file(STRINGS ../staticLibsConfig.txt OutPath)
file(STRINGS ../staticLibsConfig.txt GAME_LIB)
file(STRINGS ../toolChainConfig.txt ToolChain)
file(STRINGS ../NDKConfig.txt NDK_ROOT)
file(STRINGS ../ControllerConfig.txt Controller)

string(REPLACE "gameLib" "engineLib" ENGINE_LIB ${GAME_LIB})

if(${BuildType} STREQUAL "Release")
    add_definitions(-DSIMPLE2D_SUPPORT)
    add_definitions(-DSCRIPT_SUPPORT)
    add_definitions(-DLUA_INGORE_BEGIN=)
    add_definitions(-DLUA_INGORE_END=)
    add_definitions(-DLUA_INGORE=)
    add_definitions(-DHK_DEBUG_SLOW)
    add_definitions(-DHK_TARGET_NAME=AndroidArmVs2010)
    add_definitions(-DPROFILING)
    add_definitions(-D_VISION_POSIX)
    add_definitions(-D_VISION_NO_WARNING_IMAGE_FORMAT)
    add_definitions(-DVBASE_LIB)
    add_definitions(-DLINK_WITH_VISION)
    add_definitions(-DVISIONDLL_LIB)
    add_definitions(-DPERFORMANCE_TEST)
    add_definitions(-D_VISION_MOBILE)
    add_definitions(-D_VISION_ANDROID)
    add_definitions(-DCLIENT_PROFILING)
    add_definitions(-DTARGET_PLATFORM=2)
    add_definitions(-D__ANDROID__)
	add_definitions(-DCLIENT_USE)

    set(GAME_LIB_DIR ${GAME_LIB}/release${ToolChain})
    set(ENGINE_LIB_DIR ${ENGINE_LIB}/release${ToolChain})
elseif(${BuildType} STREQUAL "Debug")
    # add_definitions(-D_DEBUG)
    add_definitions(-DHK_DEBUG)
    add_definitions(-DSIMPLE2D_SUPPORT)
    add_definitions(-DSCRIPT_SUPPORT)
    add_definitions(-DLUA_INGORE_BEGIN=)
    add_definitions(-DLUA_INGORE_END=)
    add_definitions(-DLUA_INGORE=)
    add_definitions(-DHK_DEBUG_SLOW)
    add_definitions(-DHK_TARGET_NAME=AndroidArmVs2010)
    add_definitions(-DPROFILING)
    add_definitions(-D_VISION_POSIX)
    add_definitions(-D_VISION_NO_WARNING_IMAGE_FORMAT)
    add_definitions(-DVBASE_LIB)
    add_definitions(-DLINK_WITH_VISION)
    add_definitions(-DVISIONDLL_LIB)
    add_definitions(-DPERFORMANCE_TEST)
    add_definitions(-D_VISION_MOBILE)
    add_definitions(-D_VISION_ANDROID)
    add_definitions(-DS_DEBUG)
    add_definitions(-DCLIENT_PROFILING)
    add_definitions(-DTARGET_PLATFORM=2)
    add_definitions(-D__ANDROID__)
	

    set(GAME_LIB_DIR ${GAME_LIB}/debug${ToolChain})
    set(ENGINE_LIB_DIR ${ENGINE_LIB}/debug${ToolChain})
endif()

set(INCLUDE_DIR      
   ../../Share/EsShare
   ../../Share/Engine
   ../../Share/Engine/Include
   ../../Share/include   
   ../../Share/Table
   ../../Share/CommonShare
   ../../Other/Simple2D
   ../../Other/Simple2D/network
   ../../Share/Protocol
   ../../Client/Logic/BaseRace
   ../../Other/SimpleNet
   ../../Other/libuv/include
   ${EngineSource}
   ${NDK_ROOT}/sources
   ../../Client/ClientDll
   ../../Client/ClientDll/GameNet  
   ../../Other/PathFinder
)

set(LIB_DIR
    ${GAME_LIB_DIR}
)
include_directories(${INCLUDE_DIR})
link_directories(${LIB_DIR})

file(GLOB SRC_FILE
    "../../Client/ClientDll/*.cpp"
    "../../Client/ClientDll/GameNet/*.cpp"    
    "../../Share/Protocol/SeCRStream.cpp"
    "../../Share/Protocol/SeCommStream.cpp"
    "../../Share/Protocol/SeCTStream.cpp"
    "../../Share/Protocol/SeCGTStream.cpp"
    )

add_library( # Sets the name of the library.
             Client

             # Sets the library as a shared library.
             SHARED

             # Provides a relative path to your source file(s).
             # Associated headers in the same location as their source
             # file are automatically included.
             ${SRC_FILE}
             )

 # Searches for a specified prebuilt library and stores the path as a
 # variable. Because system libraries are included in the search path by
 # default, you only need to specify the name of the public NDK library
 # you want to add. CMake verifies that the library exists before
 # completing its build.

 find_library( # Sets the name of the path variable.
               log-lib

               # Specifies the name of the NDK library that
               # you want CMake to locate.
               log )
 find_library(
             stdc++-lib
             stdc++ )
 find_library(
             c-lib
             c )
 find_library(
             dl-lib
             dl )
 find_library(
             m-lib
             m )
 find_library(
             EGL-lib
             EGL
             )
 find_library(
             android-lib
             android )
 find_library(
             c++_static-lib
             c++_static )

 # Specifies libraries CMake should link to your target library. You
 # can link multiple libraries, such as libraries you define in the
 # build script, prebuilt third-party libraries, or system libraries.

 if(${BuildType} STREQUAL "Release")
     target_link_libraries( # Specifies the target library.
                             Client
                             "-Wl,--start-group"
                             ######## start gameLib #######
                             ${NDK_ROOT}/sources/cxx-stl/llvm-libc++/libs/${ANDROID_ABI}/libandroid_support.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libRaceLogic.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libTask.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libBaseRace.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libPathFinder.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libRaceTool.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libTable.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libCommonShare.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libEsShare.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libSimpleNet.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libuv.a
                             ${android-lib}
                             "-Wl,--end-group"
                             ${GLESv1_CM-lib}
                             ${GLESv2-lib}
                             ${EGL-lib}
                             ${c-lib}
                             ${log-lib}
                             ${dl-lib}
                             ${m-lib}
                             )
 elseif(${BuildType} STREQUAL "Debug")
     target_link_libraries( # Specifies the target library.
                             Client
                             "-Wl,--start-group"
                             ######## start gameLib #######
                             ${NDK_ROOT}/sources/cxx-stl/llvm-libc++/libs/${ANDROID_ABI}/libandroid_support.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libRaceLogic.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libTask.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libBaseRace.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libPathFinder.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libRaceTool.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libTable.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libCommonShare.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libEsShare.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libSimpleNet.a
                             ${GAME_LIB_DIR}/${ANDROID_ABI}/libuv.a
                             ${android-lib}
                             "-Wl,--end-group"
                             ${GLESv1_CM-lib}
                             ${GLESv2-lib}
                             ${EGL-lib}
                             ${c-lib}
                             ${log-lib}
                             ${dl-lib}
                             ${m-lib}

                             #"-Wl,--no-whole-archive"
                             )
 endif()