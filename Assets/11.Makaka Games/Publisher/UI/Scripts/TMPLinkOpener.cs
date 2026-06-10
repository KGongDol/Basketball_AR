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
using UnityEngine.EventSystems;

using TMPro;

namespace MakakaGames.Publisher.UI
{
    [HelpURL("https://makaka.org/unity-assets")]
    [RequireComponent(typeof(TMP_Text))]
    public class TMPLinkOpener : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private TMP_Text tmpText = null;

        private int linkIndex;

        private TMP_LinkInfo linkInfo;

        public void OnPointerClick(PointerEventData eventData)
        {
            // If you are not in a Canvas using Screen Overlay,
            // put your camera instead of null

            // This is the camera the raycaster used – works for
            // Screen-Space *and* World-Space canvases.
            Camera cam = eventData.pressEventCamera;

            // NOTE: eventData.position is already in screen coordinates.
            linkIndex = TMP_TextUtilities.FindIntersectingLink(
                tmpText,
                eventData.position,
                cam);

            if (linkIndex != -1) // was a link clicked?
            { 
                linkInfo = tmpText.textInfo.linkInfo[linkIndex];

                Application.OpenURL(linkInfo.GetLinkID());
            }
        }

    }
}