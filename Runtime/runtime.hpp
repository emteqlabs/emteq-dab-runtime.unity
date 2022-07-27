#pragma once

#if __cplusplus < 201103L 
#error "This header requires a C++11 compiler"
#endif

#include <cstdint> //< std::size_t
#include <exception>
#include <memory> //< std::unique_ptr
#include <chrono> //< std::milliseconds
#include <string_view> //< std::string_view

#include "emteq-device-runtime_export.h" //< Cmake generated
#include "runtime.h" //< C defines reuse for interop

#if __ANDROID__
/** [Android Only] The JVM is required to access USB subsystem under Android
*/
#include <jni.h>
#endif

namespace emteq {
namespace runtime {

    // @todo Proper remapping to CPP type
    using RetVal = EmteqRetval_t;

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

    using StreamHandle = EmteqStreamHandle_t; ///< Bring C type definityion in C++ namespace

    /** @note All values mapped for c-interop
    */
    enum class StreamId : uint16_t
    {
        Raw = EMTEQ_STREAMID_RAW_DAB
    };

    template<EmteqOption_t option> struct Option;
    template<> struct Option<Emteq_Option_LogLevel> { using Value = EmteqLogLevel_t; };
    template<> struct Option<Emteq_Option_StreamOpenMode> { using Value = EmteqStreamOpenMode_t; };
    template<> struct Option<Emteq_Option_RxBeginMode> { using Value = EmteqRxBeginMode_t; };
#if __ANDROID__
    template<> struct Option<Emteq_Option_Android_JavaVm> { using Value = JavaVM*; };
#endif


    /**  Context object encapsulates all the global state associated with
    * emteq runtime library
    */
    class EMTEQ_DEVICE_RUNTIME_EXPORT Context final
    {
        struct Impl; //< Pimpl implementation
    public:

        /** Magic number to check this is a valid context instance
        * @note This is used for C-interoperability checks where the context is an opaque pointer
        * @note ABI - Value is compiled into client-code
        */
        static constexpr uint32_t MagicTag = 0xEDABC0DE;

        /** Get Global option value used as default for all further contexts
        */
        template<EmteqOption_t option>
        static typename Option<option>::Value getDefault();

        /** Set option value used as default for all further contexts
        */
        template<EmteqOption_t option, typename... Value>
        static void setDefault(Value... value);

    public:

        /** Create the context object.
        */
        Context();
        ~Context();

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

        /** Get option value */
        template<EmteqOption_t option>
        typename Option<option>::Value get()
        { throw std::domain_error("Option is not supported"); }

        /** Set option value 
        */
        template<EmteqOption_t option, typename... Value>
        void set( Value... value )
        { throw std::domain_error("Option is not supported"); }

        /** Set the path at which data can be persisted/cached/saved
        * @param[in]  id   PathId to set
        * @param[in]  path  PathId uri
        */
        void setPath(const PathId id, const std::string_view path );

        /** Retrieve the options specified by `setPath`
        */
        std::string_view getPath( const PathId id);

        /** Polling runtime update
        * @param[in]  timeout  If the timeout is zero, returns immediately without blocking.
        * @warn Must be called regularily to update device interaction
        * @note call `run()` on separate thread is preferred(?)
        */
        void update(const std::chrono::milliseconds timeout = std::chrono::milliseconds{ 0 } );

        /**  Blocking execution, never returns on current thread until another thread calls stopExecute
        * @note Any prior unproceessed call to `stop()` will cause `run()` to exit immediately
        */
        void run();

        /** Check is a stop is pending on the context
        * @note Stop will be processed by the active or next call to `run()`
        */
        bool isStopping() const noexcept;

        /** Check for running state
        * @notice The thread calling `run()` may not yet be scheduled and check for `while(!isRunning()){}` may  be useful
        */
        bool isRunning() const noexcept;

        /** Stop 'run()` from another thread
        * @note Calling `stop()` will cause any proceeding `run()` to exit immediately
        */
        void stop();

        /** Create a new raw read-write socket to the DAB device
        * @note This is  a unix domain socket on compatible platforms
        * @ref https://stackoverflow.com/a/2760267
        * @return Socket descriptor
        */
        StreamHandle openStream(const StreamId id);

        bool isRawSocket(StreamHandle descriptor);

        /** Function for opening client socket
         *
         * @param timeoutMs The timeout for the operations, in milliseconds.
         * If set to -1, operation is waiting indefinitely for the socket to open
         * If set to 0, returns immediately
         * Otherwise must be greater than 100 ms
         * @todo: handle timeout better
         * @todo: replace openStream
         * @return The socket descriptor and status
         */
        EmteqRuntimeSocketStatus_t openStream(const StreamId id, const std::chrono::milliseconds timeoutMs);

        /** Function for reading from client socket
         *
         * @param socketDescriptor The socket descriptor to be read from
         * @param buffer The buffer to store the read data
         * @param bufferSize The size of the buffer
         * @param timeoutMs The timeout for the operations, in milliseconds.
         * If set to -1, operation is waiting indefinitely for the socket to open
         * If set to 0, returns immediately
         * Otherwise must be greater than 100 ms
         * @todo: handle timeout better
         * @return The number of bytes read and status of the operation
         */
        EmteqRuntimeSocketIoStatus_t readStream(StreamHandle descriptor, char* bytes, const size_t bytesSize, const std::chrono::milliseconds timeoutMs);
        
        /** Function for writing to client socket
         *
         * @param socketDescriptor The socket descriptor to write data to
         * @param buffer The buffer holding the data to be written
         * @param bufferSize The size of the data to be written
         * @param timeoutMs The timeout for the operations, in milliseconds.
         * If set to -1, operation is waiting indefinitely for the socket to open
         * If set to 0, returns immediately
         * Otherwise must be greater than 100 ms
         * @todo: handle timeout better
         * @todo: multiple write transfers untill all data is written
         * If set to -1, operation is waiting indefinitely for the socket to open
         * @return The number of bytes written and status of the operation
         */
        EmteqRuntimeSocketIoStatus_t writeStream(StreamHandle descriptor, const char* bytes, const size_t bytesSize, const std::chrono::milliseconds timeoutMs);

        /** Close a socket opened via openStream() 
        * 
        */
        void closeRawSocket(StreamHandle descriptor);


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

    /** @see EmteqLogLevel_t
     */
    template<> EMTEQ_DEVICE_RUNTIME_EXPORT void Context::set<Emteq_Option_LogLevel>(EmteqLogLevel_t option);
    template<> EMTEQ_DEVICE_RUNTIME_EXPORT EmteqLogLevel_t Context::get<Emteq_Option_LogLevel>();

    /** @see EmteqStreamOpenMode_t
     */
    template<> EMTEQ_DEVICE_RUNTIME_EXPORT void Context::set<Emteq_Option_StreamOpenMode>(EmteqStreamOpenMode_t option);
    template<> EMTEQ_DEVICE_RUNTIME_EXPORT EmteqStreamOpenMode_t Context::get<Emteq_Option_StreamOpenMode>();

    /** @see EmteqRxBeginMode_t
     */
    template<> EMTEQ_DEVICE_RUNTIME_EXPORT void Context::set<Emteq_Option_RxBeginMode>(EmteqRxBeginMode_t option);
    template<> EMTEQ_DEVICE_RUNTIME_EXPORT EmteqRxBeginMode_t Context::get<Emteq_Option_RxBeginMode>();

#if __ANDROID__
    template<> EMTEQ_DEVICE_RUNTIME_EXPORT void Context::set<Emteq_Option_Android_JavaVm>(JavaVM* jvm);
    template<> EMTEQ_DEVICE_RUNTIME_EXPORT JavaVM* Context::get<Emteq_Option_Android_JavaVm>();
#endif

} //END: runtime
} //END: emteq
