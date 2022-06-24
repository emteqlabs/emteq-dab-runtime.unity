using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Emteq.Device.Runtime.Unity.Mobile
{

    public class DiagnosticProvider : MonoBehaviour
    {
        Texture emteqLogo;
        Context context = null;
        bool verifiedNativeCall = false;
        Version apiVersion = default;

      
        public void OnGUI()
        {
//#if UNITY_EDITOR // Does this mean it only shows under the editor?
            GUILayout.Label(emteqLogo);
            GUILayout.Label($"Emteq-Device-Runtime/NativeCall, {verifiedNativeCall}, HasContext, {context != null}");
            GUILayout.Label($"Emteq-Device-Runtime/Api.version, {apiVersion.major}.{apiVersion.minor}.{apiVersion.patch}.{apiVersion.commit} ({apiVersion.describe})");
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
    }

}
