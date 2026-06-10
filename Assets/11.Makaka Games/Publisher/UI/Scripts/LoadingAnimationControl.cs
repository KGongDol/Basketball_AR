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
using UnityEngine.UI;

namespace MakakaGames.Publisher.UI
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class LoadingAnimationControl : MonoBehaviour
    {
        [SerializeField]
        private RectTransform fillAreaTransform;

        [SerializeField]
        private Image fillArea;

        [Header("Speed")]
        [SerializeField]
        private float rotationSpeed = 200f;

        [SerializeField]
        private float openSpeed = 0.005f;

        [SerializeField]
        private float closeSpeed = 0.01f;

        [Header("Size")]
        [SerializeField]
        private float sizeOnTop = 0.30f;

        [SerializeField]
        private float sizeOnBottom = 0.02f;

        private float fillAreaCurrentSize;

        private bool isFillAreaOnTop = true;

        private void Update()
        {
            fillAreaTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

            ChangeFillAreaSize();
        }

        private void ChangeFillAreaSize()
        {
            fillAreaCurrentSize = fillArea.fillAmount;

            if (fillAreaCurrentSize < sizeOnTop && isFillAreaOnTop)
            {
                fillArea.fillAmount += openSpeed;
            }
            else if (fillAreaCurrentSize >= sizeOnTop && isFillAreaOnTop)
            {
                isFillAreaOnTop = false;
            }
            else if (fillAreaCurrentSize >= sizeOnBottom && !isFillAreaOnTop)
            {
                fillArea.fillAmount -= closeSpeed;
            }
            else if (fillAreaCurrentSize < sizeOnBottom && !isFillAreaOnTop)
            {
                isFillAreaOnTop = true;
            }
        }

    }
}