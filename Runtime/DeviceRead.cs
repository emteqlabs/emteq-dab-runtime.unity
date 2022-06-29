using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.IO; //< Stream, File.Create
using System.Threading; //< Thread
using System.Threading.Tasks; //< Task

namespace Emteq.Device.Runtime.UnityRuntime
{

    public class DeviceRead : MonoBehaviour
    {
        Texture emteqLogo;
        Context context = null;
        bool verifiedNativeCall = false;
        Version apiVersion = default;
        bool verifiedDataRead = false;
      
        public void OnGUI()
        {
//#if UNITY_EDITOR // Does this mean it only shows under the editor?
            GUILayout.Label(emteqLogo);
            GUILayout.Label($"Emteq-Device-Runtime/NativeCall, {verifiedNativeCall}, HasContext, {context != null}");
            GUILayout.Label($"Emteq-Device-Runtime/Api.version, {apiVersion.major}.{apiVersion.minor}.{apiVersion.patch}.{apiVersion.commit} ({apiVersion.describe})");
            GUILayout.Label($"Emteq-Device-Runtime/SandboxReadPassed, {verifiedDataRead}");
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
            
            Task contextTask = context.run();
            // open stream
            Stream client = context.openStream(Emteq.Device.Runtime.StreamId.Raw, 200);
            try
            {
                // delay some time, to allow server to accept the client (TODO: not sure if really needed)
                Thread.Sleep(1000); //1000 ms
                if(!verifiedDataRead)
                {
                     // expected data
                    byte[] expectedData = new byte[256];
                    for (int i = 0; i < expectedData.Length; i++) { expectedData[i] = BitConverter.GetBytes(i)[0]; }
                    // read data
                    byte[] data = new byte[256];
                    client.ReadTimeout = 500;
                    Int32 readCount = client.Read(data, 0, data.Length);
                    Console.WriteLine($"Start: I've read {readCount} bytes")
                    if(readCount == data.Length)
                    {
                        verifiedDataRead = true;
                        for(int i = 0; i < readCount; i++)
                        {
                            if(expectedData[i] != data[i])
                            {
                                verifiedDataRead = false;
                                break;
                            }
                        }
                    }
                }
            }
            finally
            {
                client?.Dispose();
                context.stop();
                contextTask.Wait();
                context.Dispose();
            }
            
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
            
        }

        void Awake()
        {

        }

        void OnApplicationQuit()
        {
            
        }
    }

}
