using UnityEngine;
using System.Runtime.InteropServices;

namespace EmteqLabs.Unity.Dab.Base.Mobile
{

    public class HelloWorld : MonoBehaviour
    {
        public void OnGUI()
        {
            GUILayout.Label("Hello Unity-3d World!!!" + emteq_runtime_helloWorld());
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
            // Calls the ExamplePluginFunction inside the plugin
            // And prints 5 to the console
            print(emteq_runtime_helloWorld());
        }
    }

}
