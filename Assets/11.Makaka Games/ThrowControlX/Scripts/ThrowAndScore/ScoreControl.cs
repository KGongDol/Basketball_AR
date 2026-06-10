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
using System.Collections;

using TMPro;

using MakakaGames.Publisher.Movement;
using MakakaGames.Publisher.Debugging;
using MakakaGames.Publisher.ExplosionWithSmoke;
using MakakaGames.Publisher.TagSelectorPropertyDrawerX;

#pragma warning disable 649

namespace MakakaGames.ThrowControlX
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class ScoreControl : MonoBehaviour
    {
        [SerializeField]
        private SameGameObjectDetector sameGameObjectDetector;
        private GameObject currentObject;
        
        [SerializeField]
        private ContainerControl containerControl;
        
        [SerializeField]
        private ExplosionControl explosionOnScoring;

        [Header("Tag For Triggering")]
        [SerializeField]
        private bool isTagCustomUsedForTriggering = false;

        [TagSelector]
        [SerializeField]
        private string tagCustomForTriggering = TagSelectorAttribute.Untagged;
        
        [Header("Text")]
        [SerializeField]
        private TextMeshPro scoreText;
        private int currentScore = 0;

        [SerializeField]
        private Animator scoreAnimator;

        [SerializeField]
        private float scoringAnimationDelay = 0f;
        private string scoringAnimationTrigger = "Scoring";
        
        [SerializeField]
        private float completeTaskAnimationDelay = 0f;
        private string completeTaskAnimationTrigger = "CompleteTask";

        [SerializeField]
        private float clearTaskAnimationDelay = 0f;
        private string clearTaskAnimationBool = "ClearTask";
        
        [SerializeField]
        private string messageOnTaskCompleted = "•";
        private bool isTaskCompleted  = true;

        [SerializeField]
        private string messageOnTaskCleared = "X";

        [SerializeField]
        private string messageOnStart = "";
        
        [Header("Audio")]
        [SerializeField]
        private AudioSource audioSource;
        
        [SerializeField]
        private AudioClip[] scoringSounds;

        private event Action OnInitialized;
        private event Action OnTaskCompleted;
        private event Action OnTaskCleared;
        private event Action<GameObject> OnScored;
        
        private void Start()
        {
            scoreText.text = messageOnStart;
        }

        public void Init(
            int countOfObjectsForInteraction, 
            Action OnInitialized,
            Action OnTaskCompleted,
            Action OnTaskCleared,
            Action<GameObject> OnScored, 
            Action<GameObject> OnCollisionSafe)
        {
            this.OnInitialized += OnInitialized;        
            this.OnTaskCompleted += OnTaskCompleted;
            this.OnTaskCleared += OnTaskCleared;
            this.OnScored += OnScored;
        
            sameGameObjectDetector.Init(
                countOfObjectsForInteraction,
                () => InitContainerControl(
                    OnCollisionSafe, countOfObjectsForInteraction));
        }

        private void InitContainerControl(
            Action<GameObject> OnCollisionSafe, int countOfObjectsForInteraction)
        {
            containerControl.Init(
                countOfObjectsForInteraction, InitBase, OnCollisionSafe);
        }

        private void InitBase()
        {
            if (OnInitialized != null)
            {
                OnInitialized.Invoke();
            }
        }

        public void SetTask(int score)
        {
            currentScore = score;
            scoreText.text = currentScore.ToString();

            isTaskCompleted = false;

            //DebugPrinter.Print("New Task was set !: " + score);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isTaskCompleted)
            {   
                currentObject = other.gameObject;

                if (isTagCustomUsedForTriggering
                    && !currentObject.CompareTag(tagCustomForTriggering))
                {
                    return;
                }

                if (sameGameObjectDetector.DetectOrRegister(currentObject) == false)
                {
                    Score();
                }
            }
        }

        private void Score()
        {
            if (OnScored != null)
            {
                OnScored.Invoke(currentObject);
            }

            audioSource.PlayOneShot(
                scoringSounds[UnityEngine.Random.Range(0, scoringSounds.Length)]);

            explosionOnScoring.Show();

            if (scoreAnimator)
            {
                PlayScoringAnimation();
            }

            if (currentScore > 1)
            {
                scoreText.text = (--currentScore).ToString();
            }
            else
            {
                CompleteTask();
            }
        }

        private void PlayClearTaskAnimation()
        {
            StartCoroutine(
                PlayClearTaskAnimationCoroutine(clearTaskAnimationDelay));
        }

        private IEnumerator PlayClearTaskAnimationCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            scoreAnimator.SetBool(clearTaskAnimationBool, true);

            yield return new WaitForSeconds(delay);

            scoreAnimator.SetBool(clearTaskAnimationBool, false);
        }

        private void PlayCompleteTaskAnimation()
        {
            StartCoroutine(
                PlayCompleteTaskAnimationCoroutine(completeTaskAnimationDelay));
        }

        private IEnumerator PlayCompleteTaskAnimationCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            scoreAnimator.SetTrigger(completeTaskAnimationTrigger);
        }
        
        private void PlayScoringAnimation()
        {
            StartCoroutine(PlayScoringAnimationCoroutine(scoringAnimationDelay));
        }

        private IEnumerator PlayScoringAnimationCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            scoreAnimator.SetTrigger(scoringAnimationTrigger);
        }

        private void CompleteTask()
        {
            //DebugPrinter.Print("Win for this Volume!");

            PlayCompleteTaskAnimation();

            scoreText.text = messageOnTaskCompleted;

            isTaskCompleted = true;

            if (OnTaskCompleted != null)
            {
                OnTaskCompleted.Invoke();
            }   
        }

        internal void ClearTask()
        {
            // if (!isTaskCompleted)
            // {
                PlayClearTaskAnimation();

                scoreText.text = messageOnTaskCleared;

                isTaskCompleted = true;

                if (OnTaskCleared != null)
                {
                    OnTaskCleared.Invoke();
                }   

                DebugPrinter.Print(
                    $"Task Cleared on {containerControl.name}");
            // }
        }
    }
}