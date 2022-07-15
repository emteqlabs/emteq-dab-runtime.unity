using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;

namespace Emteq.Device.Runtime.UnityRuntime
{

    public class DiagnosticProvider : MonoBehaviour
    {
        Texture emteqLogo;
        Context context = null;
        Task contextTask = null;
        Stream rawStream = null;
        bool verifiedNativeCall = false;
        Version apiVersion = default;
        long readBytes = 0;
        bool contextEnabled = true;

        private string FormatBytes(long bytes)
        {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0 Bytes";
        }

        public void OnGUI()
        {
            bool isRunning = (contextTask != null) ? contextTask.Status.Equals(TaskStatus.Running) : false;
            
           // GUILayout.BeginVertical();
            GUILayout.Label(emteqLogo);
            GUILayout.Label($"Emteq-Device-Runtime/NativeCall, {verifiedNativeCall}, HasContext, {context != null}");
            GUILayout.Label($"Emteq-Device-Runtime/Api.version, {apiVersion.major}.{apiVersion.minor}.{apiVersion.patch}.{apiVersion.commit} ({apiVersion.describe})");

            var newEnabled = GUILayout.Toggle(contextEnabled, contextEnabled ? "STOP" : "START"
                , GUILayout.ExpandWidth(true), GUILayout.Height(50) );

            GUILayout.Label($"Emteq-Device-Runtime/Context.running, {isRunning}");
            GUILayout.Label($"Emteq-Device-Runtime/RawStream.open, {rawStream != null}, bytes, {FormatBytes(readBytes)} ");
            //GUILayout.BeginVertical();

            if (newEnabled != contextEnabled)
            {
                contextEnabled = newEnabled;
                if ( !contextEnabled)
                {
                    context?.stop();
                    contextTask?.Wait();
                    contextTask = null;
                }
                else
                {
                    contextTask = context.run();
                }
            }
        }


        // Start is called before the first frame update
        async void Start()
        {
            Debug.Log("Start");

            emteqLogo = Resources.Load("emteq_logo") as Texture2D;

            float helloValue = -1;
            try
            {
                helloValue = CApi.emteq_runtime_helloWorld();
                apiVersion = CApi.emteq_api_version();
            }
            finally
            {
                float expectedRetVal = 1234.568F;
                verifiedNativeCall = Math.Abs(helloValue - expectedRetVal) < 0.0005;
            }

            context = new Context();
            contextTask = context.run();
        }

        void OnDestroy()
        {
            Debug.Log("OnDestroy");

            rawStream?.Dispose();
            context?.stop();
            contextTask?.Wait();
        }

        // Update is called once per frame
        void Update()
        {

        }

        /** Update is called at frame independent rate (default 0.02 seconds / 50hz)
         * @note You can change the fixed time intervals from the Time section under the Project Settings.
         */
        void FixedUpdate()
        {
            if (rawStream == null)
            {
                rawStream = context.openStream(Emteq.Device.Runtime.StreamId.Raw, 0);

                if (rawStream != null)
                {
                    Debug.Log("FixedUpdate: Open-success");
                    rawStream.ReadTimeout = 1;
                    readBytes = 0;
                }
                else
                {
                    Debug.Log("FixedUpdate: Not-opened");
                }
            }
            else
            {
                try
                {
                    Byte[] data = new Byte[4096];
                    Int32 readCount = rawStream.Read(data, 0, data.Length);
                    readBytes += readCount;
                    //Debug.Log($"FixedUpdate: read {readCount}");
                }
                catch(OperationCanceledException)
                {
                    Debug.Log($"FixedUpdate: Client socket closing");
                    rawStream.Dispose();
                    rawStream = null;
                }
            }
        }

        void Awake()
        {
            Debug.Log("Awake");

        }
    }

}
