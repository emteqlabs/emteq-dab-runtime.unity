using UnityEngine;
using System.Runtime.InteropServices;

namespace EmteqLabs.Unity.Dab.Base.Mobile
{

    public class HelloWorld : MonoBehaviour
    {
        public void OnGUI()
        {
            GUILayout.Label("Hello Unity-3d World!!!" + helloWorld_unity3d());
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

#if UNITY_IPHONE
    // On iOS plugins are statically linked into
    // the executable, so we have to use __Internal as the
    // library name.
    [DllImport ("__Internal")]
#else
        // Other platforms load plugins dynamically, so pass the
        // name of the plugin's dynamic library.
        [DllImport("hello_unity3d")]
#endif
        private static extern float helloWorld_unity3d();

        void Awake()
        {
            // Calls the ExamplePluginFunction inside the plugin
            // And prints 5 to the console
            print(helloWorld_unity3d());
        }
    }

}
