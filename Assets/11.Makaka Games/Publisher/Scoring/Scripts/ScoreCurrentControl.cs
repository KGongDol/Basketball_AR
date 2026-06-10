/*
================================
Assets for Unity by Makaka Games
================================
 
[Online  Docs -> Updated]: https://makaka.org/unity-assets
[Offline Docs - PDF file]: find it in the package folder.

[Support]: https://makaka.org/support

Copyright © 2025 Andrey Sirota (Makaka Games)
*/

using System.Collections;

using UnityEngine;

using TMPro;

using MakakaGames.Publisher.Debugging;

#pragma warning disable 649

namespace MakakaGames.Publisher.Scoring
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class ScoreCurrentControl : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI text;
        private int value = 0;

        [SerializeField]
        private int valueAtStart = 0;

        [SerializeField]
        private float posYExtraForPortraitOrientation = 0;
        
        [SerializeField]
        [Header("Animation")]
        private Animator animator;
        
        [SerializeField]
        [Range(0f, 20f)]
        private float resetAnimationDelay = 0f;
        private string resetAnimationTrigger = "Reset";

        private void Start()
        {
            SetValue(valueAtStart);

            if (Input.deviceOrientation == DeviceOrientation.Portrait
                || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
            {
                RectTransform rectTransform = (RectTransform)text.transform;

                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition += 
                        new Vector2(0, posYExtraForPortraitOrientation);

                    DebugPrinter.Print("ADDED: Extra Value for PosY of Current"
                        + " Score for Portrait Orientation");
                }
            }
        }

        private void SetValue(int score)
        {   
            this.value = score;
            
            text.text = this.value.ToString();
        }

        public void Add(int value)
        {   
            this.value += value;

            text.text = this.value.ToString();
        }

        public void Reset()
        {   
            value = 0;

            text.text = value.ToString();

            PlayResetAnimation();
        }

        public int GetValue()
        {
            return value;
        }

        private void PlayResetAnimation()
        {
            StartCoroutine(PlayResetAnimationCoroutine(resetAnimationDelay));
        }

        private IEnumerator PlayResetAnimationCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            animator.SetTrigger(resetAnimationTrigger);
        }
    }
}