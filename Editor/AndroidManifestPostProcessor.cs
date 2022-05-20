#if UNITY_ANDROID
using System;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.Android;

namespace Emteq.Device.Runtime
{
    /// @TODO Better set of C# example code here https://stackoverflow.com/a/54894488/876651
    public class AndroidManifestPostProcessor : IPostGenerateGradleAndroidProject
    {
        const string kAndroidNamespaceURI = "http://schemas.android.com/apk/res/android";

        public int callbackOrder { get { return 0; } }

        public void OnPostGenerateGradleAndroidProject(string projectPath)
        {
            CopyResources(projectPath);

            InjectAndroidManifest(projectPath);
        }

        private void CopyResources(string projectPath)
        {
            /* @todo We don't have any assets to copy yet
             
            // The projectPath points to the the parent folder instead of the actual project path.
            if (!Directory.Exists(Path.Combine(projectPath, "src")))
            {
                projectPath = Path.Combine(projectPath, PlayerSettings.productName);
            }
           
            // Get the icons set in the UnityNotificationEditorManager and write them to the res folder, then we can use the icons as res.
            var icons = NotificationSettingsManager.Initialize().GenerateDrawableResourcesForExport();
            foreach (var icon in icons)
            {
                var fileInfo = new FileInfo(string.Format("{0}/src/main/res/{1}", projectPath, icon.Key));
                if (fileInfo.Directory != null)
                {
                    fileInfo.Directory.Create();
                    File.WriteAllBytes(fileInfo.FullName, icon.Value);
                }
            }
            */
        }

        /* Adds:
	        <uses-feature android:name="android.hardware.usb.host" />
	        <uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
        */
        private void InjectBasePermissions(string manifestPath, XmlDocument manifestDoc)
        {
            AppendAndroidFeatureField(manifestPath, manifestDoc, "android.hardware.usb.host");
            AppendAndroidPermissionField(manifestPath, manifestDoc, "android.permission.WRITE_EXTERNAL_STORAGE");
        }

        private void InjectPluginSettings(string manifestPath, XmlDocument manifestDoc)
        {
            /* TODO: Customize for out use
            - If Cloud upload is enabled then we need to request Internet etc
            
            var settings = NotificationSettingsManager.Initialize().AndroidNotificationSettingsFlat;

            var useCustomActivity = (bool)settings.Find(i => i.Key == "UnityNotificationAndroidUseCustomActivity").Value;
            if (useCustomActivity)
            {
                var customActivity = (string)settings.Find(i => i.Key == "UnityNotificationAndroidCustomActivityString").Value;
                AppendAndroidMetadataField(manifestPath, manifestDoc, "custom_notification_android_activity", customActivity);
            }

            var enableRescheduleOnRestart = (bool)settings.Find(i => i.Key == "UnityNotificationAndroidRescheduleOnDeviceRestart").Value;
            if (enableRescheduleOnRestart)
            {
                AppendAndroidMetadataField(manifestPath, manifestDoc, "reschedule_notifications_on_restart", "true");
                AppendAndroidPermissionField(manifestPath, manifestDoc, "android.permission.RECEIVE_BOOT_COMPLETED");
            }
            */
        }

        private void InjectAndroidManifest(string projectPath)
        {
           var manifestPath = string.Format("{0}/src/main/AndroidManifest.xml", projectPath);
           if (!File.Exists(manifestPath))
               throw new FileNotFoundException(string.Format("'{0}' doesn't exist.", manifestPath));
           
           XmlDocument manifestDoc = new XmlDocument();
           manifestDoc.Load(manifestPath);

            InjectBasePermissions(manifestPath, manifestDoc);
            InjectReceivers(manifestPath, manifestDoc);
            InjectPluginSettings(manifestPath, manifestDoc);

            manifestDoc.Save(manifestPath);
        }

        internal static void InjectReceivers(string manifestPath, XmlDocument manifestXmlDoc)
        {
            /* @todo We probably need to hook up some Receivers... ?

            const string kNotificationManagerName = "com.unity.androidnotifications.UnityNotificationManager";
            const string kNotificationRestartOnBootName = "com.unity.androidnotifications.UnityNotificationRestartOnBootReceiver";

            var applicationXmlNode = manifestXmlDoc.SelectSingleNode("manifest/application");
            if (applicationXmlNode == null)
                throw new ArgumentException(string.Format("Missing 'application' node in '{0}'.", manifestPath));

            XmlElement notificationManagerReceiver = null;
            XmlElement notificationRestartOnBootReceiver = null;

            var receiverNodes = manifestXmlDoc.SelectNodes("manifest/application/receiver");
            if (receiverNodes != null)
            {
                // Check existing receivers.
                foreach (XmlNode node in receiverNodes)
                {
                    var element = node as XmlElement;
                    if (element == null)
                        continue;

                    var elementName = element.GetAttribute("name", kAndroidNamespaceURI);
                    if (elementName == kNotificationManagerName)
                        notificationManagerReceiver = element;
                    else if (elementName == kNotificationRestartOnBootName)
                        notificationRestartOnBootReceiver = element;

                    if (notificationManagerReceiver != null && notificationRestartOnBootReceiver != null)
                        break;
                }
            }

            // Create notification manager receiver if necessary.
            if (notificationManagerReceiver == null)
            {
                notificationManagerReceiver = manifestXmlDoc.CreateElement("receiver");
                notificationManagerReceiver.SetAttribute("name", kAndroidNamespaceURI, kNotificationManagerName);

                applicationXmlNode.AppendChild(notificationManagerReceiver);
            }
            notificationManagerReceiver.SetAttribute("exported", kAndroidNamespaceURI, "true");

            // Create notification restart-on-boot receiver if necessary.
            if (notificationRestartOnBootReceiver == null)
            {
                notificationRestartOnBootReceiver = manifestXmlDoc.CreateElement("receiver");
                notificationRestartOnBootReceiver.SetAttribute("name", kAndroidNamespaceURI, kNotificationRestartOnBootName);

                var intentFilterNode = manifestXmlDoc.CreateElement("intent-filter");

                var actionNode = manifestXmlDoc.CreateElement("action");
                actionNode.SetAttribute("name", kAndroidNamespaceURI, "android.intent.action.BOOT_COMPLETED");

                intentFilterNode.AppendChild(actionNode);
                notificationRestartOnBootReceiver.AppendChild(intentFilterNode);
                applicationXmlNode.AppendChild(notificationRestartOnBootReceiver);
            }
            notificationRestartOnBootReceiver.SetAttribute("enabled", kAndroidNamespaceURI, "false");
            */
        }

        internal static void AppendAndroidFeatureField(string manifestPath, XmlDocument xmlDoc, string name)
        {
            AppendAndroidToplevelOptionField("uses-feature", manifestPath, xmlDoc, name);
        }
        internal static void AppendAndroidPermissionField(string manifestPath, XmlDocument xmlDoc, string name)
        {
            AppendAndroidToplevelOptionField("uses-permission", manifestPath, xmlDoc, name);
        }

        internal static void AppendAndroidToplevelOptionField(string elementTag, string manifestPath, XmlDocument xmlDoc, string name)
        {
            var manifestNode = xmlDoc.SelectSingleNode("manifest");
            if (manifestNode == null)
                throw new ArgumentException(string.Format("Missing 'manifest' node in '{0}'.", manifestPath));

            foreach (XmlNode node in manifestNode.ChildNodes)
            {
                if (!(node is XmlElement) || node.Name != elementTag)
                    continue;

                var elementName = ((XmlElement)node).GetAttribute("name", kAndroidNamespaceURI);
                if (elementName == name)
                    return;
            }

            XmlElement metaDataNode = xmlDoc.CreateElement(elementTag);
            metaDataNode.SetAttribute("name", kAndroidNamespaceURI, name);

            manifestNode.AppendChild(metaDataNode);
        }

        internal static void AppendAndroidMetadataField(string manifestPath, XmlDocument xmlDoc, string name, string value)
        {
            var applicationNode = xmlDoc.SelectSingleNode("manifest/application");
            if (applicationNode == null)
                throw new ArgumentException(string.Format("Missing 'application' node in '{0}'.", manifestPath));

            var nodes = xmlDoc.SelectNodes("manifest/application/meta-data");
            if (nodes != null)
            {
                // Check if there is a 'meta-data' with the same name.
                foreach (XmlNode node in nodes)
                {
                    var element = node as XmlElement;
                    if (element == null)
                        continue;

                    var elementName = element.GetAttribute("name", kAndroidNamespaceURI);
                    if (elementName == name)
                    {
                        element.SetAttribute("value", kAndroidNamespaceURI, value);
                        return;
                    }
                }
            }

            XmlElement metaDataNode = xmlDoc.CreateElement("meta-data");
            metaDataNode.SetAttribute("name", kAndroidNamespaceURI, name);
            metaDataNode.SetAttribute("value", kAndroidNamespaceURI, value);

            applicationNode.AppendChild(metaDataNode);
        }
    }
}
#endif
