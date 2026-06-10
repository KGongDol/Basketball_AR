/*
================================
Assets for Unity by Makaka Games
================================
 
[Online  Docs -> Updated]: https://makaka.org/unity-assets
[Offline Docs - PDF file]: find it in the package folder.

[Support]: https://makaka.org/support

Copyright © 2025 Andrey Sirota (Makaka Games)
*/

using UnityEngine;
using UnityEngine.Events;

using TMPro;

using System.Threading.Tasks;

using MakakaGames.Publisher.Debugging;

#pragma warning disable 649

namespace MakakaGames.Publisher.Permissions.Camera
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class PermissionCameraControl : MonoBehaviour
    {

    #if UNITY_EDITOR

        [SerializeField]
        private bool isPermissionDeniedInEditorTesting = false;

    #endif

        [Space]
        [SerializeField]
        private UnityEvent OnPermissionsGranted = null;

        [Header("Tutorial is shown for iOS by Default")]
        [Space]
        [SerializeField]
        TextMeshProUGUI textTutorial;

        [SerializeField]
        private GameObject iconsTutorialIOS;

        [Space]
        [SerializeField]
        [TextArea(3, 9)]
        private string messageTutorialAndroid;

        private bool isPermissionGranted = false;

        private bool isPermissionChecked = false;

    #if NATIVE_CAMERA_PLUGIN_EXISTS

        private static bool isPicturePermissionOnAndroid = false;

    #else

        private static readonly string messageErrorOfInstallation =
            "Correct Version of" +
            " Native Camera is NOT INSTALLED — Learn PDF-file in the package" +
            " folder or the <a href=\"https://makaka.org/unity-assets\">" +
            "Latest Docs Online</a>. Find a Target Asset by Makaka Games:" +
            " <a href=\"https://makaka.org/unity-assets\">" +
            "https://makaka.org/unity-assets</a>";

        private static readonly string messageErrorOfInstallationTMP =
            "Correct Version of Native Camera is NOT Installed. Check Docs of" + 
            " Target Asset by Makaka Games in folder or web:" +
            " <color=#FFE100><link=\"https://makaka.org/unity-assets\">" +
            "https://makaka.org/unity-assets</link></color>";

        private static readonly string textButtonOnErrorOfInsallation = "Open Docs";

    #endif

        [Header("Button")]
        [Space]
        [SerializeField]
        TextMeshProUGUI textButton;

        /// <summary>
        /// App doesn't enter here When OnApplicationFocus() causes Scene Loading
        /// </summary>
        private void Start()
        {
            DebugPrinter.Print("Camera Permission Scene — Start()");

            CheckPermissionOrRequest();


    #if UNITY_ANDROID

            textTutorial.text = messageTutorialAndroid;

            iconsTutorialIOS.SetActive(false);

    #endif

    #if !NATIVE_CAMERA_PLUGIN_EXISTS

            DebugPrinter.PrintError(messageErrorOfInstallation);

            textTutorial.text = messageErrorOfInstallationTMP;

            textButton.text = textButtonOnErrorOfInsallation; 

    #endif

        }

    #if UNITY_ANDROID

        /// <summary>
        /// <para>
        /// When App Settings are changed outside the App (Phone Settings) to "Deny" 
        /// then App will be terminated and Restarted when the user
        /// will switch to it (OnApplicationFocus is not calling).
        /// </para>
        /// <para>
        /// If "Allow" will set on Android then App Session will be continued.
        /// </para>
        /// </summary>
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                // Program enters here When App Start:
                // iOS - No
                // Android — Yes

                DebugPrinter.Print("Permissions Scene — OnApplicationFocus(true)");

                CheckPermissionOrRequest();
            }
        }

    #endif

        private async void CheckPermissionOrRequest()
        {

    #if UNITY_EDITOR

            if (isPermissionDeniedInEditorTesting)
            {
                return;
            }

    #endif

            if (!isPermissionChecked)
            {
                isPermissionChecked = true;

                isPermissionGranted = await CheckPermissionOrRequestBaseAsync();

                if (isPermissionGranted)
                {
                    OnPermissionsGranted?.Invoke();
                }

                isPermissionChecked = false;
            }
        }

        /// <summary>
        /// Android call contains 2 requests:
        /// Camera, Storage.
        /// </summary> 

        public async static Task<bool> CheckPermissionOrRequestBaseAsync()
        {

    #if NATIVE_CAMERA_PLUGIN_EXISTS

            NativeCamera.Permission resultOfCheckPermission = await
                NativeCamera.RequestPermissionAsync(isPicturePermissionOnAndroid);

            switch (resultOfCheckPermission)
            {
                case NativeCamera.Permission.Granted:

                    DebugPrinter.Print("Camera Permission — Granted Previously");

                    return true;

                case NativeCamera.Permission.ShouldAsk:

                    do
                    {
                        resultOfCheckPermission = await
                            NativeCamera.RequestPermissionAsync(isPicturePermissionOnAndroid);

                        DebugPrinter.Print("Camera Permission — Request...");
                    }
                    while (resultOfCheckPermission
                        == NativeCamera.Permission.ShouldAsk);

                    if (resultOfCheckPermission
                        == NativeCamera.Permission.Granted)
                    {
                        DebugPrinter.Print("Camera Permission — Granted");

                        return true;
                    }
                    else
                    {
                        DebugPrinter.Print("Camera Permission — Not Granted");

                        return false;
                    }

                case NativeCamera.Permission.Denied:

                    DebugPrinter.Print("Camera Permission — Denied");

                    return false;

                default:

                    DebugPrinter.Print("Camera Permission — False by Defult");

                    return false;
            }

    #else

            await Task.CompletedTask; 

            return false;

    #endif

        }

        public static void OpenSettingBase()
        {

    #if NATIVE_CAMERA_PLUGIN_EXISTS

            DebugPrinter.Print("OpenSetting() called.");
            
            NativeCamera.OpenSettings();

    #else

            Application.OpenURL("https://makaka.org/unity-assets");
        
            DebugPrinter.PrintError(messageErrorOfInstallation);

    #endif

        }

        public void OpenSetting()
        {
            OpenSettingBase();
        }
    }
}