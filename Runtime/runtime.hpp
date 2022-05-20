#pragma once

#if __cplusplus < 201103L 
#error "This header requires a C++11 compiler"
#endif

#include <filesystem> //< std::filesystem::path
#include <cstdint> //< std::size_t
#include <memory> //< std::unique_ptr
#include <chrono> //< std::milliseconds
#include <string_view> //< std::string_view

#include "emteq-device-runtime_export.h" //< Cmake generated
#include "runtime.h" //< C defines reuse for interop

#if 0 //< WIP
#include "internal/experimental/fdStream.hpp"
#endif

namespace emteq {
namespace runtime {

    /** @note All values mapped for c-interop
    */
    enum class PathId : uint16_t
    {
        Cache = EMTEQ_PATH_CACHE, /**< Storage location for temporary cache files that are private to the running application
                * @note For Android `cachePath` corresponds to Activity Internal-DataPath
                */
        Export = EMTEQ_PATH_EXPORT, /**< Storage location for exported / saved files
                * @note  For Android corresponds to Activity External-DataPath. This is world-readable and can be modified by the user when they enable USB mass storage
                */
    };

    /**  Context object encapsulates all the global state associated with
    * emteq runtime library
    */
    class Context final
    {
        struct Impl; //< Pimpl implementation
    private:

        /** Magic number to check this is a valid context instance
        * @note This is used for C-interoperability checks where the context is an opaque pointer
        * @note ABI - Value is compiled into client-code
        */
        static constexpr uint32_t MagicTag = 0xEDABC0DE;

    public:

        /** Create the context object.
        */
        EMTEQ_DEVICE_RUNTIME_EXPORT Context();
        EMTEQ_DEVICE_RUNTIME_EXPORT ~Context();

        ///@{ Non-Copyable nor Movable
        Context(const Context&) = delete;
        Context& operator= (const Context&) = delete;
        Context(Context&&) = delete;
        Context& operator= (Context&&) = delete;
        ///@}

        /** Returns true if object is-a `Context` instance.
        * @see MagicTag
        * @note ABI - Function is in-lined into client-code
        */
        inline bool checkTag() const
        { return magicTag_ == MagicTag; }

        /** Set the path at which data can be persisted/cached/saved
        * @param[in]  id   PathId to set
        * @param[in]  path  PathId uri
        */
        EMTEQ_DEVICE_RUNTIME_EXPORT void setPath(const PathId id, const std::filesystem::path& path );

        /** Retrieve the options specified by `setPath`
        */
        EMTEQ_DEVICE_RUNTIME_EXPORT const std::filesystem::path& getPath( const PathId id);

        /** Polling runtime update
        * @param[in]  timeout  If the timeout is zero, returns immediately without blocking.
        * @warn Must be called regularily to update device interaction
        * @note call `run()` on separate thread is preferred(?)
        */
        EMTEQ_DEVICE_RUNTIME_EXPORT void update(const std::chrono::milliseconds timeout = std::chrono::milliseconds{ 0 } );

        /**  Blocking execution, never returns on current thread until another thread calls stopExecute
        * @note Any prior unproceessed call to `stop()` will cause `run()` to exit immediately
        */
        EMTEQ_DEVICE_RUNTIME_EXPORT void run();

        /** Check is a stop is pending on the context
        * @note Stop will be processed by the active or next call to `run()`
        */
        EMTEQ_DEVICE_RUNTIME_EXPORT bool isStopping() const noexcept;

        /** Check for running state
        * @notice The thread calling `run()` may not yet be scheduled and check for `while(!isRunning()){}` may  be useful
        */
        EMTEQ_DEVICE_RUNTIME_EXPORT bool isRunning() const noexcept;

        /** Stop 'run()` from another thread
        * @note Calling `stop()` will cause any proceeding `run()` to exit immediately
        */
        EMTEQ_DEVICE_RUNTIME_EXPORT void stop();

#if __linux || __ANDROID__
        /** Create a new raw read-write socket to the DAB device
        * @note This is  a unix domain socket on compatible platforms
        * @ref https://stackoverflow.com/a/2760267
        * @return Socket descriptor or -1 on failure
        */
        EMTEQ_DEVICE_RUNTIME_EXPORT int openRawSocket();

        /** Close a socket opened via openRawSocket() 
        * 
        */
        EMTEQ_DEVICE_RUNTIME_EXPORT void closeRawSocket(int descriptor);
#endif

#if 0 //< WIP
        /** Create a new raw read-write socket to the DAB device
        * @note This is  a unix domain socket on compatible platforms
        */
        EMTEQ_DEVICE_RUNTIME_EXPORT experimental::fdstream openDabFdStream();
#endif

    private:
        const Impl& impl() const { return *impl_; }
        Impl& impl() { return *impl_; }

    private:

        std::unique_ptr<Impl> impl_;
        uint32_t magicTag_ = MagicTag; //< @see MagicTag
    };

} //END: runtime
} //END: emteq
