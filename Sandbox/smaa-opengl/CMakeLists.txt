#cmake ver req
cmake_minimum_required(VERSION 2.6)

set(project_name smaa)

#project name
project(${project_name})

#so that it talks to us
set(CMAKE_VERBOSE_MAKEFILE ON)
set(CMAKE_ALLOW_LOOSE_LOOPS TRUE)

if(UNIX)
#some compiler directives to make sure all warnings etc. are caught, 
#and those that come from 3rd party libs are not
add_definitions( "-Wall -Wno-long-long -ansi -pedantic" )
endif()

#header files source
include_directories(${CMAKE_SOURCE_DIR}/src/)
link_directories(${CMAKE_SOURCE_DIR}/src/)

#put depencencies here
include_directories(${CMAKE_SOURCE_DIR}/external/include/)
link_directories(${CMAKE_SOURCE_DIR}/external/lib/)

if(UNIX)
set(${project_name}_external_libs sfml-window sfml-system sfml-audio GLEW GL)
endif()

if(WIN32)
set(${project_name}_external_libs sfml-window sfml-system sfml-audio GLEW opengl32)
endif()

#adding the project's exe
add_executable(${project_name} main)

target_link_libraries(${project_name} ${${project_name}_external_libs})