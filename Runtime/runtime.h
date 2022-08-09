#pragma	once

#include "emteq-device-runtime_export.h" //< Cmake generated

#if !(__STDC_VERSION__ >= 201112L || __cplusplus >= 201103L)
#error "This header requires a C11 or C++11 compiler"
#endif 

#include <cstdint> //< uint16_t, uintptr_t
#include <cstddef> //< size_t

#include "common.h" //< EmteqVersion_t, EmteqRuntime_t etc.

#ifdef __cplusplus
extern "C" {
#endif

EMTEQ_DEVICE_RUNTIME_EXPORT EmteqVersion_t emteq_api_version();

EMTEQ_DEVICE_RUNTIME_EXPORT char const* emteq_api_string( EmteqRetval_t retval );

/** Check if the given runtime is a valid runtime instance i.e. Has called emteq_runtime_create(runtime)
 * @param runtime The runtime object to test for validity
 * @return True if the runtime has been instantiated, false if not or when `emteq_runtime_destroy(runtime)` has been called
*/
EMTEQ_DEVICE_RUNTIME_EXPORT bool emteq_runtime_isInstance(EmteqRuntime_t* runtime);

/** Initialise the provided emteq runtime instance
* @warning `runtime` should not already be created or shall have been destroyed via `emteq_runtime_destroy()`
* @param[in]  sizeOfRuntime  Should be passed as sizeof(*runtime) for API runtime-compatiblity. * 
* @return EMTEQ_SUCCESS successful context creation, emteq_runtime_destroy() should be called to release resource
* @return EMTEQ_INVALID_PARAMETER runtime shall be a valid pointer
* @return EMTEQ_INSUFFICIENT_RESOURCE sizeOfRuntime is not sufficient to instantiate the dynamically linked runtime, rebuild Application against latest emteq-device-runtime API
* @return EMTEQ_INTERNAL_ERROR unexpected excep[tion thrown from C++ backend (TODO: Diagnostic)
*/
EMTEQ_DEVICE_RUNTIME_EXPORT EmteqRetval_t emteq_runtime_create(EmteqRuntime_t* runtime, const size_t sizeOfRuntime );

/** Release resources for a created from `emteq_runtime_create`
*/
EMTEQ_DEVICE_RUNTIME_EXPORT EmteqRetval_t emteq_runtime_destroy(EmteqRuntime_t* runtime);

/** Set context options
 * @param  runtime  Runtime context on which to operate
 * @param  option   Which option to set
 * @param  ...      Any required arguments for the specified option
*/
EMTEQ_DEVICE_RUNTIME_EXPORT EmteqRetval_t emteq_runtime_setOption(EmteqRuntime_t* runtime, enum EmteqOption_t option, ...);

/** Get context options
 * @param  runtime  Runtime context on which to operate
 * @param  option   Which option to get
 * @param  ...      Pointer(s) to store any required arguments for the specified option
 */
EMTEQ_DEVICE_RUNTIME_EXPORT EmteqRetval_t emteq_runtime_getOption(EmteqRuntime_t* runtime, EmteqOption_t option, ...);

/** Perform an update tick
* @note Normally called in application loop or from inbuilt function `emteq_runtime_run()
*/
EMTEQ_DEVICE_RUNTIME_EXPORT EmteqRetval_t emteq_runtime_update(EmteqRuntime_t* runtime, const int timeoutMs);

/** Blocking update loop until `emteq_runtime_stop()` is called from a separate thread
*/
EMTEQ_DEVICE_RUNTIME_EXPORT EmteqRetval_t emteq_runtime_run(EmteqRuntime_t* runtime);

/** Returns whether `emteq_runtime_run()` is executing on a separate thread
*/
EMTEQ_DEVICE_RUNTIME_EXPORT bool emteq_runtime_isRunning(EmteqRuntime_t* runtime);

/** Signals any pending `emteq_runtime_run()` that is running on a separate thread to return
*/
EMTEQ_DEVICE_RUNTIME_EXPORT EmteqRetval_t emteq_runtime_stop(EmteqRuntime_t* runtime);

/** Returns whether a stop is pending which will cause any current/subseqnet call to `emteq_runtime_run()` to return
*/
EMTEQ_DEVICE_RUNTIME_EXPORT bool emteq_runtime_isStopping(EmteqRuntime_t* runtime);

/** Set the path at which data can be persisted/cached/saved
* @param[in]  id   PathId to set
* @param[in]  path  PathId uri
*/
EMTEQ_DEVICE_RUNTIME_EXPORT EmteqRetval_t emteq_runtime_setDataPath(EmteqRuntime_t* runtime, const EmteqPathId_t id, const char* path);

/** Retrieve the options specified by `emteq_runtime_setDataPath` 
*/
EMTEQ_DEVICE_RUNTIME_EXPORT EmteqRetval_t emteq_runtime_getDataPath(EmteqRuntime_t* runtime, const EmteqPathId_t id, const char** path);

/** Internal: API test function
* @returns 1234.5678f
*/
EMTEQ_DEVICE_RUNTIME_EXPORT float emteq_runtime_helloWorld();

/** Create a new raw read-write socket to the DAB device
* @note This is  a unix domain socket on compatible platforms
* @ref https://stackoverflow.com/a/2760267
* @return Socket descriptor or -1 on failure
* @see EmteqStreamOpenMode_t for details of options for when stream can be opened
*/
EMTEQ_DEVICE_RUNTIME_EXPORT EmteqRuntimeSocketStatus_t emteq_runtime_openStream(EmteqRuntime_t* runtime, const EmteqStreamId_t id, const int timeoutMs);

EMTEQ_DEVICE_RUNTIME_EXPORT bool emteq_runtime_isRawSocket(EmteqRuntime_t* runtime, EmteqStreamHandle_t descriptor);

/** Close a socket opened via openRawSocket()
*
*/
EMTEQ_DEVICE_RUNTIME_EXPORT void emteq_runtime_closeRawSocket(EmteqRuntime_t* runtime, EmteqStreamHandle_t descriptor);

EMTEQ_DEVICE_RUNTIME_EXPORT EmteqRuntimeSocketIoStatus_t emteq_runtime_readStream(EmteqRuntime_t* runtime, EmteqStreamHandle_t descriptor, char* bytes, const size_t bytesSize, const int timeoutMs);

EMTEQ_DEVICE_RUNTIME_EXPORT EmteqRuntimeSocketIoStatus_t emteq_runtime_writeStream(EmteqRuntime_t* runtime, EmteqStreamHandle_t descriptor, const char* bytes, const size_t bytesSize, const int timeoutMs);


#ifdef __cplusplus
}
#endif
