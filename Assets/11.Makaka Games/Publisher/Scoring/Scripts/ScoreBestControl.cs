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

using TMPro;

using MakakaGames.Publisher.SceneManagement;

#pragma warning disable 649

namespace MakakaGames.Publisher.Scoring
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class ScoreBestControl : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI text;
        private int value = 0;
        private string playerPrefsKey = "ScoreBest";

        [SerializeField]
        private int valueAtFirstStart = 3;

        [SerializeField]
        private bool isValueSceneRelated;

        [Header("Animation")]
        [SerializeField]
        private Animator animator;

        [SerializeField]
        [Range(0f, 20f)]
        private float animationDelay = 0.4f;
        private readonly string animationTrigger = "NewScoreBest";

        [Header("Audio")]
        [SerializeField]
        [Range(0f, 20f)]
        private float soundDelay = 0f;

        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private AudioClip[] sounds;

        private void Start()
        {
            if (isValueSceneRelated)
            {
                playerPrefsKey += "_" + SceneControl.GetActiveSceneName();
            }

            if (PlayerPrefs.HasKey(playerPrefsKey))
            {
                SetValue(PlayerPrefs.GetInt(playerPrefsKey));
            }
            else
            {
                SetValue(valueAtFirstStart);
            }
        }

        private void SetValue(int value)
        {
            this.value = value;

            text.text = this.value.ToString();
        }

        public int GetValue()
        {
            return value;
        }

        private void PlayBestScoreSound()
        {
            StartCoroutine(PlayBestScoreSoundCoroutine(soundDelay));
        }

        private void PlayBestScoreAnimation()
        {
            StartCoroutine(PlayBestScoreAnimationCoroutine(animationDelay));
        }

        private IEnumerator PlayBestScoreSoundCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            audioSource.PlayOneShot(
                sounds[UnityEngine.Random.Range(0, sounds.Length)]);
        }

        private IEnumerator PlayBestScoreAnimationCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            animator.SetTrigger(animationTrigger);
        }

        public void SaveAndShow(int value)
        {
            PlayerPrefs.SetInt(playerPrefsKey, value);

            PlayBestScoreSound();

            SetValue(value);

            PlayBestScoreAnimation();
        }
    }
}