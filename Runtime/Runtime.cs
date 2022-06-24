using System.Runtime.InteropServices;
using System;
using System.Threading.Tasks; //< Task

namespace Emteq.Device.Runtime
{
    public enum RetVal
    {
          EMTEQ_TRYAGAIN = 1 ///< EWOULDBLOCK, WSAEWOULDBLOCK

        , EMTEQ_SUCCESS = 0 ///< >=0 No-Error, <0 Error

        , EMTEQ_INVALID_PARAMETER = -1
        , EMTEQ_INSUFFICIENT_RESOURCE = -3
        , EMTEQ_INVALID_SOCKET = -4 //< tried read/write operation on non-client socket

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


    [StructLayout(LayoutKind.Sequential)]
    public struct Version
    {
        public UInt16 major;
        public UInt16 minor;
        public UInt16 patch;
        public UInt16 commit;
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
        internal static extern void emteq_api_setJvm(IntPtr jvm);

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
        internal static extern bool emteq_runtime_isInstance(IntPtr/*CRuntime*/ runtime);

        /** @todo @see `runtime.h`
        */
        [DllImport(DLL_Path)]
        internal static extern RetVal emteq_runtime_create(IntPtr/*CRuntime*/ runtime, int/* @todo UIntPtr? for size_t etc*/ sizeOfRuntime );

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

#if true /// @todo How to!?!? UNITY_STANDALONE_WIN
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
            , byte[] bytes
            , UIntPtr bytesSize
            , int timeoutMs);

        [DllImport(DLL_Path)]
        internal static extern IoStatus emteq_runtime_writeStream(IntPtr/*CRuntime*/ runtime
            , StreamHandle descriptor
            , byte[] bytes
            , UIntPtr bytesSize
            , int timeoutMs);

    }


    public class Context : IDisposable
    {
        internal CApi.CRuntime runtime = new CApi.CRuntime();

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


        internal int readStream(CApi.StreamHandle descriptor, byte[] buffer, int offset, int bytesToRead, int timeoutMs )
        {
            if (offset != 0) throw new Exception("Offset must be 0 at present!");

            CApi.IoStatus ret = CApi.emteq_runtime_readStream(runtime, descriptor, buffer
                , (System.UIntPtr)bytesToRead, timeoutMs );

            if (ret.status == RetVal.EMTEQ_SUCCESS
                || ret.status == RetVal.EMTEQ_TRYAGAIN )
                return (int)ret.count;
            else
                throw new ApplicationException("Runtime read failed: " + ret.status);
        }

        internal int writeStream(CApi.StreamHandle descriptor, byte[] buffer, int offset, int bytesToWrite, int timeoutMs)
        {
            if (offset != 0) throw new Exception("Offset must be 0 at present!");

            CApi.IoStatus ret = CApi.emteq_runtime_writeStream(runtime, descriptor, buffer
                , (System.UIntPtr)bytesToWrite, timeoutMs);

            if (ret.status == RetVal.EMTEQ_SUCCESS
                || ret.status == RetVal.EMTEQ_TRYAGAIN)
                return (int)ret.count;
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
            return context.readStream(descriptor, buffer, offset, bytesToRead, ReadTimeout );
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