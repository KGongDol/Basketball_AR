/*
================================
Assets for Unity by Makaka Games
================================
 
[Online  Docs -> Updated]: https://makaka.org/unity-assets
[Offline Docs - PDF file]: find it in the package folder.

[Support]: https://makaka.org/support

Copyright © 2025 Andrey Sirota (Makaka Games)
*/

#if UNITY_EDITOR

using UnityEngine;

namespace MakakaGames.Publisher.AR.ARFoundationBase.UX.Common
{
    /// <summary>
    /// To restart AR Scene in XR Simulation correctly.
    /// </summary>
    public class SceneUtilitySetup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Setup()
        {
            var gameObject = new GameObject("SceneUtility");
            gameObject.AddComponent<SceneUtility>();
        }
    }
}

#endif