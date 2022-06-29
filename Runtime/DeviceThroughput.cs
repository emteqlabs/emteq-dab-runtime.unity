using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Threading; //< Thread
using System.Threading.Tasks; //< Task

namespace Emteq.Device.Runtime.UnityRuntime
{

    public class DeviceThroughput : MonoBehaviour
    {
        Texture emteqLogo;
        Context context = null;
        bool verifiedNativeCall = false;
        Version apiVersion = default;
        Task contextTask;
        Stream client;
        bool destroy = false;
        bool clientValid = false;

        int dataBufferSize = 4096; //bytes
        byte[] dataBuffer;
        int totalReadBytes = 0;
      
        public void OnGUI()
        {
//#if UNITY_EDITOR // Does this mean it only shows under the editor?
            GUILayout.Label(emteqLogo);
            GUILayout.Label($"Emteq-Device-Runtime/NativeCall, {verifiedNativeCall}, HasContext, {context != null}");
            GUILayout.Label($"Emteq-Device-Runtime/Api.version, {apiVersion.major}.{apiVersion.minor}.{apiVersion.patch}.{apiVersion.commit} ({apiVersion.describe})");
            GUILayout.Label($"Emteq-Device-Runtime/TotalReadSize, {totalReadBytes/1024.0} KiB");
//#endif
        }

        // Start is called before the first frame update
        void Start()
        {
            emteqLogo = Resources.Load("emteq_logo") as Texture2D;

            float helloValue = -1;
            try
            {
                helloValue = CApi.emteq_runtime_helloWorld();
                apiVersion = CApi.emteq_api_version();
                context = new Context();
            }
            finally
            {
                float expectedRetVal = 1234.568F;
                verifiedNativeCall = Math.Abs(helloValue - expectedRetVal) < 0.0005;
            }
            // allocate buffer
            dataBuffer = new byte[dataBufferSize];
            // run the main runtime thread
            contextTask = context.run();
            // create a client
            client = context.openStream(Emteq.Device.Runtime.StreamId.Raw, 200);
            clientValid = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.touchCount > 0)
            {
               destroy = true; 
            }
            if(!destroy && clientValid)
            {
                try
                {
                    totalReadBytes += client.Read(dataBuffer, 0, dataBufferSize);
                }
                catch(Exception e)
                {
                    // if exception raises, close the client
                    client?.Dispose();
                    clientValid = false;
                }
            }
            if(destroy)
            {
                client?.Dispose();
                clientValid = false;
                context.stop();
                contextTask.Wait();
            }
            if(!destroy && !clientValid)
            {
                client = context.openStream(Emteq.Device.Runtime.StreamId.Raw, 200);
                clientValid = true;
            }
        }

        /** Update is called at frame independent rate (default 0.02 seconds / 50hz)
         * @note You can change the fixed time intervals from the Time section under the Project Settings.
         */
        void FixedUpdate()
        {

        }

        void Awake()
        {

        }
    }

}
