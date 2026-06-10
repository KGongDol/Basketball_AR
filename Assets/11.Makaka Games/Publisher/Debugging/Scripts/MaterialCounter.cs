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
using UnityEngine.InputSystem;

#pragma warning disable 649

namespace MakakaGames.Publisher.Debugging
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class MaterialCounter : MonoBehaviour
    {
        private static readonly string messageByDefault = "Materials Count:";
        private static readonly string messageByDefaultOnDestroy =
            "Materials Count — Result:";

        [SerializeField]
        private Key keyPrint = Key.Q;

        [SerializeField]
        private bool isPrintedOnDestroy = true;

        private void Update()
        {
            if (Keyboard.current[keyPrint].wasPressedThisFrame)
            {
                Print();
            }
        }

        private void OnDestroy()
        {
            if (isPrintedOnDestroy)
            {
                Print(messageByDefaultOnDestroy);
            }
        }

        public static void Print(string messagge)
        {
            DebugPrinter.Print(messagge + Count());
        }

        public static void Print()
        {
            DebugPrinter.Print(messageByDefault + Count());
        }

        public static int Count()
        {
            return Resources.FindObjectsOfTypeAll(typeof(Material)).Length;
        }
    }
}