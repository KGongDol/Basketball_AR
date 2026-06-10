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
using MakakaGames.Publisher.Debugging;
using MakakaGames.Publisher.TagSelectorPropertyDrawerX;

#pragma warning disable 649

namespace MakakaGames.ThrowControlX
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class FailControl : MonoBehaviour
    {
        public int failMaterialIndex = 0;

        [Tooltip("To prevent failing several times.")]
        [SerializeField]
        private SameGameObjectDetector failingSeveralTimesDetector;
        private GameObject currentObject;

        [Tooltip("To Prevent Failing after Scoring.")]
        [SerializeField]
        private SameGameObjectDetector failingAfterScoringDetector;

        [SerializeField]
        private SameGameObjectDetector collisionForOutsideDetector;

        [Header("Tag For Collision Detection")]
        [SerializeField]
        private bool isTagCustomUsedForCollisionDetection = false;

        [TagSelector]
        [SerializeField]
        private string tagCustomForCollisionDetection =
            TagSelectorAttribute.Untagged;

        [Header("Audio")]
        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private AudioClip[] failSounds;

        private event Action OnInitialized;
        private event Action<GameObject> OnFailed;
        private event Action<GameObject> OnCollision;

        public void Init(
            int countOfObjectsForInteraction,
            Action OnInitialized,
            Action<GameObject> OnCollision,
            Action<GameObject> OnFailed)
        {
            this.OnInitialized += OnInitialized;
            this.OnCollision += OnCollision;
            this.OnFailed += OnFailed;

            failingSeveralTimesDetector.Init(
                countOfObjectsForInteraction,
                () =>
                    InitFailingAfterScoringDetector(countOfObjectsForInteraction));
        }

        private void InitFailingAfterScoringDetector(
            int countOfObjectsForInteraction)
        {
            failingAfterScoringDetector.Init(
                countOfObjectsForInteraction,
                () =>
                    InitCollisionForOutsideDetector(countOfObjectsForInteraction));
        }

        private void InitCollisionForOutsideDetector(
            int countOfObjectsForInteraction)
        {
            collisionForOutsideDetector.Init(
                countOfObjectsForInteraction, InitBase);
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

            if (OnCollision != null)
            {
                if (collisionForOutsideDetector.DetectOrRegister(currentObject)
                    == false)
                {
                    OnCollision.Invoke(currentObject);
                }
            }

            //DebugPrinter.Print("Collision1 from: " + currentObject);

            if (failingSeveralTimesDetector.DetectOrRegister(currentObject)
                == false)
            {
                //DebugPrinter.Print("Collision2 from: " + currentObject);

                if (failingAfterScoringDetector.DetectOrRegister(currentObject)
                    == false)
                {
                    //DebugPrinter.Print("Collision3 from: " + currentObject);

                    PlayFailSound();

                    if (OnFailed != null)
                    {
                        //DebugPrinter.Print("Fail");

                        OnFailed.Invoke(currentObject);
                    }
                }
            }
        }

        private void PlayFailSound()
        {
            if (failSounds.Length > 0)
            {
                audioSource.PlayOneShot(
                    failSounds[UnityEngine.Random.Range(0, failSounds.Length)]);
            }
            else
            {
                DebugPrinter.Print("No Fail Sounds!");
            }
        }

        /// <summary>
        ///  To Register Events happening outside.
        /// </summary>
        public void RegisterAfterScoring(GameObject scoredObject)
        {
            //DebugPrinter.Print("RegisterAfterScoring");

            failingAfterScoringDetector.DetectOrRegister(scoredObject);
        }
    }
}