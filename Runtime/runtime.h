#pragma	once

#include "dabruntime_export.h" //< Cmake generated

#if !(__STDC_VERSION__ >= 201112L || __cplusplus >= 201103L)
#error "This header requires a C11 or C++11 compiler"
#endif 

#ifdef __cplusplus
extern "C" {
#endif


enum EmteqRetval_t
{
    EMTEQ_SUCCESS, //< No-Error
    EMTEQ_INVALID_PARAMETER,
    EMTEQ_INTERNAL_ERROR,
    EMTEQ_INSUFFICIENT_RESOURCE
};

enum EmteqPathId_t
{
    EMTEQ_PATH_CACHE,  /**< Storage location for temporary cache files that are private to the running application
                        * @note For Android `cachePath` corresponds to Activity Internal-DataPath
                        */
    EMTEQ_PATH_EXPORT,  /**< Storage location for exported / saved files
                        * @note  For Android corresponds to Activity External-DataPath. This is world-readable and can be modified by the user when they enable USB mass storage
                        */
};

struct alignas(void*) EmteqRuntime_t
{
    char _[32]; //< internal use
};

#if __ANDROID__
#include <jni.h>
/** [Android Only] The JVM is required to access USB subsystem under Android 
*/
DABRUNTIME_EXPORT void emteq_api_setJvm(JavaVM* jvm);
#endif

DABRUNTIME_EXPORT void emteq_api_version(int* major, int* minor, int* patch, int* commit);

DABRUNTIME_EXPORT char const* emteq_api_string( EmteqRetval_t retval );

/** Check if the given runtime is a valid runtime instance i.e. Has called emteq_runtime_create(runtime)
 * @param runtime The runtime object to test for validity
 * @return True if the runtime has been instantiated, false if not or when `emteq_runtime_destroy(runtime)` has been called
*/
DABRUNTIME_EXPORT bool emteq_runtime_isInstance(EmteqRuntime_t* runtime);

DABRUNTIME_EXPORT EmteqRetval_t emteq_runtime_create(EmteqRuntime_t* runtime);

DABRUNTIME_EXPORT EmteqRetval_t emteq_runtime_run(EmteqRuntime_t* runtime);
DABRUNTIME_EXPORT bool emteq_runtime_isRunning(EmteqRuntime_t* runtime);
DABRUNTIME_EXPORT EmteqRetval_t emteq_runtime_stop(EmteqRuntime_t* runtime);
DABRUNTIME_EXPORT bool emteq_runtime_isStopping(EmteqRuntime_t* runtime);


/** Set the path at which data can be persisted/cached/saved
* @param[in]  id   PathId to set
* @param[in]  path  PathId uri
*/
DABRUNTIME_EXPORT EmteqRetval_t emteq_runtime_setDataPath(EmteqRuntime_t* runtime, const EmteqPathId_t id, const char* path);

/** Retrieve the options specified by `emteq_runtime_setDataPath` 
*/
DABRUNTIME_EXPORT EmteqRetval_t emteq_runtime_getDataPath(EmteqRuntime_t* runtime, const EmteqPathId_t id, const char** path);

DABRUNTIME_EXPORT EmteqRetval_t emteq_runtime_destroy(EmteqRuntime_t* runtime);

DABRUNTIME_EXPORT float emteq_runtime_helloWorld();


#if __linux || __ANDROID__
/** Create a new raw read-write socket to the DAB device
* @note This is  a unix domain socket on compatible platforms
* @ref https://stackoverflow.com/a/2760267
* @return Socket descriptor or -1 on failure
*/
DABRUNTIME_EXPORT int emteq_runtime_openRawSocket(EmteqRuntime_t* runtime);

/** Close a socket opened via openRawSocket()
*
*/
DABRUNTIME_EXPORT void emteq_runtime_closeRawSocket(EmteqRuntime_t* runtime, int descriptor);
#endif

#ifdef __cplusplus
}
#endif
