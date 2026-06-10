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

namespace MakakaGames.Publisher.PlayerPrefsX
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class PlayerPrefsXControl : MonoBehaviour
    {
        public void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}
