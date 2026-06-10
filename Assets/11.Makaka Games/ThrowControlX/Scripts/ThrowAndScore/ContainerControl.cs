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

using System;

using MakakaGames.Publisher.Movement;
using MakakaGames.Publisher.TagSelectorPropertyDrawerX;

#pragma warning disable 649

namespace MakakaGames.ThrowControlX
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class ContainerControl : MonoBehaviour
    {
        [SerializeField]
        private SameGameObjectDetector sameGameObjectDetector;
        private GameObject currentObject;

        [Header("Tag For Triggering")]
        [SerializeField]
        private bool isTagCustomUsedForCollisionDetection = false;

        [TagSelector]
        [SerializeField]
        private string tagCustomForCollisionDetection =
            TagSelectorAttribute.Untagged;

        private event Action OnInitialized;
        private event Action<GameObject> OnCollisionSafe;
        
        public void Init(
            int countOfObjectsForInteraction,
            Action OnInitialized,
            Action<GameObject> OnCollisionSafe)
        {   
            this.OnInitialized += OnInitialized;
            this.OnCollisionSafe += OnCollisionSafe;

            sameGameObjectDetector.Init(countOfObjectsForInteraction, InitBase);
        }

        private void InitBase()
        {   
            if (OnInitialized != null)
            {
                OnInitialized.Invoke();
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            currentObject = other.gameObject;

            if (isTagCustomUsedForCollisionDetection
                && !currentObject.CompareTag(tagCustomForCollisionDetection))
            {
                return;
            }

            if (sameGameObjectDetector.DetectOrRegister(currentObject) == false)
            {
                if (OnCollisionSafe != null)
                {
                    OnCollisionSafe.Invoke(currentObject);
                }
            }
        }
    }
}