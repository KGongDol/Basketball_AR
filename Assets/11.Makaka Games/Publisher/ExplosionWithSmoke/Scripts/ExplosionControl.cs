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

#pragma warning disable 649

namespace MakakaGames.Publisher.ExplosionWithSmoke
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class ExplosionControl : MonoBehaviour
    {
        [SerializeField]
        private GameObject explosion;

        [SerializeField]
        private Transform pivot;

        [SerializeField]
        private ParticleSystem particleSystemCustom;

        [SerializeField]
        private float delayBeforeShowing = 0f;

        private Coroutine showCoroutineReference;

        private void Awake()
        {
            explosion.SetActive(false);
        }

        public void Show()
        {
            if (showCoroutineReference != null)
            {
                StopCoroutine(showCoroutineReference);

                showCoroutineReference = null;
            }

            showCoroutineReference =
                StartCoroutine(ShowCoroutine());
        }

        private IEnumerator ShowCoroutine()
        {
            explosion.transform.SetPositionAndRotation(
                pivot.position, pivot.rotation);

            yield return new WaitForSeconds(delayBeforeShowing);

            if (!explosion.activeSelf)
            {
                explosion.SetActive(true);
            }

            particleSystemCustom.Play();
        }
    }
}