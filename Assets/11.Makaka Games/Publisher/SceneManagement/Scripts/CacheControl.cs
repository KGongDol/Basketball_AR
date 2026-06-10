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

using System.Collections;

namespace MakakaGames.Publisher.SceneManagement
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class CacheControl : MonoBehaviour
    {
        [SerializeField]
        private float deactivationDelay = 0.1f;

        [Space]
        [SerializeField]
        private bool isParentChangingOn = false;

        [SerializeField]
        private Transform parentOnCompleting;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(deactivationDelay);

            if (isParentChangingOn)
            {
                transform.parent = parentOnCompleting;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}