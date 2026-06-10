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
using UnityEngine.Events;

namespace MakakaGames.Publisher.AR.ARFoundationBase.PlaneDetection
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class ARPlayerControl : MonoBehaviour
    {
        [SerializeField]
        private GameObject safeZone;

        internal bool isOutOfSafeZone = false;
        private bool isFirstEnterToSafeZone = true;

        [Space]
        [SerializeField]
        private UnityEvent OnSafeZoneEnter;

        [SerializeField]
        private UnityEvent OnSafeZoneExit;

        public static ARPlayerControl Current;

        private void Awake()
        {
            Current = this;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == safeZone)
            {
                if (isFirstEnterToSafeZone)
                {
                    isFirstEnterToSafeZone = false;
                }
                else
                {
                    isOutOfSafeZone = false;

                    OnSafeZoneEnter.Invoke();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == safeZone)
            {
                isOutOfSafeZone = true;

                OnSafeZoneExit.Invoke();
            }
        }
    }
}