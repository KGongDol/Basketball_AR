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

namespace MakakaGames.Publisher.Debugging
{
    public static class DebugPrinter
    {
        public static void Print(object message)
        {
            if (IsDebugLogging)
            {
                Debug.Log(message);
            }
        }

        public static bool IsDebugLogging
        {
            get
            {

    #if DEVELOPMENT_BUILD || UNITY_EDITOR

                return true;

    #else

                return false;

    #endif

            }
        }

        public static void PrintWarning(object message)
        {
            if (IsDebugLogging)
            {
                Debug.LogWarning(message);
            }
        }

        public static void PrintError(object message)
        {
            if (IsDebugLogging)
            {
                Debug.LogError(message);
            }
        }

        public static void PrintArray(object[] objects)
        {
            if (IsDebugLogging)
            {
                for (int i = 0; i < objects.Length; i++)
                {
                    DebugPrinter.Print(objects[i]);
                }
            }
        }

    }
}
