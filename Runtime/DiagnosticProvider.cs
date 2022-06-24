using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Emteq.Device.Runtime.Unity.Mobile
{

    public class DiagnosticProvider : MonoBehaviour
    {
        public void OnGUI()
        {
#if UNITY_EDITOR // Does this mean it only shows under the editor?

            float retVal = -1;
            try
            {
                retVal = emteq_runtime_helloWorld();
            }
            finally 
            { 
                float expectedRetVal = 1234.568F;
                bool hasNativeCalls = Math.Abs(retVal-expectedRetVal) < 0.0005;
                GUILayout.Label($"Emteq-Device-Runtime/HasNative, {hasNativeCalls}");            
            }

#endif

        }

        // Start is called before the first frame update
        void Start()
        {

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

#if UNITY_IPHONE // On iOS plugins are statically linked
		[DllImport ("__Internal")]
#else
        [DllImport("emteq-device-runtime")]
#endif
        private static extern float emteq_runtime_helloWorld();

        void Awake()
        {

        }
    }

}
