using System.Runtime.InteropServices;
using System;
using System.Threading.Tasks; //< Task

namespace Emteq.Device.Runtime
{
    public enum RetVal
    {
          EMTEQ_CLOSING = 2 ///< Device is being disconnected @note next call will likely error
        , EMTEQ_TRYAGAIN = 1 ///< EWOULDBLOCK, WSAEWOULDBLOCK

        , EMTEQ_SUCCESS = 0 ///< >=0 No-Error, <0 Error

        , EMTEQ_INVALID_PARAMETER = -1
        , EMTEQ_INSUFFICIENT_RESOURCE = -3
        , EMTEQ_INVALID_SOCKET = -4 //< tried read/write operation on non-client socket

        , EMTEQ_NOT_SUPPORTED = -66 //< Operation not supported or unimplemented on this platform 

        , EMTEQ_INTERNAL_ERROR = -99
    };

    public enum PathId
    {
        EMTEQ_PATH_CACHE = 0,  /**< Storage location for temporary cache files that are internal to the running application
                        * @note For Android `cachePath` corresponds to Activity Internal-DataPath
                        */
        EMTEQ_PATH_EXPORT = 1,  /**< Storage location for exported / saved files
                        * @note  For Android corresponds to Activity External-DataPath. This is world-readable and can be modified by the user when they enable USB mass storage
                        */
    };

    public enum StreamId
    {
        Raw = 0
    }

    /** When shall the Client stream be available 
    * * @see emteq_runtime_openStream
    */
    public enum StreamOpenMode
    {
        Always = 1, ///< [default] Stream can always be opened before device is attached (will close on removal allowing immediate reopen)
        WhenAttached = 2, ///< Stream can only be opened after device is attached (will close on removal)
    };

    /** When shall the device data reads commence
    */
    public enum RxBeginMode
    {
        OnOpen = 1, ///< [default] rx-transfers commence when a client opens the device stream (@see EmteqStreamOpenMode_t for when client is able to open the stream)
        OnFirstWrite = 2 ///< rx-transfers commence after first tx-transfer on the stream (client @see emteq_runtime_writeStream)
    }

    public enum LogLevel
    {
       Verbose = 0, ///< Verbose logging. Should typically be disabled for a release.    
       Debug = 1, ///< Debug logging. Should typically be disabled for a release.    
       Info = 2, ///< Informational logging. Should typically be disabled for a release.   
       Warning = 3, ///< Warning logging. For use with recoverable failures.   
       Error = 4, ///< Error logging. For use with unrecoverable failures.   
       Fatal = 5, ///< Fatal logging. For use when aborting.
    };

    public enum Option
    {
        /**  Set the log message verbosity.
         *
	     * If emteq-device-runtime was compiled with verbose debug message logging, this function
	     * does nothing: you'll always get messages from all levels.
	     */
         LogLevel = 0,

        /** Specify when shall Client openStream will succeed. 
         * i.e. Always mode allows open prior to device being attached but WhenAttached will fail to open a stream until device is enumerated.
         * 
         * Value shall be provided of type `EmteqStreamOpenMode_t`
         * 
         * @see EmteqStreamOpenMode_t
         */
        StreamOpenMode = 1,

        /** Specify when shall shall the device data reads commence. 
         * i.e. OnFirstWrite allows confiruing the device prior to data streaming
         * 
         * Value shall be provided of type `EmteqRxBeginMode_t`
         * 
         * @see EmteqRxBeginMode_t
         */
        RxBeginMode = 2,

        /** This option should be set _before_ calling emteq_runtime_create(), and
         * specifies the java virtual machine for new calls to
         * emteq_runtime_create() after the option is set.  The context pointer is
         * unused.
         * 
         * Value shall be provided of type `JavaVM* jvm`
         * 
         * Only valid on Android.
         */
        Android_JavaVm = 3,

#if false ///TODO
        StreamReadTimeout = , //< TODO: Support stateful option instead of per-read
        StreamWriteTimeout = , //< TODO: Support stateful option instead of per-write
#endif
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Version
    {
        public UInt16 major;
        public UInt16 minor;
        public UInt16 patch;
        public UInt16 commit;

        [MarshalAs(UnmanagedType.LPStr)]
        public String describe; ///< For ABI compatibility only. e.g "v0.7.4-0-g49012c6"
    }

    internal class CApi
    {
        [StructLayout(LayoutKind.Sequential)]
        internal class CRuntime
        {
            internal const int Size = 32; ///< @warn Shall match or exceed C-APi `sizeof(CRuntime)`
            private IntPtr interopBuffer;  //< internal

            public static implicit operator IntPtr(CRuntime rhs)
            {
                return rhs.interopBuffer;
            }

            internal CRuntime()
            {
                interopBuffer = Marshal.AllocHGlobal(Size);
            }

            ~CRuntime()
            {
                Marshal.FreeHGlobal(interopBuffer);
            }
        }

#if UNITY_IPHONE ///< @note On iOS plugins are statically linked so internal
        internal const string DLL_Path = "__Internal";
#else ///< @note On Android (+others? TBC) plugins dynamic and imported by name wihtout 'lib' prefix or file extension
        internal const string DLL_Path = "emteq-device-runtime";
#endif

        /** @todo @see `runtime.h`
        */
        [DllImport(DLL_Path)]
        internal static extern Version emteq_api_version();

        /** @todo @see `runtime.h`
        */
        [DllImport(DLL_Path)]
        internal static extern String emteq_api_string(RetVal retval);


        /** @todo @see `runtime.h`
        */
        [DllImport(DLL_Path)]
        internal static extern RetVal emteq_runtime_setOption(IntPtr/*CRuntime*/ runtime, Option option, int value);

        /** @todo @see `runtime.h`
         * @todo Do we need: , CallingConvention = CallingConvention.Cdecl
         */
        [DllImport(DLL_Path)]
        internal static extern RetVal emteq_runtime_getOption(IntPtr/*CRuntime*/ runtime, Option option, ref int value);

        /** @todo @see `runtime.h`
        */
        [DllImport(DLL_Path)]
        internal static extern bool emteq_runtime_isInstance(IntPtr/*CRuntime*/ runtime);

        /** @todo @see `runtime.h`
        */
        [DllImport(DLL_Path)]
        internal static extern RetVal emteq_runtime_create(IntPtr/*CRuntime*/ runtime, int/* @todo UIntPtr? for size_t etc*/ sizeOfRuntime);

        /** @todo @see `runtime.h`
        */
        [DllImport(DLL_Path)]
        internal static extern RetVal emteq_runtime_destroy(IntPtr/*CRuntime*/ runtime);

        /** @todo @see `runtime.h`
        */
        [DllImport(DLL_Path)]
        internal static extern RetVal emteq_runtime_run(IntPtr/*CRuntime*/ runtime);

        /** @todo @see `runtime.h`
        */
        [DllImport(DLL_Path)]
        internal static extern bool emteq_runtime_isRunning(IntPtr/*CRuntime*/ runtime);

        /** @todo @see `runtime.h`
        */
        [DllImport(DLL_Path)]
        internal static extern RetVal emteq_runtime_stop(IntPtr/*CRuntime*/ runtime);

        /** @todo @see `runtime.h`
        */
        [DllImport(DLL_Path)]
        internal static extern bool emteq_runtime_isStopping(IntPtr/*CRuntime*/ runtime);

        /** @todo @see `runtime.h`
        */
        [DllImport(DLL_Path)]
        internal static extern RetVal emteq_runtime_setDataPath(IntPtr/*CRuntime*/ runtime, PathId id, String path);

        /** @todo @see `runtime.h`
        */
        [DllImport(DLL_Path)]
        internal static extern RetVal emteq_runtime_getDataPath(IntPtr/*CRuntime*/ runtime, PathId id, ref String path);


        /** @todo @see `runtime.h`
        */
        [DllImport(DLL_Path)]
        internal static extern float emteq_runtime_helloWorld();

#if false || UNITY_STANDALONE_WIN
        [StructLayout(LayoutKind.Sequential)]
        internal struct StreamHandle
        {
            UInt64 val;
        }
#else
        [StructLayout(LayoutKind.Sequential)]
        internal struct StreamHandle
        {
            int val;
        }
#endif

        internal struct StreamStatus
        {
            public StreamHandle descriptor;
            public RetVal status;
        };

        /** @todo @see `runtime.h`
        */
        [DllImport(DLL_Path)]
        internal static extern StreamStatus emteq_runtime_openStream(IntPtr/*CRuntime*/ runtime, StreamId id, int timeoutMs);

        /** @todo @see `runtime.h`
        */
        [DllImport(DLL_Path)]
        internal static extern bool emteq_runtime_isRawSocket(IntPtr/*CRuntime*/ runtime, StreamHandle descriptor);

        /** @todo @see `runtime.h`
        */
        [DllImport(DLL_Path)]
        internal static extern void emteq_runtime_closeRawSocket(IntPtr/*CRuntime*/ runtime, StreamHandle descriptor);

        internal struct IoStatus
        {
            public UIntPtr count;
            public RetVal status;
        };

        /** @todo @see `runtime.h`
        */
        [DllImport(DLL_Path)]
        internal static extern IoStatus emteq_runtime_readStream(IntPtr/*CRuntime*/ runtime
            , StreamHandle descriptor
            , IntPtr bytes
            , UIntPtr bytesSize
            , int timeoutMs);

        [DllImport(DLL_Path)]
        internal static extern IoStatus emteq_runtime_writeStream(IntPtr/*CRuntime*/ runtime
            , StreamHandle descriptor
            , IntPtr bytes
            , UIntPtr bytesSize
            , int timeoutMs);

    }


    public class Context : IDisposable
    {
        internal CApi.CRuntime runtime = new CApi.CRuntime();

        public static void setDefault<ValueT>(Option option, ValueT value) where ValueT : Enum
        {
            int interopValue = Convert.ToInt32(value);
            handleRetVal(CApi.emteq_runtime_setOption(IntPtr.Zero, option, interopValue), "setOption");
        }

        public static ValueT getDefault<ValueT>(Option option) where ValueT : Enum
        {
            int value = default;
            handleRetVal(CApi.emteq_runtime_getOption(IntPtr.Zero, option, ref value), "getOption");
            return (ValueT)Enum.ToObject(typeof(ValueT), value);
        }

        public Context()
        {
            var retVal = CApi.emteq_runtime_create(runtime, CApi.CRuntime.Size);
            if (retVal != RetVal.EMTEQ_SUCCESS)
            {
                throw new Exception("emteq_runtime_create failed : " + retVal.ToString());
            }
        }

        ~Context()
        {
            Dispose(disposing: false);
        }

        /** Implement IDisposable.
         * @note Do not make this method virtual.
         */
        public void Dispose()
        {
            Dispose(disposing: true);

            // Take this object off the finalization queue to prevent finalization code a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isInstance())
            {
                // Dispose shall always clear up unmanaged objects
                // @note if 'disposing==true' we should clean up managed members too
                var retVal = CApi.emteq_runtime_destroy(runtime);
                if (retVal != RetVal.EMTEQ_SUCCESS)
                {
                    throw new Exception("emteq_runtime_destroy failed : " + retVal.ToString());
                }
            }
        }

        static private void handleRetVal(RetVal retVal, String tag)
        {
            switch (retVal)
            {
                case RetVal.EMTEQ_SUCCESS:
                    break;
                case RetVal.EMTEQ_INVALID_PARAMETER:
                    throw new ArgumentOutOfRangeException($"{tag} parameter is invalid : {retVal}");

                case RetVal.EMTEQ_NOT_SUPPORTED:
                    throw new InvalidOperationException($"{tag} is not supported : {retVal}");

                default:
                    throw new Exception($"{tag} set failed: {retVal}");
            }
        }

        public void set<ValueT>(Option option, ValueT value) where ValueT : Enum
        {
            int interopValue = Convert.ToInt32(value);
            handleRetVal(CApi.emteq_runtime_setOption(runtime, option, interopValue), "set-Option");
        }

        public ValueT get<ValueT>(Option option) where ValueT : Enum
        {
            int value = default;
            handleRetVal(CApi.emteq_runtime_getOption(runtime, option, ref value), "get-Option");
            return (ValueT)Enum.ToObject(typeof(ValueT), value);
        }

        public bool isInstance()
        {
            return CApi.emteq_runtime_isInstance(runtime);
        }

        public bool isRunning()
        {
            return CApi.emteq_runtime_isRunning(runtime);
        }

        public void stop()
        {
            var retVal = CApi.emteq_runtime_stop(runtime);
            if (retVal != RetVal.EMTEQ_SUCCESS)
            {
                throw new Exception("emteq_runtime_destroy failed : " + retVal.ToString());
            }
        }

        //Blocking
        public Task run()
        {
            return Task.Factory.StartNew(() =>
            {
                var retVal = CApi.emteq_runtime_run(runtime);
                if (retVal != RetVal.EMTEQ_SUCCESS)
                {
                    throw new Exception("emteq_runtime_run failed : " + retVal.ToString());
                }
            }, TaskCreationOptions.LongRunning);
        }

        internal bool isRawSocket(CApi.StreamHandle descriptor)
        {
            return CApi.emteq_runtime_isRawSocket(runtime, descriptor);
        }

        internal void closeRawSocket(CApi.StreamHandle descriptor)
        {
            CApi.emteq_runtime_closeRawSocket(runtime, descriptor);
        }

        public Stream openStream(StreamId id = StreamId.Raw, int timeoutMs = -1)
        {
            return new Stream(this, id, timeoutMs);
        }

        internal CApi.StreamHandle openStreamHandle(StreamId id, int timeoutMs)
        {
            CApi.StreamStatus ret = CApi.emteq_runtime_openStream(runtime, id, timeoutMs);

            if (ret.status == RetVal.EMTEQ_SUCCESS)
                return ret.descriptor;
            else
            if (ret.status == RetVal.EMTEQ_TRYAGAIN)
                throw new OperationCanceledException("Runtime rejected connection: Try-Again");
            else
                throw new ApplicationException("Runtime rejected connection: " + ret.status);
        }


        internal int readStream(CApi.StreamHandle descriptor, byte[] buffer, int offset, int bytesToRead, int timeoutMs)
        {
            GCHandle pinned_buffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr ptr_buffer = pinned_buffer.AddrOfPinnedObject();

            CApi.IoStatus ret = CApi.emteq_runtime_readStream(runtime, descriptor, ptr_buffer + offset
                , (System.UIntPtr)bytesToRead, timeoutMs);

            pinned_buffer.Free();

            if (ret.status == RetVal.EMTEQ_SUCCESS
                || ret.status == RetVal.EMTEQ_TRYAGAIN)
                return (int)ret.count;
            else
            if (ret.status == RetVal.EMTEQ_CLOSING)
                throw new OperationCanceledException("Runtime Stream closing: Client socket must be closed");
            else
                throw new ApplicationException("Runtime read failed: " + ret.status);
        }

        internal int writeStream(CApi.StreamHandle descriptor, byte[] buffer, int offset, int bytesToWrite, int timeoutMs)
        {
            GCHandle pinned_buffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr ptr_buffer = pinned_buffer.AddrOfPinnedObject();

            CApi.IoStatus ret = CApi.emteq_runtime_writeStream(runtime, descriptor, ptr_buffer + offset
                , (System.UIntPtr)bytesToWrite, timeoutMs);

            pinned_buffer.Free();

            if (ret.status == RetVal.EMTEQ_SUCCESS
                || ret.status == RetVal.EMTEQ_TRYAGAIN)
                return (int)ret.count;
            else
            if (ret.status == RetVal.EMTEQ_CLOSING)
                throw new OperationCanceledException("Runtime Stream closing: Client socket must be closed");
            else
                throw new ApplicationException("Runtime write failed: " + ret.status);
        }
    }

    // Single-producer, single-consumer Fifo byte-stream
    public class Stream : System.IO.Stream
    {
        private Context context = null;
        private CApi.StreamHandle descriptor;

        public Stream(Context context, StreamId id, int timeoutMs = -1)
        {
            this.context = context;
            this.descriptor = context.openStreamHandle(id, timeoutMs);
        }

        protected override void Dispose(bool disposing)
        {
            context.closeRawSocket(descriptor);
            base.Dispose(disposing);
        }

        public override int ReadTimeout { get; set; }
        public override int WriteTimeout { get; set; }

        public override void Write(byte[] buffer, int offset, int bytesToWrite)
        {
            context.writeStream(descriptor, buffer, offset, bytesToWrite, WriteTimeout);
        }

        //public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancel)
        //{
        //    // TODO: could be extended here with a proper async write
        //    Write(buffer, offset, count);
        //    return Task.CompletedTask;
        //}

        public override int Read(byte[] buffer, int offset, int bytesToRead)
        {
            return context.readStream(descriptor, buffer, offset, bytesToRead, ReadTimeout);
        }

        //public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        //{
        //    // TODO: Could be extended here with a proper async read
        //    var result = Read(buffer, offset, count);
        //    return Task.FromResult(result);
        //}

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;

        public override long Seek(long offset, System.IO.SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        //TODO support?
        public override void Flush() => throw new NotSupportedException();

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
    }

} //END: Emteq.Device.Runtime