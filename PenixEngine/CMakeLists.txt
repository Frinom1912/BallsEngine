include(./env.cmake OPTIONAL RESULT_VARIABLE LOCAL_ENV)
message(STATUS "Local .env.cmake: ${LOCAL_ENV}")
 
set(NAME PenixEngine)
 
message(STATUS "using ${CMAKE_GENERATOR}")
if (CMAKE_GENERATOR STREQUAL "MinGW Makefiles")
    if (NOT MINGW_PATH) 
      message(FATAL_ERROR "MINGW_PATH not set in .env.cmake")
    endif()
    set(USE_MINGW "True")
    set(CMAKE_C_COMPILER ${MINGW_PATH}/bin/gcc.exe)
    set(CMAKE_CXX_COMPILER  ${MINGW_PATH}/bin/g++.exe)
endif()
 
project(${NAME} VERSION 0.0.1)

 
# 1. Set VULKAN_SDK_PATH in .env.cmake to target specific vulkan version
if (DEFINED VULKAN_SDK_PATH)
  set(Vulkan_INCLUDE_DIRS "${VULKAN_SDK_PATH}/Include") # 1.1 Make sure this include path is correct
  set(Vulkan_LIBRARIES "${VULKAN_SDK_PATH}/Lib") # 1.2 Make sure lib path is correct
  set(Vulkan_FOUND "True")
else()
  find_package(Vulkan REQUIRED) # throws error if could not find Vulkan
  message(STATUS "Found Vulkan: $ENV{VULKAN_SDK}")
endif()
if (NOT Vulkan_FOUND)
    message(FATAL_ERROR "Could not find Vulkan library!")
else()
    message(STATUS "Using vulkan lib at: ${Vulkan_LIBRARIES}")
endif()
 
 
# 2. Set GLFW_PATH in .env.cmake to target specific glfw
if (DEFINED GLFW_PATH)
  message(STATUS "Using GLFW path specified in .env")
  set(GLFW_INCLUDE_DIRS "${GLFW_PATH}/include")
  if (MSVC)
    set(GLFW_LIB "${GLFW_PATH}/lib-vc2022") # 2.1 Update lib-vc2019 to use same version as your visual studio
  elseif (CMAKE_GENERATOR STREQUAL "MinGW Makefiles")
    message(STATUS "USING MINGW")
    set(GLFW_LIB "${GLFW_PATH}/lib-mingw-w64") # 2.2 make sure matches glfw mingw subdirectory
  endif()
else()
  find_package(glfw3 3.3 REQUIRED)
  set(GLFW_LIB glfw)
  message(STATUS "Found GLFW")
endif()
if (NOT GLFW_LIB)
    message(FATAL_ERROR "Could not find glfw library!")
else()
    message(STATUS "Using glfw lib at: ${GLFW_LIB}")
endif()
 
# 3. Set BOOST_ROOT in .env.cmake to target Boost version
# should only include components ideally, but idc for now
find_package(Boost 1.84.0 REQUIRED)  #to see the auto assigned variables https://cmake.org/cmake/help/latest/module/FindBoost.html#result-variables
if (NOT Boost_FOUND)
    message(FATAL_ERROR "Boost not installed, or BOOST_ROOT is not specified")
endif()

include_directories(external)

file(GLOB_RECURSE LIB_SOURCES lib_src/*.cpp)

set(PROJECT_NAME_LIB ${PROJECT_NAME}_lib)

add_library(PROJECT_NAME_LIB ${LIB_SOURCES} "lib_src/GraphicsLib/VulkanTest.h")

add_library(PROJECT_NAME_LIB::lib_1 ALIAS PROJECT_NAME_LIB)

file(GLOB_RECURSE SOURCES src/*.cpp)

add_executable(${PROJECT_NAME} ${SOURCES} "lib_src/GraphicsLib/VulkanTest.h")

target_compile_features(${PROJECT_NAME} PUBLIC cxx_std_20)
target_compile_features(PROJECT_NAME_LIB PUBLIC cxx_std_20)
 
set_property(TARGET ${PROJECT_NAME} PROPERTY VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}/build")

if (WIN32)
  message(STATUS "CREATING BUILD FOR WINDOWS")
 
  if (USE_MINGW)
    target_include_directories(${PROJECT_NAME} PUBLIC
      ${MINGW_PATH}/include
    )
    target_link_directories(${PROJECT_NAME} PUBLIC
      ${MINGW_PATH}/lib
    )
  endif()
 
  target_include_directories(PROJECT_NAME_LIB PUBLIC
    ${PROJECT_SOURCE_DIR}/lib_src
    ${Vulkan_INCLUDE_DIRS}
    ${GLFW_INCLUDE_DIRS}
    ${GLM_PATH}
    ${Boost_INCLUDE_DIRS}
    )

 
  target_link_directories(PROJECT_NAME_LIB PUBLIC
    ${Vulkan_LIBRARIES}
    ${GLFW_LIB}
    ${Boost_LIBRARY_DIRS}
  )

  target_link_libraries(PROJECT_NAME_LIB glfw3 vulkan-1)

  target_link_libraries(${PROJECT_NAME}
    PRIVATE
        PROJECT_NAME_LIB::lib_1
  )
elseif (UNIX)
    message(STATUS "CREATING BUILD FOR UNIX")
    target_include_directories(${PROJECT_NAME} PUBLIC
      ${PROJECT_SOURCE_DIR}
    )
    target_link_libraries(${PROJECT_NAME} glfw ${Vulkan_LIBRARIES})
endif()

add_custom_command(TARGET ${PROJECT_NAME} PRE_BUILD COMMAND cpplint ${SOURCES})
 