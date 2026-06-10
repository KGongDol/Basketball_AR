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

namespace MakakaGames.Publisher.Movement
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class ScaleControl : MonoBehaviour
    {
        public void ScaleLocalScale(Vector3 scale)
        {
            transform.localScale = Vector3.Scale(transform.localScale, scale);
        }

        public void ScaleLocalScaleX(float scaleX)
        {
            transform.localScale =
                Vector3.Scale(transform.localScale, new Vector3(scaleX, 1f, 1f));
        }

        public void ScaleLocalScaleY(float scaleY)
        {
            transform.localScale =
                Vector3.Scale(transform.localScale, new Vector3(1f, scaleY, 1f));
        }

        public void ScaleLocalScaleZ(float scaleZ)
        {
            transform.localScale =
                Vector3.Scale(transform.localScale, new Vector3(1f, 1f, scaleZ));
        }
    }
}