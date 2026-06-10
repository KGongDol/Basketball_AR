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

#pragma warning disable 649

namespace MakakaGames.Publisher.ArrowDirectional
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class ArrowDirectionalControl : MonoBehaviour
    {
        [SerializeField]
        public Transform cameraMain;
        
        [SerializeField]
        private Transform pivot;

        [SerializeField]
        private Image image;
        
        [SerializeField]
        public Transform target;

        private Vector3 direction;
        private Vector3 directionLocalEulerAngles = new(0f, 180f, 0f);

        private void OnEnable()
        {
            image.enabled = false;
        }

        private void Update()
        {
            SetArrowDirection();
        }

        private void SetArrowDirection()
        {
            if (target && pivot && cameraMain)
            {
                direction = cameraMain.InverseTransformPoint(target.position);
                
                directionLocalEulerAngles.z =
                    Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg
                        + directionLocalEulerAngles.y;
                
                pivot.localEulerAngles = directionLocalEulerAngles;

                // To Avoid Showing the Arrow before Direction was set
                if (!image.enabled)
                {
                    image.enabled = true;                         
                }
            }
        }

    }
}