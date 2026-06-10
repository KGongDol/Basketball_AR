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

using MakakaGames.Publisher.Debugging;

namespace MakakaGames.Publisher.SceneManagement
{
    public class DontDestroyOnLoadCustom : MonoBehaviour
    {
        static bool isLoaded;

        void Awake() 
        {
            DebugPrinter.Print("Back Button isLoaded=" + isLoaded);

            if (!isLoaded)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);  
            }
    
            isLoaded = true;
        }
    }
}