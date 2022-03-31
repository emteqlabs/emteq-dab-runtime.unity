using UnityEngine;
using System.Runtime.InteropServices;

namespace EmteqLabs.Unity.Dab.Base.Mobile
{

    public class HelloWorld : MonoBehaviour
    {
        public void OnGUI()
        {
            GUILayout.Label("Hello Unity-3d World!!!" + dabSdk_helloWorld());
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

#if UNITY_IPHONE // On iOS plugins are statically linked
		[DllImport ("__Internal")]
#else
        [DllImport("dabSdk")]
#endif
        private static extern float dabSdk_helloWorld();

        void Awake()
        {
            // Calls the ExamplePluginFunction inside the plugin
            // And prints 5 to the console
            print(dabSdk_helloWorld());
        }
    }

}
