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

namespace MakakaGames.Publisher.UI
{
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private Animator animator;

        [Space]
        [SerializeField]
        private UnityEvent OnInitialized;

        private readonly string animationTriggerNameFadeIn = "FadeIn";
        private readonly string animationTriggerNameFadeInLongStart =
            "FadeInLongStart";

        private readonly string animationTriggerNameFadeOutFastStart =
            "FadeOutFastStart";

        private void Awake()
        {
            OnInitialized.Invoke();
        }

        public void FadeIn()
        {
            animator.SetTrigger(animationTriggerNameFadeIn);
        }

        public void FadeInLongStart()
        {
            animator.SetTrigger(animationTriggerNameFadeInLongStart);
        }

        public void FadeOutFastStart()
        {
            animator.SetTrigger(animationTriggerNameFadeOutFastStart);
        }

        public void SetSortingOrder(int order)
        {
            canvas.sortingOrder = order;
        }
    }
}