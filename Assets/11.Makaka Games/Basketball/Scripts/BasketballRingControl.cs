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

using MakakaGames.Publisher.Debugging;

#pragma warning disable 649

namespace MakakaGames.Basketball
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class BasketballRingControl : MonoBehaviour
    {
        private Vector3 localScaleAtStart;

        private Vector3 localPositionAtStart;

        [SerializeField]
        private Vector3 localPositionOnBigSize = new(0f, 9.51f, -1.9f);

        [Space]
        [SerializeField]
        private Cloth clothNetNormal;

        /// <summary>
        /// It's provided with Predefined Scale, which can't be changed.
        /// </summary>
        [SerializeField]
        private Cloth clothNetBig;

        private Cloth clothNetCurrent;

        private ClothSphereColliderPair[] clothSphereColliderPairs;

        private void Awake()
        {
            localScaleAtStart = transform.localScale;
            localPositionAtStart = transform.localPosition;
        }

        public void InitNet(int countOfThrowingObjects)
        {
            //https://docs.unity3d.com/ScriptReference/Cloth-sphereColliders.html
            if (countOfThrowingObjects > 32)
            {
                countOfThrowingObjects = 32;
            }

            clothNetCurrent = clothNetNormal;

            clothSphereColliderPairs =
                new ClothSphereColliderPair[countOfThrowingObjects];
        }

        public IEnumerator SetBigSize()
        {
            clothNetNormal.gameObject.SetActive(false);

            yield return new WaitForFixedUpdate();

            clothNetBig.gameObject.SetActive(true);

            gameObject.SetActive(false);

            yield return new WaitForFixedUpdate();

            transform.localPosition = localPositionOnBigSize;
            transform.localScale = clothNetBig.transform.localScale;

            yield return new WaitForFixedUpdate();

            gameObject.SetActive(true);

            yield return new WaitForFixedUpdate();

            clothNetBig.transform.SetParent(transform);

            clothNetCurrent = clothNetBig;
            clothNetCurrent.sphereColliders = clothSphereColliderPairs;
        }

        public IEnumerator SetNormalSize()
        {
            clothNetBig.transform.SetParent(transform.parent);

            yield return new WaitForFixedUpdate();

            clothNetBig.gameObject.SetActive(false);

            yield return new WaitForFixedUpdate();

            clothNetNormal.gameObject.SetActive(true);

            gameObject.SetActive(false);

            yield return new WaitForFixedUpdate();

            transform.localPosition = localPositionAtStart;
            transform.localScale = localScaleAtStart;

            yield return new WaitForFixedUpdate();

            gameObject.SetActive(true);

            yield return new WaitForFixedUpdate();

            clothNetCurrent = clothNetNormal;
            clothNetCurrent.sphereColliders = clothSphereColliderPairs;
        }

        public void RegisterSphereCollider(SphereCollider collider)
        {
            if (clothNetCurrent && clothSphereColliderPairs != null)
            {
                for (int i = 0; i < clothSphereColliderPairs.Length; i++)
                {
                    if (clothSphereColliderPairs[i].first == null
                        || !clothSphereColliderPairs[i]
                            .first.gameObject.activeInHierarchy)
                    {
                        clothSphereColliderPairs[i].first = collider;

                        // it can't be set once as a reference
                        clothNetCurrent.sphereColliders = clothSphereColliderPairs;

                        return;
                    }
                }
            }
            else
            {
                DebugPrinter.Print("Net in Not Initialized!");
            }
        }

        public void AnnulSphereCollider(SphereCollider collider)
        {
            if (clothNetCurrent && clothSphereColliderPairs != null)
            {
                for (int i = 0; i < clothSphereColliderPairs.Length; i++)
                {
                    if (clothSphereColliderPairs[i].first == collider)
                    {
                        clothSphereColliderPairs[i].first = null;

                        // it can't be set once as a reference
                        clothNetCurrent.sphereColliders = clothSphereColliderPairs;

                        return;
                    }
                }
            }
            else
            {
                DebugPrinter.Print("Net in Not Initialized!");
            }
        }
    }
}